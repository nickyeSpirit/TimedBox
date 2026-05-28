using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimeBoxModuleRewardData
{
    public ModuleStackableItemData item;
    public int petCount;
    public ModulePetDetail pet;
    public int equipmentCount;
    public ModuleEquipmentItemData equipment;
    public int rewardMount; // mapped from MountType
    public string extendData;
    public string extendData2;
    public ModuleRandomValue minMaxValue;
    public float growthRate;
}

[Serializable]
public class ModuleStackableItemData
{
    public int infoId; 
    public int quantity;
    public string extent;
}

[Serializable]
public class ModulePetDetail
{
    public int petType; 
    public int levelEvolution;
}

[Serializable]
public class ModuleEquipmentItemData
{
    public int infoId;
    public int rarity;
}

[Serializable]
public class ModuleRandomValue
{
    public float min;
    public float max;
}

[Serializable]
public class TimeBoxModuleWeightData : ITimedBoxWeightable<TimeBoxModuleRewardData>
{
    public TimeBoxModuleRewardData reward;
    public float weight;

    public float GetWeight() => weight;
    
    public TimeBoxModuleRewardData GetCurrentReward()
    {
        return reward;
    }
}
