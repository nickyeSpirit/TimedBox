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
    public int itemID;
    public int amount;
}

[Serializable]
public class DictionaryTimedBoxV3ProgressionDetail : TimedBoxV3SerializedDictionary<TimeBoxDefine, TimedBoxV3ProgressionDetail> { }

[Serializable]
public class TimedBoxV3Progression
{
    public DictionaryTimedBoxV3ProgressionDetail timedBoxV3ProgressionDetail;
}

[Serializable]
public class DictionaryTimedBoxV3Progression : TimedBoxV3SerializedDictionary<int, TimedBoxV3Progression> { }


[Serializable]
public class DictionaryTimedBoxV3RewardSO : TimedBoxV3SerializedDictionary<TimeBoxDefine, UnityEngine.ScriptableObject> { }

[Serializable]
public class DictionaryMonsterTimedBoxWeight : TimedBoxV3SerializedDictionary<MonsterTimeBoxType, float> { }
