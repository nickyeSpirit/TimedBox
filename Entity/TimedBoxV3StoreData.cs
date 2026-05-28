
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
    public TimeBoxDefine boxType;
    public TimedBoxV3SlotStatus status;
    public int customData;

    public System.DateTime endTime;
    public bool isHatchDeferred;

    public bool IsDeferredHatch => status == TimedBoxV3SlotStatus.Hatching && isHatchDeferred;

    public bool IsActiveHatching => status == TimedBoxV3SlotStatus.Hatching && !isHatchDeferred;

    public float timeLeft
    {
        set
        {
            endTime = System.DateTime.Now.AddSeconds(value);
        }
    }

    public void ResetData()
    {
        timeLeft = 0;
        status = TimedBoxV3SlotStatus.Empty;
        isHatchDeferred = false;
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

    public int numSkipTimeToday;
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

    public void AddEgg(TimeBoxDefine boxType, float hatchTime, bool isDeferred, int customData = 0)
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
                slots[i].boxType = boxType;
                slots[i].customData = customData;
                slots[i].status = TimedBoxV3SlotStatus.Hatching;
                slots[i].isHatchDeferred = isDeferred;
                
                if (!isDeferred)
                {
                    slots[i].endTime = System.DateTime.Now.AddSeconds(hatchTime);
                }
                break;
            }
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
                if ((System.DateTime.Now - x.endTime).TotalSeconds > 0)
                {
                    x.status = TimedBoxV3SlotStatus.Hatched;

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

    public void StartActiveHatching(int index, int hatchTime)
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
        slot.endTime = System.DateTime.Now.AddSeconds(hatchTime);

    }

    public void HatchEgg(int index, int hatchTime)
    {
        StartActiveHatching(index, hatchTime);


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