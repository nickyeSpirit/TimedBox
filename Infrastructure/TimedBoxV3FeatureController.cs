using System;
using System.Collections.Generic;
using System.Linq;

public class TimedBoxV3FeatureController : ITimedBoxV3Feature
{
    private readonly ITimedBoxProgressionMapper _mapper;
    private readonly TimedBoxV3ConfigSO _config;
    private readonly IStorage _storage;

    private static TimedBoxV3DTO _pendingDrop;
    public static TimedBoxV3DTO PendingDrop => _pendingDrop;
    public static void ClearPendingDrop() => _pendingDrop = null;
    
    private static DateTime? _cachedLastClaimTime = null;
    private static float _nextReadyTime = -1f;
    
    public TimedBoxV3FeatureController(ITimedBoxProgressionMapper mapper, TimedBoxV3ConfigSO config, IStorage storage)
    {
        _mapper = mapper;
        _config = config;
        _storage = storage;
    }

    public bool CanDropTimedBox(GameSetting.GamePlayMode gameMode)
    {
        if (_nextReadyTime < 0f)
        {
            var strLastTimedClaimedTimedBox = _storage.GetString(TimedBoxV3Constant.LastTimeClaimTimebox);
            _cachedLastClaimTime = TimedBoxV3Helper.ConvertStringToDateTime(strLastTimedClaimedTimedBox);

            var now = DateTimeHelper.Instance.GetTimeNow();
            var targetTime = _cachedLastClaimTime.Value.AddSeconds(_config.cooldownTimebox);
            
            float remaining = (float)(targetTime - now).TotalSeconds;
            if (remaining < 0) remaining = 0;
            
            _nextReadyTime = UnityEngine.Time.time + remaining;
        }

        if (UnityEngine.Time.time < _nextReadyTime)
        {
            return false;
        }

        int selectedMap = _mapper.GetCurrentStoryStage();
        var customWeightList = _config.GetCustomTimeboxWeight();
        if (gameMode == GameSetting.GamePlayMode.Endless)
        {
            customWeightList = _config.GetCustomTimeboxWeightForLeagueMode();
        }

        if (customWeightList != null && customWeightList.Count > 0)
        {
            TimedBoxV3WeightByStage finalItem = null;

            var lastItem = customWeightList.Last();
            if (selectedMap > lastItem.toStage)
            {
                finalItem = lastItem;
            }
            else
            {
                finalItem = customWeightList.Find(x => x.toStage >= selectedMap);
            }

            if (finalItem != null && finalItem.weightInfo.Count > 0)
            {
                return finalItem.weightInfo.Any(w => w.weight > 0 && w.boxType != TimeBoxDefine.None);
            }
        }

        return false;
    }

    public TimedBoxV3DTO GetReceiveTimedBoxData(GameSetting.GamePlayMode gameMode)
    {
        int selectedMap = _mapper.GetCurrentStoryStage();

        var customWeightList = _config.GetCustomTimeboxWeight();
        if (gameMode == GameSetting.GamePlayMode.Endless)
        {
            customWeightList = _config.GetCustomTimeboxWeightForLeagueMode();
        }
        
        if (customWeightList != null && customWeightList.Count > 0)
        {
            TimedBoxV3WeightByStage finalItem = null;

            var lastItem = customWeightList.Last();
            if (selectedMap > lastItem.toStage)
            {
                finalItem = lastItem;
            }
            else
            {
                finalItem = customWeightList.Find(x => x.toStage >= selectedMap);
            }

            if (finalItem != null && finalItem.weightInfo.Count > 0)
            {
                List<TimedBoxV3DropInfo> listItem = new List<TimedBoxV3DropInfo>();
                listItem.AddRange(finalItem.weightInfo);
                
                TimedBoxV3DropInfo randomed = MyWeightable.Randomize<TimedBoxV3DropInfo>(listItem);
                if (randomed != null && randomed.boxType != TimeBoxDefine.None)
                {
                    // Save timestamp
                    var timeStr = DateTimeHelper.Instance.GetTimeNow().ToString(System.Globalization.CultureInfo.InvariantCulture);
                    _storage.SetString(TimedBoxV3Constant.LastTimeClaimTimebox, timeStr);
                    _cachedLastClaimTime = TimedBoxV3Helper.ConvertStringToDateTime(timeStr);
                    _nextReadyTime = UnityEngine.Time.time + _config.cooldownTimebox;

                    // Get rewards based on current player level
                    //int playerLevel = _mapper.GetCurrentPlayerLevel();
                    //var progressionDetail = GetTimedBoxV3ProgressionDetail(playerLevel, randomed.boxType);
                    
                    _pendingDrop = new TimedBoxV3DTO()
                    {
                        type = randomed.boxType,
                        //rewards = progressionDetail != null && progressionDetail.rewards != null ? progressionDetail.rewards.ToArray() : new RewardDetail[0]
                    };
                    return _pendingDrop;
                }
            }
        }

        return null;
    }

    public TimedBoxV3ProgressionDetail GetTimedBoxV3ProgressionDetail(int level, TimeBoxDefine type)
    {
        return TimedBoxV3Helper.GetTimedBoxV3ProgressionDetail(level, type, _config);
    }

    public UnityEngine.ScriptableObject GetTimedBoxV3RewardInfo(TimeBoxDefine type)
    {
        if (_config != null && _config.timedBoxV3Reward != null &&
            _config.timedBoxV3Reward.TryGetValue(type, out var rewardInfo))
        {
            return rewardInfo;
        }
        return null;
    }
}
