/// <summary>
/// [Domain Layer] - TimedBoxV3.Module
/// Quản lý việc đọc/ghi dữ liệu trạng thái các slot rương đang ấp.
/// </summary>
public interface ITimedBoxRepository
{
    TimedBoxV3StoreData GetStoreData();
    void SaveStoreData(TimedBoxV3StoreData data);
    void InvalidateCache();
}
