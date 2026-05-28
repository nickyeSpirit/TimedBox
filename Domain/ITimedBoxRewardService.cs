using System.Collections.Generic;

/// <summary>
/// [Domain Layer] - TimedBoxV3.Module
/// Đóng gói 100% toàn bộ logic Random/Claim quà của TimedBox.
/// </summary>
public interface ITimedBoxRewardService
{
    List<TimeBoxV3PoolRewardData> GetResolvedPools(long seed, TimeBoxDefine type);
    List<TimeBoxModuleRewardData> ClaimRewards(long seed, TimeBoxDefine type, int playerLevel);
    TimedBoxV3ProgressionDetail GetProgressionRewards(int playerLevel, TimeBoxDefine type);
}
