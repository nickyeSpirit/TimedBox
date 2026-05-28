public interface ITimedBoxV3Feature
{
    bool CanDropTimedBox(bool isEndlessMode);
    
    // Gameplay: Checks cooldown and stage, rolls drop box, updates timestamp, and returns box data.
    TimedBoxV3DTO GetReceiveTimedBoxData(bool isEndlessMode);

    // UI: Retrieves progression rewards for a given Navy Rank / Player level and box type.
    TimedBoxV3ProgressionDetail GetTimedBoxV3ProgressionDetail(int level, TimeBoxDefine type);

    TimeBoxV3RewardSO GetTimedBoxV3RewardInfo(TimeBoxDefine type);
}
