public interface ITimedBoxV3Feature
{
    bool CanDropTimedBox(GameSetting.GamePlayMode gameMode);
    
    // Gameplay: Checks cooldown and stage, rolls drop box, updates timestamp, and returns box data.
    TimedBoxV3DTO GetReceiveTimedBoxData(GameSetting.GamePlayMode gameMode);

    // UI: Retrieves progression rewards for a given Navy Rank / Player level and box type.
    TimedBoxV3ProgressionDetail GetTimedBoxV3ProgressionDetail(int level, TimeBoxDefine type);

    UnityEngine.ScriptableObject GetTimedBoxV3RewardInfo(TimeBoxDefine type);
}
