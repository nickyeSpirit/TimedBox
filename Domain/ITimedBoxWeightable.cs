using System.Collections.Generic;

public interface ITimedBoxWeightable<TRewardData>
{
    float GetWeight();
    TRewardData GetCurrentReward();
}
