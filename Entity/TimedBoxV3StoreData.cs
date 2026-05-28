using Architecture.EventBus;
using CodeStage.AntiCheat.ObscuredTypes;
using MonsterTrainer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public enum TimedBoxV3SlotStatus
{
    Empty,
    Hatching,
    Hatched,
}

[System.Serializable]
public class TimedBoxV3SlotData
{
    public TimedBoxV3SlotStatus status;
    public ItemInfo.ItemID timedBoxV3;
    public IMountInfo.MountType mountType;

    public System.DateTime endTime;
    public bool isHatchDeferred;

    public bool IsDeferredHatch => status == TimedBoxV3SlotStatus.Hatching && isHatchDeferred;

    public bool IsActiveHatching => status == TimedBoxV3SlotStatus.Hatching && !isHatchDeferred;

    public float timeLeft
    {
        set
        {
            endTime = DateTimeHelper.Instance.GetTimeNow().AddSeconds(value);
        }
    }

    public void ResetData()
    {
        timeLeft = 0;
        status = TimedBoxV3SlotStatus.Empty;
        isHatchDeferred = false;
        mountType = IMountInfo.MountType.None;
        EventSystemServiceStatic.DispatchAll(GameConstant.OnOpenTimeBox);
    }
}

[System.Serializable]
public class TimedBoxV3StoreData
{
    public const int MAX_SLOT = 8;
    public const int DEFAULT_FREE_SLOTS = 2;
    public const int IAP_SLOT_COUNT = 6;

    [Serializable]
    public enum ESOURCE_INCREASE_SLOT
    {
        NONE = 0,
        DEFAULT = 1,
        IAP_01 = 10,
        IAP_02,
        IAP_03,
        IAP_04,
        IAP_05,
        IAP_06,
    }

    public TimedBoxV3SlotData slot1;
    public TimedBoxV3SlotData slot2;
    public TimedBoxV3SlotData slot3;
    public TimedBoxV3SlotData slot4;
    public TimedBoxV3SlotData slot5;
    public TimedBoxV3SlotData slot6;
    public TimedBoxV3SlotData slot7;
    public TimedBoxV3SlotData slot8;

    public ObscuredInt numSkipTimeToday;
    public List<string> ExtraOpenerSources = new List<string>();

    public TimedBoxV3StoreData()
    {
        slot1 = new TimedBoxV3SlotData();
        slot2 = new TimedBoxV3SlotData();
        slot3 = new TimedBoxV3SlotData();
        slot4 = new TimedBoxV3SlotData();
        slot5 = new TimedBoxV3SlotData();
        slot6 = new TimedBoxV3SlotData();
        slot7 = new TimedBoxV3SlotData();
        slot8 = new TimedBoxV3SlotData();
    }

    public int maxNumberOfHatchingEgg
    {
        get
        {
            int totalSlot = DEFAULT_FREE_SLOTS;
            if (ExtraOpenerSources != null)
            {
                totalSlot += ExtraOpenerSources.Count;
            }

            return Mathf.Min(totalSlot, MAX_SLOT);
        }
    }

    public List<EggSlotData> oldSlots
    {
        get
        {
            if (_oldSlots == null)
            {
                _oldSlots = new List<EggSlotData>();
            }
            return _oldSlots;
        }
    }
    
    public List<TimedBoxV3SlotData> slots
    {
        get
        {
            if (_slots == null || _slots.Count <= 0)
            {
                _slots = new List<TimedBoxV3SlotData>() { slot1, slot2, slot3, slot4, slot5, slot6, slot7, slot8 };
            }

            return _slots;
        }
    }

    [System.NonSerialized] List<TimedBoxV3SlotData> _slots;
    
    [SerializeField]
    private List<EggSlotData> _oldSlots;

    public bool canHatchEgg => HasDeferredHatchReadyToStart();

    public bool canAddEgg => HasEmptyActiveSlot();

    public bool CheckCanAddEgg(int quantity)
    {
        int emptyActiveSlots = 0;
        int maxActive = maxNumberOfHatchingEgg;
        for (int i = 0; i < slots.Count && i < maxActive; i++)
        {
            if (slots[i].status == TimedBoxV3SlotStatus.Empty)
            {
                emptyActiveSlots++;
            }
        }

        return emptyActiveSlots >= quantity;
    }

    private bool HasEmptyActiveSlot()
    {
        int maxActive = maxNumberOfHatchingEgg;
        for (int i = 0; i < slots.Count && i < maxActive; i++)
        {
            if (slots[i].status == TimedBoxV3SlotStatus.Empty)
            {
                return true;
            }
        }

        return false;
    }

    public int GetIndexOldTimedBox()
    {
        for (int i = 0; i < oldSlots.Count; i++)
        {
            if (oldSlots[i].status != EggStatus.Empty)
            {
                return i;
            }
        }

        return -1;
    }

    public int numberOfEgg => MAX_SLOT - numberOfEmptySlot;

    public int numberOfTimedBoxInActiveSlots
    {
        get
        {
            int count = 0;
            int maxActive = maxNumberOfHatchingEgg;
            for (int i = 0; i < slots.Count && i < maxActive; i++)
            {
                if (slots[i].status != TimedBoxV3SlotStatus.Empty)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public bool IsActiveSlotsFull => numberOfTimedBoxInActiveSlots >= maxNumberOfHatchingEgg;

    private static bool ShouldDeferHatchTimerForEgg(ItemInfo.ItemID eggId)
    {
        if (eggId != ItemInfo.ItemID.timedBoxTutorial)
        {
            return false;
        }

        var playerData = DataManager.Instance?.playerData;
        return playerData != null && !playerData.playedTutorialUnlockTimedBoxWorldMap;
    }

    public void AddEgg(ItemInfo.ItemID eggId, IMountInfo.MountType mountType = IMountInfo.MountType.None)
    {
        if (!canAddEgg)
        {
            return;
        }

        int maxActive = maxNumberOfHatchingEgg;
        for (int i = 0; i < slots.Count && i < maxActive; i++)
        {
            if (slots[i].status == TimedBoxV3SlotStatus.Empty)
            {
                slots[i].timedBoxV3 = eggId;
                slots[i].mountType = mountType;
                slots[i].status = TimedBoxV3SlotStatus.Hatching;
                if (ShouldDeferHatchTimerForEgg(eggId))
                {
                    slots[i].isHatchDeferred = true;
                }
                else
                {
                    slots[i].isHatchDeferred = false;
                    slots[i].endTime = DateTimeHelper.Instance.GetTimeNow()
                        .AddSeconds(((EggInfo)DOListItem.Instance.GetItemInfo(slots[i].timedBoxV3)).hatchTime);
                }

                EventSystemServiceStatic.DispatchAll(GameConstant.OnOpenTimeBox);
                break;
            }
        }
    }

    public void StartDeferredTutorialTimedBoxHatching()
    {
        bool anyStarted = false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsDeferredHatch && slots[i].timedBoxV3 == ItemInfo.ItemID.timedBoxTutorial)
            {
                StartActiveHatching(i);
                anyStarted = true;
            }
        }

        if (anyStarted)
        {
            DataManager.Save();
        }
    }

    public int numberOfHatchingEgg
    {
        get
        {
            int ret = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsActiveHatching)
                {
                    ret++;
                }
            }

            return ret;
        }
    }

    public int numberOfHatchedEgg
    {
        get
        {
            int ret = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].status == TimedBoxV3SlotStatus.Hatched)
                {
                    ret++;
                }
            }

            return ret;
        }
    }

    public int numberOfEmptySlot
    {
        get
        {
            int ret = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].status == TimedBoxV3SlotStatus.Empty)
                {
                    ret++;
                }
            }

            return ret;
        }
    }

    public void ReCalculateHatchTime()
    {
        if (slots == null)
        {
            return;
        }

        slots.ForEach(x =>
        {
            if (x != null && x.IsActiveHatching)
            {
                if ((DateTimeHelper.Instance.GetTimeNow() - x.endTime).TotalSeconds > 0)
                {
                    x.status = TimedBoxV3SlotStatus.Hatched;
                    EventSystemServiceStatic.DispatchAll(GameConstant.OnOpenTimeBox);
                }
            }
        });
    }

    public bool HasDeferredHatchReadyToStart()
    {
        if (numberOfHatchingEgg >= maxNumberOfHatchingEgg)
        {
            return false;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].IsDeferredHatch)
            {
                return true;
            }
        }

        return false;
    }

    public void StartActiveHatching(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return;
        }

        TimedBoxV3SlotData slot = slots[index];
        if (!slot.IsDeferredHatch)
        {
            return;
        }

        slot.isHatchDeferred = false;
        slot.endTime = DateTimeHelper.Instance.GetTimeNow()
            .AddSeconds(((EggInfo)DOListItem.Instance.GetItemInfo(slot.timedBoxV3)).hatchTime);
        EventSystemServiceStatic.DispatchAll(GameConstant.OnOpenTimeBox);
    }

    public void HatchEgg(int index)
    {
        StartActiveHatching(index);

        TimedBoxV3SlotData slot = slots[index];
        EventBus<EventOpenTimeBox>.Publish(new EventOpenTimeBox()
        {
            eggType = slot.timedBoxV3
        });
    }

    public void CheckAndFixDateTimeBug()
    {
        slots.ForEach(slot =>
        {
            if (slot.IsActiveHatching)
            {
                if ((slot.endTime - DateTimeHelper.Instance.GetTimeNow()).TotalSeconds > ((EggInfo)DOListItem.Instance.GetItemInfo(slot.timedBoxV3)).hatchTime)
                {
                    slot.endTime = DateTimeHelper.Instance.GetTimeNow().AddSeconds(((EggInfo)DOListItem.Instance.GetItemInfo(slot.timedBoxV3)).hatchTime);
                }
            }
        });
    }

    public void MigrateLegacyIAPUnlocks(EggStoreData legacy)
    {
        ForceClaimOldTimedBox();
        
        if (legacy?.ExtraOpenerSources == null || legacy.ExtraOpenerSources.Count == 0)
        {
            return;
        }

        var legacyIAPSources = new[]
        {
            ESOURCE_INCREASE_SLOT.IAP_01,
            ESOURCE_INCREASE_SLOT.IAP_02,
            ESOURCE_INCREASE_SLOT.IAP_03,
        };

        foreach (var source in legacyIAPSources)
        {
            if (legacy.ExtraOpenerSources.Contains(source.ToString()))
            {
                UnlockIAPSourceFromIAP(source);
            }
        }
    }

    private void ForceClaimOldTimedBox()
    {
        var eggData = DataManager.Instance.inventoryData.eggStoreData;
        
        for (var i = 0; i < eggData.slots.Count; i++)
        {
            var slot = new EggSlotData()
            {
                status = eggData.slots[i].status,
                egg = eggData.slots[i].egg,
            };
            
            if (slot.status != EggStatus.Empty)
            {
                oldSlots.Add(slot);
            }
            eggData.slots[i].ResetData();
        }
    }

    public void ValidateData()
    {
        if (slot1 == null) slot1 = new TimedBoxV3SlotData();
        if (slot2 == null) slot2 = new TimedBoxV3SlotData();
        if (slot3 == null) slot3 = new TimedBoxV3SlotData();
        if (slot4 == null) slot4 = new TimedBoxV3SlotData();
        if (slot5 == null) slot5 = new TimedBoxV3SlotData();
        if (slot6 == null) slot6 = new TimedBoxV3SlotData();
        if (slot7 == null) slot7 = new TimedBoxV3SlotData();
        if (slot8 == null) slot8 = new TimedBoxV3SlotData();

        if (ExtraOpenerSources == null)
        {
            ExtraOpenerSources = new List<string>();
        }

        ReCalculateHatchTime();

        var allSlot = new List<TimedBoxV3SlotData>()
        {
            slot1, slot2, slot3, slot4, slot5, slot6, slot7, slot8,
        };

        const int legacyNewEggStatus = 1;

        foreach (var slot in allSlot)
        {
            if (slot == null)
            {
                continue;
            }

            if ((int)slot.status == legacyNewEggStatus)
            {
                slot.status = TimedBoxV3SlotStatus.Hatching;
                slot.isHatchDeferred = ShouldDeferHatchTimerForEgg(slot.timedBoxV3);
                if (!slot.isHatchDeferred)
                {
                    var eggInfo = DOListItem.Instance.GetItemInfo(slot.timedBoxV3) as EggInfo;
                    if (eggInfo != null)
                    {
                        slot.endTime = DateTimeHelper.Instance.GetTimeNow().AddSeconds(eggInfo.hatchTime);
                    }
                }
            }

            if (slot.status != TimedBoxV3SlotStatus.Empty)
            {
                if (!ItemInfo.IsItemTimeEggChest(slot.timedBoxV3))
                {
                    slot.timedBoxV3 = ItemInfo.ItemID.timedBoxCommon;
                }
            }
        }
    }

    public bool IsActiveExtraSource(ESOURCE_INCREASE_SLOT source)
    {
        switch (source)
        {
            case ESOURCE_INCREASE_SLOT.DEFAULT:
                return true;
            case ESOURCE_INCREASE_SLOT.IAP_01:
            case ESOURCE_INCREASE_SLOT.IAP_02:
            case ESOURCE_INCREASE_SLOT.IAP_03:
            case ESOURCE_INCREASE_SLOT.IAP_04:
            case ESOURCE_INCREASE_SLOT.IAP_05:
            case ESOURCE_INCREASE_SLOT.IAP_06:
                return ExtraOpenerSources != null && ExtraOpenerSources.Contains(source.ToString());
            default:
                return false;
        }
    }

    public ESOURCE_INCREASE_SLOT? GetNextBuyableIAPSource()
    {
        for (int i = 0; i < IAP_SLOT_COUNT; i++)
        {
            var source = (ESOURCE_INCREASE_SLOT)((int)ESOURCE_INCREASE_SLOT.IAP_01 + i);
            if (!IsActiveExtraSource(source))
            {
                return source;
            }
        }

        return null;
    }

    public bool CanPurchaseIAPSource(ESOURCE_INCREASE_SLOT source)
    {
        if (source < ESOURCE_INCREASE_SLOT.IAP_01 || source > ESOURCE_INCREASE_SLOT.IAP_06)
        {
            return false;
        }

        if (IsActiveExtraSource(source))
        {
            return false;
        }

        return GetNextBuyableIAPSource() == source;
    }

    public void UnlockIAPSource(ESOURCE_INCREASE_SLOT source)
    {
        if (!CanPurchaseIAPSource(source))
        {
            return;
        }

        UnlockIAPSourceFromIAP(source);
    }

    public void UnlockIAPSourceFromIAP(ESOURCE_INCREASE_SLOT source)
    {
        if (source < ESOURCE_INCREASE_SLOT.IAP_01 || source > ESOURCE_INCREASE_SLOT.IAP_06)
        {
            return;
        }

        if (ExtraOpenerSources == null)
        {
            ExtraOpenerSources = new List<string>();
        }

        string sourceKey = source.ToString();
        if (!ExtraOpenerSources.Contains(sourceKey))
        {
            ExtraOpenerSources.Add(sourceKey);
        }
    }
}

[Serializable]
public class TimedBoxV3DTO
{
    public TimeBoxDefine type;
} 