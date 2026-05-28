public interface ITimedBoxProgressionMapper
{
    TimeBoxDefine MapItemIDToBoxType(int itemId);
    int GetCurrentPlayerLevel(); // Navy rank level or similar progression
    int GetCurrentStoryStage();
}
