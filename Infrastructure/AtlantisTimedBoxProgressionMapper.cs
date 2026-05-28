public class AtlantisTimedBoxProgressionMapper : ITimedBoxProgressionMapper
{
    public int GetCurrentPlayerLevel()
    {
        return NavyRankData.GetCurrentLevelBoxUnlockerMachine();
    }

    public int GetCurrentStoryStage()
    {
        return DataManager.Instance.storyModeData.CurrentMapSelected;
    }

    public static ItemInfo.ItemID MapBoxTypeToItemID(TimeBoxDefine type)
    {
        switch (type)
        {
            case TimeBoxDefine.Tutorial: return ItemInfo.ItemID.timedBoxTutorial;
            case TimeBoxDefine.Common: return ItemInfo.ItemID.timedBoxCommon;
            case TimeBoxDefine.Rare: return ItemInfo.ItemID.timedBoxRare;
            case TimeBoxDefine.Epic: return ItemInfo.ItemID.timedBoxEpic;
            case TimeBoxDefine.Legendary: return ItemInfo.ItemID.timedBoxLegendary;
            default: return ItemInfo.ItemID.none;
        }
    }

    public static TimeBoxDefine MapItemIDToBoxType(ItemInfo.ItemID itemId)
    {
        switch (itemId)
        {
            case ItemInfo.ItemID.timedBoxTutorial: return TimeBoxDefine.Tutorial;
            case ItemInfo.ItemID.timedBoxCommon: return TimeBoxDefine.Common;
            case ItemInfo.ItemID.timedBoxRare: return TimeBoxDefine.Rare;
            case ItemInfo.ItemID.timedBoxEpic: return TimeBoxDefine.Epic;
            case ItemInfo.ItemID.timedBoxLegendary: return TimeBoxDefine.Legendary;
            default: return TimeBoxDefine.None;
        }
    }
}
