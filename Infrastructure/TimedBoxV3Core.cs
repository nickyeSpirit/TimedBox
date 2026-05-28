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
                var config = GameInformation.instance.TimedBoxV3SO;
                var storage = new UnityStorage();
                _instance = TimedBoxModuleFactory.Create(config, storage);
            }
            return _instance;
        }
    }
}
