using System.Collections.Generic;
using System.Linq;

public class TimedBoxRewardService : ITimedBoxRewardService 
{
    private readonly TimedBoxV3ConfigSO _config;

    public TimedBoxRewardService(TimedBoxV3ConfigSO config)
    {
        _config = config;
    }

    public List<TimeBoxV3PoolRewardData> GetResolvedPools(long seed, TimeBoxDefine type)
    {
        var rewardInfoSO = _config.GetTimedBoxV3Reward(type);
        if (rewardInfoSO == null) return new List<TimeBoxV3PoolRewardData>();
        
        var rewardInfo = rewardInfoSO as TimeBoxV3RewardSO;
        if (rewardInfo == null || rewardInfo.pool == null) return new List<TimeBoxV3PoolRewardData>();

        var priorityRandomizer = new System.Random((int)(seed & 0x7FFFFFFF));
        
        List<TimeBoxV3PoolRewardData> resolvedPools = new List<TimeBoxV3PoolRewardData>();
        List<TimeBoxV3PoolRewardData> priorityRewardList = new List<TimeBoxV3PoolRewardData>();

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

            TimeBoxV3PoolRewardData selected = null;
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

    public List<TimeBoxModuleRewardData> ClaimRewards(long seed, TimeBoxDefine type, int playerLevel)
    {
        var rewardInfoSO = _config.GetTimedBoxV3Reward(type);
        if (rewardInfoSO == null) return new List<TimeBoxModuleRewardData>();
        
        var rewardInfo = rewardInfoSO as TimeBoxV3RewardSO;
        if (rewardInfo == null || rewardInfo.pool == null) return new List<TimeBoxModuleRewardData>();

        var randomizer = new System.Random();
        List<TimeBoxModuleRewardData> listRewardsFinal = new List<TimeBoxModuleRewardData>();
        var resolvedPools = GetResolvedPools(seed, type);

        foreach (var p in resolvedPools)
        {
            var rw = p.RandomizeReward(randomizer);
            if (rw != null && rw.reward != null)
            {
                listRewardsFinal.Add(rw.reward);
            }
        }

        // Merge progression
        var progressionDetail = GetProgressionRewards(playerLevel, type);
        if (progressionDetail != null && progressionDetail.rewards != null)
        {
            foreach (var r in progressionDetail.rewards)
            {
                foreach (var rFinal in listRewardsFinal)
                {
                    if (rFinal.item != null && rFinal.item.infoId == r.itemID)
                    {
                        rFinal.item.quantity = r.amount;
                    }
                }
            }
        }

        listRewardsFinal = SortRewards(listRewardsFinal);

        return listRewardsFinal;
    }

    public TimedBoxV3ProgressionDetail GetProgressionRewards(int playerLevel, TimeBoxDefine type)
    {
        return TimedBoxV3Helper.GetTimedBoxV3ProgressionDetail(playerLevel, type, _config);
    }

    private List<TimeBoxModuleRewardData> SortRewards(List<TimeBoxModuleRewardData> rewards)
    {
        rewards.Sort((a, b) =>
        {
            return GetRewardSortOrder(a) - GetRewardSortOrder(b);
        });
        return rewards;
    }

    private int GetRewardSortOrder(TimeBoxModuleRewardData reward)
    {
        if (reward?.item == null) return 99;
        var id = reward.item.infoId;

        if (id == 21) return 0; // gold
        if (id >= 198 && id <= 202) return 1; // Random equipment 1-5 star
        if (id >= 203 && id <= 207) return 2; // Random drone 1-5 star
        if (id >= 346 && id <= 348) return 2; // Random drone shard 3-5
        if (id >= 342 && id <= 344) return 3; // Random ship shard

        return 4;
    }
}
