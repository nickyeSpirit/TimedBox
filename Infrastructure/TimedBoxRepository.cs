using UnityEngine;

/// <summary>
/// [Infrastructure Layer] - TimedBoxV3.Module
/// Implementation cá»§a ITimedBoxRepository.
/// </summary>
public class TimedBoxRepository : ITimedBoxRepository
{
    private TimedBoxV3StoreData _cachedData;
    private readonly IStorage _storage;

    public TimedBoxRepository(IStorage storage)
    {
        _storage = storage;
    }

    public TimedBoxV3StoreData GetStoreData()
    {
        if (_cachedData != null) return _cachedData;
        
        string json = _storage.GetString("TimedBoxV3StoreData");
        if (!string.IsNullOrEmpty(json))
        {
            _cachedData = UnityEngine.JsonUtility.FromJson<TimedBoxV3StoreData>(json);
        }
        
        if (_cachedData == null)
        {
            _cachedData = new TimedBoxV3StoreData();
        }
        return _cachedData;
    }

    public void SaveStoreData(TimedBoxV3StoreData data)
    {
        _cachedData = data;
        string json = UnityEngine.JsonUtility.ToJson(data);
        _storage.SetString("TimedBoxV3StoreData", json);
    }

    public void InvalidateCache()
    {
        _cachedData = null;
    }
}
