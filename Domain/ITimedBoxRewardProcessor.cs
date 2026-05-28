using System.Collections.Generic;

public interface ITimedBoxRewardProcessor<TRewardData>
{
    List<TRewardData> SortRewards(List<TRewardData> rewards);
    void MergeProgressionRewards(List<TRewardData> targetList, TimeBoxDefine type);
}
