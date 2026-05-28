using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using MonsterTrainer;
using UnityEngine.Serialization;
using Random = System.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Networking;
using System.Threading.Tasks;
#endif

[CreateAssetMenu(fileName = "TimedBoxV3SO", menuName = "Atlantis/TimedBoxV3/TimedBoxV3SO")]
public class TimedBoxV3ConfigSO : ScriptableObject
{
    public int cooldownTimebox;
    
    [TitleGroup("Điều chỉnh tỷ lệ drop timed box trong story mode")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<TimedBoxV3WeightByStage> customTimeboxWeight;
    
    [TitleGroup("Điều chỉnh tỷ lệ drop timed box trong league mode")]
    public TimedBoxV3WeightByStage customTimeboxWeightForLeagueMode;
    
    [TitleGroup("Chứa info reward số lượng shard & gold theo progression navy rank")]
    public DictionaryTimedBoxV3Progression timedBoxV3Progression;
    
    [TitleGroup("Chứa info reward bên trong timed box")]
    public DictionaryTimedBoxV3RewardSO timedBoxV3Reward;
    
    [TitleGroup("Điều chỉnh tỷ lệ behaviour cho monster timed box trong game play")]
    public DictionaryMonsterTimedBoxWeight monsterTimedBoxWeightWeight;
    public List<WaveTimedBoxWeightByStage> waveTimedBoxWeightByStage;     
    
    // Cache parsed from remote config (runtime only)
    [NonSerialized] private TimedBoxV3ProgressionRemote _remoteProgression;
    [NonSerialized] private TimedBoxV3CustomWeightRemote _remoteCustomWeightData;
    
    #region Function

    /// <summary> Trả về progression detail, ưu tiên remote config trước, fallback về SO asset./// </summary>
    public TimedBoxV3ProgressionDetail GetTimedBoxV3ProgressionDetail(int level, TimeBoxDefine type)
    {
        // Thử lấy từ remote config
        var remoteResult = GetProgressionFromRemoteConfig(level, type);
        if (remoteResult != null)
        {
            return remoteResult;
        }

        // Fallback: lấy từ SO asset
        TimedBoxV3ProgressionDetail result = null;
        if (timedBoxV3Progression.TryGetValue(level, out var progression))
        {
            if (progression.timedBoxV3ProgressionDetail.TryGetValue(type, out var info))
            {
                result = info;
            }
        }
        return result;
    }

    public void FetchTimedBoxRemoteConfig()
    {
        try
        {
            string json = FirebaseInit.GetValueRemoteConfig(FirebaseRemoteConfigKey.kTimedBoxV3Progression, enableCache: true);
            if (!string.IsNullOrEmpty(json))
            {
                _remoteProgression = JsonConvert.DeserializeObject<TimedBoxV3ProgressionRemote>(json);
            }
        }
        catch (Exception e)
        {
            _remoteProgression = null;
        }

        try
        {
            string jsonWeight = FirebaseInit.GetValueRemoteConfig(FirebaseRemoteConfigKey.kTimedBoxV3CustomWeight, enableCache: true);
            if (!string.IsNullOrEmpty(jsonWeight))
            {
                _remoteCustomWeightData = JsonConvert.DeserializeObject<TimedBoxV3CustomWeightRemote>(jsonWeight);
            }
        }
        catch (Exception e)
        {
            _remoteCustomWeightData = null;
        }
    }
    
    public List<TimedBoxV3WeightByStage> GetCustomTimeboxWeight()
    {
        if (_remoteCustomWeightData != null && _remoteCustomWeightData.storyMode != null && _remoteCustomWeightData.storyMode.Count > 0)
        {
            List<TimedBoxV3WeightByStage> result = new List<TimedBoxV3WeightByStage>();
            foreach (var stageData in _remoteCustomWeightData.storyMode)
            {
                var stageWeight = new TimedBoxV3WeightByStage() { toStage = stageData.toStage, weightInfo = new List<TimedBoxV3DropInfo>() };
                if (stageData.weightInfo != null)
                {
                    foreach (var info in stageData.weightInfo)
                    {
                        TimeBoxDefine type = ParseBoxType(info.boxType);
                        stageWeight.weightInfo.Add(new TimedBoxV3DropInfo(type, info.weight));
                    }
                }
                result.Add(stageWeight);
            }
            return result;
        }
        
        return customTimeboxWeight;
    }

    public List<TimedBoxV3WeightByStage> GetCustomTimeboxWeightForLeagueMode()
    {
        if (_remoteCustomWeightData != null && _remoteCustomWeightData.leagueMode != null && _remoteCustomWeightData.leagueMode.Count > 0)
        {
            var stageWeight = new TimedBoxV3WeightByStage() { toStage = 99999, weightInfo = new List<TimedBoxV3DropInfo>() };
            foreach (var info in _remoteCustomWeightData.leagueMode)
            {
                TimeBoxDefine type = ParseBoxType(info.boxType);
                stageWeight.weightInfo.Add(new TimedBoxV3DropInfo(type, info.weight));
            }
            return new List<TimedBoxV3WeightByStage>() { stageWeight };
        }

        return new List<TimedBoxV3WeightByStage>() { customTimeboxWeightForLeagueMode };
    }

    private static TimeBoxDefine ParseBoxType(string boxTypeStr)
    {
        if (string.Equals(boxTypeStr, "Legend", StringComparison.OrdinalIgnoreCase))
            return TimeBoxDefine.Legendary;
        if (Enum.TryParse<TimeBoxDefine>(boxTypeStr, true, out var result))
            return result;
        return TimeBoxDefine.None;
    }
    
    private TimedBoxV3ProgressionDetail GetProgressionFromRemoteConfig(int level, TimeBoxDefine type)
    {
        if (_remoteProgression?.levels == null) return null;

        // level 1-based, index 0-based
        int index = level - 1;
        if (index < 0 || index >= _remoteProgression.levels.Count) return null;

        var entry = _remoteProgression.levels[index];
        if (entry?.boxes == null) return null;

        string typeKey = type.ToString(); // e.g. "Legendary"
        foreach (var box in entry.boxes)
        {
            bool match = string.Equals(box.boxType, typeKey, StringComparison.OrdinalIgnoreCase)
                      || (typeKey == "Legendary" && string.Equals(box.boxType, "Legend", StringComparison.OrdinalIgnoreCase));
            if (match)
                return box.ToProgressionDetail();
        }
        return null;
    }
    public UnityEngine.ScriptableObject GetTimedBoxV3Reward(TimeBoxDefine type) => timedBoxV3Reward.GetValueOrDefault(type);

    #endregion
    
#if UNITY_EDITOR
    [TitleGroup("Tool")]
    [Button("Download Progression Data")]
    public void DownloadProgressionData()
    {
        _ = EditorDownload();
    }

    private async Task EditorDownload()
    {
        string url = "https://docs.google.com/spreadsheets/d/1EwJfpBzLGOp3wj4ec0hKuHzMvP9PMdlOTqvMigW6nc0/export?format=csv&gid=1208037215";
        using (var request = UnityWebRequest.Get(url))
        {
            var op = request.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseCSV(request.downloadHandler.text);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                Debug.Log("Download & Parse progression data success!");
            }
            else
            {
                Debug.LogError("Download failed: " + request.error);
            }
        }
    }

    private List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder currentElement = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentElement.ToString());
                currentElement.Clear();
            }
            else
            {
                currentElement.Append(c);
            }
        }
        result.Add(currentElement.ToString());
        return result;
    }

    private void ParseCSV(string csv)
    {
        timedBoxV3Progression = new DictionaryTimedBoxV3Progression();
        
        var lines = csv.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;
        
        // Parse Header (B1 is index 1)
        var headers = ParseCsvLine(lines[0]);
        List<TimeBoxDefine> columnTypes = new List<TimeBoxDefine>();
        for (int i = 0; i < headers.Count; i++)
        {
            string h = headers[i].Trim();
            if (h.Equals("Common", StringComparison.OrdinalIgnoreCase)) columnTypes.Add(TimeBoxDefine.Common);
            else if (h.Equals("Rare", StringComparison.OrdinalIgnoreCase)) columnTypes.Add(TimeBoxDefine.Rare);
            else if (h.Equals("Epic", StringComparison.OrdinalIgnoreCase)) columnTypes.Add(TimeBoxDefine.Epic);
            else if (h.Equals("Legend", StringComparison.OrdinalIgnoreCase) || h.Equals("Legendary", StringComparison.OrdinalIgnoreCase)) columnTypes.Add(TimeBoxDefine.Legendary);
            else columnTypes.Add(TimeBoxDefine.None);
        }

        // Parse Data rows
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = ParseCsvLine(lines[i]);
            for (int colIdx = 0; colIdx < cols.Count; colIdx++)
            {
                if (colIdx >= columnTypes.Count) break;
                TimeBoxDefine boxType = columnTypes[colIdx];
                if (boxType == TimeBoxDefine.None) continue;
                
                string cellData = cols[colIdx].Trim();
                if (string.IsNullOrEmpty(cellData)) continue;
                
                var items = cellData.Split(',');
                if (items.Length == 0) continue;
                
                int currentLevel = -1;
                List<RewardDetail> cellRewards = new List<RewardDetail>();
                
                for (int itemIdx = 0; itemIdx < items.Length; itemIdx++)
                {
                    var parts = items[itemIdx].Split(':');
                    if (itemIdx == 0)
                    {
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[0], out currentLevel);
                            int.TryParse(parts[1], out int itemId);
                            int.TryParse(parts[2], out int amount);
                            if (itemId == 0) amount *= 1000;
                            cellRewards.Add(new RewardDetail() { itemID = itemId, amount = amount });
                        }
                    }
                    else
                    {
                        if (parts.Length >= 2)
                        {
                            int.TryParse(parts[0], out int itemId);
                            int.TryParse(parts[1], out int amount);
                            if (itemId == 0) amount *= 1000;
                            cellRewards.Add(new RewardDetail() { itemID = itemId, amount = amount });
                        }
                    }
                }
                
                if (currentLevel != -1 && cellRewards.Count > 0)
                {
                    if (!timedBoxV3Progression.ContainsKey(currentLevel))
                    {
                        timedBoxV3Progression.Add(currentLevel, new TimedBoxV3Progression()
                        {
                            timedBoxV3ProgressionDetail = new DictionaryTimedBoxV3ProgressionDetail()
                        });
                    }
                    
                    var levelProgression = timedBoxV3Progression[currentLevel];
                    if (!levelProgression.timedBoxV3ProgressionDetail.ContainsKey(boxType))
                    {
                        levelProgression.timedBoxV3ProgressionDetail.Add(boxType, new TimedBoxV3ProgressionDetail() { rewards = new List<RewardDetail>() });
                    }
                    
                    levelProgression.timedBoxV3ProgressionDetail[boxType].rewards.AddRange(cellRewards);
                }
            }
        }
    }
#endif
}

[Serializable]
public class WaveTimedBoxWeightByStage
{
    public int toStage;
    public float minPercent;
    public float maxPercent;
}


[Serializable]
public class TimedBoxV3WeightByStage
{
    public int toStage;
    public List<TimedBoxV3DropInfo> weightInfo;
}

[Serializable]
public class TimedBoxV3DropInfo : MyWeightable
{
    public TimeBoxDefine boxType;

    public TimedBoxV3DropInfo(TimeBoxDefine boxType, float weight)
    {
        this.boxType = boxType;
        this.weight = weight;
    }
}

public enum MonsterTimeBoxType
{
    Bouncing,
    Dash,
    SWS,
}