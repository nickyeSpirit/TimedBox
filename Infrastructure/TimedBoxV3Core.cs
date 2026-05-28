/// <summary>
/// [Infrastructure Layer] - Global Accessor
/// Khởi tạo và lưu giữ singleton instance của TimedBoxModuleBundle cho toàn bộ game.
/// Thay thế cho việc mỗi UI script tự khởi tạo một FeatureController.
/// </summary>
public static class TimedBoxV3Core
{
    private static TimedBoxModuleBundle _instance;

    public static TimedBoxModuleBundle Instance
    {
        get
        {
            if (_instance == null)
            {
                UnityEngine.Debug.LogError("TimedBoxV3Core is not initialized! Call Init() first.");
            }
            return _instance;
        }
    }

    public static void Init(TimedBoxModuleBundle bundle)
    {
        _instance = bundle;
    }
}
