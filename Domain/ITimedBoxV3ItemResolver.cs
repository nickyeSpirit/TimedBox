namespace TimedBoxV3.Module.Domain
{
    public interface ITimedBoxV3ItemResolver
    {
        int GetHatchTime(int itemId);
        bool IsTimeBoxChest(int itemId);
        bool ShouldDeferHatch(int itemId);
    }
}
