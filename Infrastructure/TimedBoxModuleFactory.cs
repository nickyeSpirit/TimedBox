

/// <summary>
/// [Infrastructure Layer] - TimedBoxV3.Module - Entrypoint
/// Composition Root: Khởi tạo và wire toàn bộ dependencies của TimedBox Module.
/// </summary>
public static class TimedBoxModuleFactory
{
    public static TimedBoxModuleBundle Create(TimedBoxV3ConfigSO config, IStorage storage, ITimedBoxRepository repository, TimedBoxV3.Module.Domain.ITimedBoxIdMapper idMapper, ITimedBoxProgressionMapper progressionMapper)
    {
        var featureController = new TimedBoxV3FeatureController(progressionMapper, config, storage);

        return new TimedBoxModuleBundle
        {
            Feature = featureController,
            Repository = repository,
            IdMapper = idMapper
        };
    }
}

public class TimedBoxModuleBundle
{
    public ITimedBoxV3Feature Feature;
    public ITimedBoxRepository Repository;
    public TimedBoxV3.Module.Domain.ITimedBoxIdMapper IdMapper;
}
