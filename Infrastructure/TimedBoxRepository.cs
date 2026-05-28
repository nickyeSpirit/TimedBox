using UnityEngine;

/// <summary>
/// [Infrastructure Layer] - TimedBoxV3.Module
/// Implementation cá»§a ITimedBoxRepository.
/// </summary>
public class TimedBoxRepository : ITimedBoxRepository
{
    private TimedBoxV3StoreData _cachedData;

    public TimedBoxV3StoreData GetStoreData()
    {
        if (_cachedData != null) return _cachedData;
        _cachedData = DataManager.Instance?.inventoryData?.timedBoxV3StoreData;
        return _cachedData;
    }

    public void SaveStoreData(TimedBoxV3StoreData data)
    {
        _cachedData = data;
        if (DataManager.Instance != null && DataManager.Instance.inventoryData != null)
        {
            DataManager.Instance.inventoryData.timedBoxV3StoreData = data;
        }
    }

    public void InvalidateCache()
    {
        _cachedData = null;
    }
}
