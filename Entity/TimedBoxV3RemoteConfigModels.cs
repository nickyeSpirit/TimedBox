using System;
using System.Collections.Generic;

// ---------------------------------------------------------------------------
// JSON models cho Firebase Remote Config key "TimedBoxV3Progression"
// ---------------------------------------------------------------------------

/// <summary>Root object của Firebase Remote Config key <c>TimedBoxV3Progression</c>.</summary>
[Serializable]
public class TimedBoxV3ProgressionRemote
{
    public List<TimedBoxV3ProgressionRemoteEntry> levels;
}

/// <summary>Một entry ứng với một level (vị trí trong mảng = level - 1).</summary>
[Serializable]
public class TimedBoxV3ProgressionRemoteEntry
{
    public List<TimedBoxV3ProgressionRemoteBox> boxes;
}

/// <summary>Reward của một loại box.</summary>
[Serializable]
public class TimedBoxV3ProgressionRemoteBox
{
    public string boxType;
    public string rewards;

    public TimedBoxV3ProgressionDetail ToProgressionDetail()
    {
        var detail = new TimedBoxV3ProgressionDetail { rewards = new List<RewardDetail>() };
        if (string.IsNullOrEmpty(rewards)) return detail;

        var items = rewards.Split(',');
        for (int itemIdx = 0; itemIdx < items.Length; itemIdx++)
        {
            var parts = items[itemIdx].Trim().Split(':');
            int itemId, amount;
            if (itemIdx == 0) // first token: "level:itemID:amount"
            {
                if (parts.Length < 3) continue;
                if (!int.TryParse(parts[1], out itemId)) continue;
                if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out float amountF)) continue;
                amount = itemId == 0 ? (int)(amountF * 1000f) : (int)amountF;
            }
            else // subsequent tokens: "itemID:amount"
            {
                if (parts.Length < 2) continue;
                if (!int.TryParse(parts[0], out itemId)) continue;
                if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out float amountF)) continue;
                amount = itemId == 0 ? (int)(amountF * 1000f) : (int)amountF;
            }
            detail.rewards.Add(new RewardDetail { itemID = (ItemInfo.ItemID)itemId, amount = amount });
        }
        return detail;
    }
}

// ---------------------------------------------------------------------------
// JSON models cho Firebase Remote Config key "TimedBoxV3CustomWeight"
// ---------------------------------------------------------------------------

[Serializable]
public class TimedBoxV3CustomWeightRemote
{
    public List<TimedBoxV3WeightByStageRemote> storyMode;
    public List<TimedBoxV3DropInfoRemote> leagueMode;
}

[Serializable]
public class TimedBoxV3WeightByStageRemote
{
    public int toStage;
    public List<TimedBoxV3DropInfoRemote> weightInfo;
}

[Serializable]
public class TimedBoxV3DropInfoRemote
{
    public string boxType;
    public float weight;
}
