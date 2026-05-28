namespace TimedBoxV3.Module.Domain
{
    public interface ITimedBoxIdMapper
    {
        int GetHatchTime(TimeBoxDefine boxType);
        bool IsDeferredHatch(TimeBoxDefine boxType);
    }
}
