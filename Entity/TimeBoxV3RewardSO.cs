using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimeBoxV3PoolRewardData
{
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<TimeBoxModuleWeightData> rewards;
    
    public float percentPriority;

    public TimeBoxModuleWeightData RandomizeReward(System.Random randomizer = null)
    {
        if (rewards == null || rewards.Count == 0) return default;
        
        TimeBoxModuleWeightData selectedItem = default;
        float totalWeight = GetTotalWeight();
        
        float random = randomizer != null ? randomizer.Next(0, (int)totalWeight) : UnityEngine.Random.Range(0, totalWeight);
        foreach (var item in rewards)
        {
            random -= item.GetWeight();
            if (random <= 0)
            {
                selectedItem = item;
                break;
            }
        }

        return selectedItem;
    }

    public float GetTotalWeight()
    {
        float totalWeight = 0;
        foreach (var rw in rewards)
        {
            totalWeight += rw.GetWeight();
        }
        return totalWeight;
    }
}

[CreateAssetMenu(fileName = "TimeBoxV3RewardSO", menuName = "TimedBoxV3/TimeBoxV3RewardSO")]
public class TimeBoxV3RewardSO : ScriptableObject 
{
    public TimeBoxDefine boxType;

    public List<string> poolName;

    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<TimeBoxV3PoolRewardData> pool;
}