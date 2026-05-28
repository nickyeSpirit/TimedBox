using System.Collections.Generic;
using System.Linq;

public class TimedBoxRewardService<TRewardData, TWeightData> : ITimedBoxRewardService<TRewardData, TWeightData> 
    where TWeightData : ITimedBoxWeightable<TRewardData>
{
    private readonly TimedBoxV3ConfigSO _config;
    private readonly ITimedBoxRewardProcessor<TRewardData> _processor;

    public TimedBoxRewardService(TimedBoxV3ConfigSO config, ITimedBoxRewardProcessor<TRewardData> processor)
    {
        _config = config;
        _processor = processor;
    }

    public List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>> GetResolvedPools(long seed, TimeBoxDefine type)
    {
        var rewardInfoSO = _config.GetTimedBoxV3Reward(type);
        if (rewardInfoSO == null) return new List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>>();
        
        var rewardInfo = rewardInfoSO as TimeBoxV3RewardSOBase<TRewardData, TWeightData>;
        if (rewardInfo == null || rewardInfo.pool == null) return new List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>>();

        var priorityRandomizer = new System.Random((int)(seed & 0x7FFFFFFF));
        
        List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>> resolvedPools = new List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>>();
        List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>> priorityRewardList = new List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>>();

        foreach (var p in rewardInfo.pool)
        {
            if (p.percentPriority > 0.0001f)
                priorityRewardList.Add(p);
            else
                resolvedPools.Add(p);
        }

        if (priorityRewardList.Count > 0)
        {
            float totalPriority = 0f;
            foreach (var p in priorityRewardList) totalPriority += p.percentPriority;
            float rand = (float)priorityRandomizer.NextDouble() * totalPriority;

            TimeBoxV3PoolRewardData<TRewardData, TWeightData> selected = null;
            foreach (var p in priorityRewardList)
            {
                rand -= p.percentPriority;
                if (rand <= 0f)
                {
                    selected = p;
                    break;
                }
            }

            if (selected == null) selected = priorityRewardList[priorityRewardList.Count - 1];
            resolvedPools.Add(selected);
        }

        return resolvedPools;
    }

    public List<TRewardData> ClaimRewards(long seed, TimeBoxDefine type)
    {
        var rewardInfoSO = _config.GetTimedBoxV3Reward(type);
        if (rewardInfoSO == null) return new List<TRewardData>();
        
        var rewardInfo = rewardInfoSO as TimeBoxV3RewardSOBase<TRewardData, TWeightData>;
        if (rewardInfo == null || rewardInfo.pool == null) return new List<TRewardData>();

        var randomizer = new System.Random();
        List<TRewardData> listRewardsFinal = new List<TRewardData>();
        var resolvedPools = GetResolvedPools(seed, type);

        foreach (var p in resolvedPools)
        {
            var rw = p.RandomizeReward(randomizer);
            if (rw != null)
            {
                listRewardsFinal.Add(rw.GetCurrentReward());
            }
        }

        if (_processor != null)
        {
            _processor.MergeProgressionRewards(listRewardsFinal, type);
            listRewardsFinal = _processor.SortRewards(listRewardsFinal);
        }

        return listRewardsFinal;
    }

    public TimedBoxV3ProgressionDetail GetProgressionRewards(int playerLevel, TimeBoxDefine type)
    {
        return TimedBoxV3Helper.GetTimedBoxV3ProgressionDetail(playerLevel, type, _config);
    }
}
