using System;
using System.Linq;

public static class TimedBoxV3Helper
{
    public static TimedBoxV3ProgressionDetail GetTimedBoxV3ProgressionDetail(int level, TimeBoxDefine type, TimedBoxV3ConfigSO config)
    {
        return config.GetTimedBoxV3ProgressionDetail(level, type);
    }

    public static DateTime ConvertStringToDateTime(string dateTime)
    {
        if (DateTime.TryParse(dateTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }
        return DateTime.MinValue;
    }
}
