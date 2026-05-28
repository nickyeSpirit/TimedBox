using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimeBoxV3PoolRewardData<TRewardData, TWeightData> where TWeightData : ITimedBoxWeightable<TRewardData>
{
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<TWeightData> rewards;
    
    public float percentPriority;

    public TWeightData RandomizeReward(System.Random randomizer = null)
    {
        if (rewards == null || rewards.Count == 0) return default;
        
        TWeightData selectedItem = default;
        float totalWeight = GetTotalWeight();
        
        float random = randomizer != null ? randomizer.Next(0, (int)totalWeight) : UnityEngine.Random.Range(0, totalWeight);
        foreach (TWeightData item in rewards)
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

[Serializable]
public class TimeBoxPoolV3RewardData : TimeBoxV3PoolRewardData<MonsterTrainer.WSRewardData, AtlantisRewardTimeBoxV3WeightData> {}

public abstract class TimeBoxV3RewardSOBase<TRewardData, TWeightData> : ScriptableObject 
    where TWeightData : ITimedBoxWeightable<TRewardData>
{
    public TimeBoxDefine boxType;

    public List<string> poolName;

    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>> pool;
}