using System.Collections.Generic;

/// <summary>
/// [Domain Layer] - TimedBoxV3.Module
/// Đóng gói 100% toàn bộ logic Random/Claim quà của TimedBox.
/// </summary>
public interface ITimedBoxRewardService<TRewardData, TWeightData> where TWeightData : ITimedBoxWeightable<TRewardData>
{
    List<TimeBoxV3PoolRewardData<TRewardData, TWeightData>> GetResolvedPools(long seed, TimeBoxDefine type);
    List<TRewardData> ClaimRewards(long seed, TimeBoxDefine type);
    TimedBoxV3ProgressionDetail GetProgressionRewards(int playerLevel, TimeBoxDefine type);
}
