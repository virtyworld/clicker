using Leopotam.EcsLite;

public sealed class IncomeCalculationSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;
    private EcsFilter _progressEventFilter;
    private EcsPool<ProgressEventComponent> _progressEventPool;
    private EcsPool<BusinessComponent> _businessPool;
    private EcsPool<Upgrade1Component> _upgrade1Pool;
    private EcsPool<Upgrade2Component> _upgrade2Pool;
    private EcsPool<LevelComponent> _levelPool;
    private EcsFilter _lvlUpEventFilter;
    private EcsPool<LvlUpEventComponent> _lvlUpEventPool;
    private EcsPool<LvlUpProcessedComponent> _lvlUpProcessedPool;
    private EcsPool<ProgressProcessedFlag> _progressProcessedFlagPool;
    private EcsFilter _upgradeEventFilter;
    private EcsPool<UpgradeEventComponent> _upgradeEventPool;
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _progressEventFilter = _world.Filter<ProgressEventComponent>().End();
        _progressEventPool = _world.GetPool<ProgressEventComponent>();
        _businessPool = _world.GetPool<BusinessComponent>();
        _upgrade1Pool = _world.GetPool<Upgrade1Component>();
        _upgrade2Pool = _world.GetPool<Upgrade2Component>();
        _levelPool = _world.GetPool<LevelComponent>();
        _lvlUpEventFilter = _world.Filter<LvlUpEventComponent>().End();
        _lvlUpEventPool = _world.GetPool<LvlUpEventComponent>();
        _lvlUpProcessedPool = _world.GetPool<LvlUpProcessedComponent>();
        _progressProcessedFlagPool = _world.GetPool<ProgressProcessedFlag>();
        _upgradeEventFilter = _world.Filter<UpgradeEventComponent>().End();
        _upgradeEventPool = _world.GetPool<UpgradeEventComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        CalculateIncomeByProgressEvent();
        CalculateIncomeByLvlUpEvent();
        CalculateIncomeByUpgradeEvent();
    }

    private void CalculateIncomeByProgressEvent()
    {
        foreach (var entity in _progressEventFilter)
        {
            ref var progressEvent = ref _progressEventPool.Get(entity);

            if (progressEvent._ecsPackedEntity.Unpack(_world, out int progressUnpackedEntity))
            {
                ref var business = ref _businessPool.Get(progressUnpackedEntity);
                ref var upgrade1 = ref _upgrade1Pool.Get(progressUnpackedEntity);
                ref var upgrade2 = ref _upgrade2Pool.Get(progressUnpackedEntity);
                ref var lvl = ref _levelPool.Get(progressUnpackedEntity);

                float money = CalculateIncome(lvl._level, business._baseIncome, upgrade1, upgrade2);
                AddMoneyEvent(money);
                ChangeUI(progressUnpackedEntity, money);
                AddProgressProcessedFlag(progressUnpackedEntity);
            }
        }
    }
    private void CalculateIncomeByLvlUpEvent()
    {
        foreach (var entity in _lvlUpEventFilter)
        {
            ref var lvlUpEvent = ref _lvlUpEventPool.Get(entity);

            if (lvlUpEvent._ecsPackedEntity.Unpack(_world, out int lvlUpUnpackedEntity))
            {
                ref var business = ref _businessPool.Get(lvlUpUnpackedEntity);
                ref var upgrade1 = ref _upgrade1Pool.Get(lvlUpUnpackedEntity);
                ref var upgrade2 = ref _upgrade2Pool.Get(lvlUpUnpackedEntity);
                ref var lvl = ref _levelPool.Get(lvlUpUnpackedEntity);
                float money = CalculateIncome(lvl._level, business._baseIncome, upgrade1, upgrade2);
                ChangeUI(lvlUpUnpackedEntity, money);
            }
        }
    }

    private void CalculateIncomeByUpgradeEvent()
    {
        foreach (var entity in _upgradeEventFilter)
        {
            ref var upgradeEvent = ref _upgradeEventPool.Get(entity);

            if (upgradeEvent._ecsPackedEntity.Unpack(_world, out int upgradeUnpackedEntity))
            {
                ref var business = ref _businessPool.Get(upgradeUnpackedEntity);
                ref var upgrade1 = ref _upgrade1Pool.Get(upgradeUnpackedEntity);
                ref var upgrade2 = ref _upgrade2Pool.Get(upgradeUnpackedEntity);
                ref var lvl = ref _levelPool.Get(upgradeUnpackedEntity);
                float money = CalculateIncome(lvl._level, business._baseIncome, upgrade1, upgrade2);
                ChangeUI(upgradeUnpackedEntity, money);
            }
        }
    }
    private float CalculateIncome(int lvl, int baseIncome, Upgrade1Component upgrade1, Upgrade2Component upgrade2)
    {
        float incomeMultiplier1 = 0;
        float incomeMultiplier2 = 0;

        if (upgrade1._isBought)
        {
            incomeMultiplier1 = upgrade1._incomeMultiplier;
        }
        if (upgrade2._isBought)
        {
            incomeMultiplier2 = upgrade2._incomeMultiplier;
        }

        float money = lvl * baseIncome * (1 + (incomeMultiplier1 / 100) + (incomeMultiplier2 / 100));
        return money;
    }

    private void AddMoneyEvent(float money)
    {
        ref var moneyEvent = ref _world.GetPool<AddMoneyEventComponent>().Add(_world.NewEntity());
        moneyEvent._money = money;
    }
    private void AddProgressProcessedFlag(int entity)
    {
        _progressProcessedFlagPool.Add(entity);
    }
    private void ChangeUI(int entity, float income)
    {
        ref var business = ref _businessPool.Get(entity);
        business._view.UpdateIncome($"Доход\n{income}");
    }
}
