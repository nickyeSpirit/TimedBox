public static class TimedBoxV3EggIdMapper
{
    public static bool IsLegacyTimedBoxEggId(ItemInfo.ItemID eggId)
    {
        return eggId == ItemInfo.ItemID.newbieBox_1
               || eggId == ItemInfo.ItemID.silverEgg
               || eggId == ItemInfo.ItemID.goldEgg
               || eggId == ItemInfo.ItemID.MagicalEgg
               || eggId == ItemInfo.ItemID.SuperMagicalEgg;
    }

    public static bool IsV3TimedBoxEggId(ItemInfo.ItemID eggId)
    {
        return eggId == ItemInfo.ItemID.timedBoxTutorial
               || eggId == ItemInfo.ItemID.timedBoxCommon
               || eggId == ItemInfo.ItemID.timedBoxRare
               || eggId == ItemInfo.ItemID.timedBoxEpic
               || eggId == ItemInfo.ItemID.timedBoxLegendary;
    }
}
