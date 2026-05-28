using MonsterTrainer;

/// <summary>
/// [Infrastructure Layer] - TimedBoxV3.Module - Entrypoint
/// Composition Root: Khởi tạo và wire toàn bộ dependencies của TimedBox Module.
/// </summary>
public static class TimedBoxModuleFactory
{
    public static TimedBoxModuleBundle Create(TimedBoxV3ConfigSO config, IStorage storage)
    {
        var mapper = new AtlantisTimedBoxProgressionMapper();
        var repository = new TimedBoxRepository();
        var processor = new AtlantisTimedBoxRewardProcessor(config);
        var rewardService = new TimedBoxRewardService<WSRewardData, AtlantisRewardTimeBoxV3WeightData>(config, processor);
        var featureController = new TimedBoxV3FeatureController(mapper, config, storage);

        return new TimedBoxModuleBundle
        {
            Feature = featureController,
            RewardService = rewardService,
            Repository = repository,
        };
    }
}

public class TimedBoxModuleBundle
{
    public ITimedBoxV3Feature Feature;
    public ITimedBoxRewardService<WSRewardData, AtlantisRewardTimeBoxV3WeightData> RewardService;
    public ITimedBoxRepository Repository;
}
