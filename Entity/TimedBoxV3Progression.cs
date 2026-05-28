using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class TimedBoxV3ProgressionDetail
{
    public List<RewardDetail> rewards;
}

[Serializable]
public class RewardDetail
{
    public ItemInfo.ItemID itemID;
    public int amount;
}

[Serializable]
public class DictionaryTimedBoxV3ProgressionDetail : UnitySerializedDictionary<TimeBoxDefine, TimedBoxV3ProgressionDetail> { }

[Serializable]
public class TimedBoxV3Progression
{
    public DictionaryTimedBoxV3ProgressionDetail timedBoxV3ProgressionDetail;
}

[Serializable]
public class DictionaryTimedBoxV3Progression : UnitySerializedDictionary<int, TimedBoxV3Progression> { }


[Serializable]
public class DictionaryTimedBoxV3RewardSO : UnitySerializedDictionary<TimeBoxDefine, UnityEngine.ScriptableObject> { }

[Serializable]
public class DictionaryMonsterTimedBoxWeight : UnitySerializedDictionary<MonsterTimeBoxType, float> { }
