using Leopotam.EcsLite;

using UnityEngine;


public sealed class UpgradeButtonsSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;
    private EcsPool<Upgrade1Component> _poolUpgrade1;
    private EcsPool<Upgrade2Component> _poolUpgrade2;
    private EcsFilter _upgrade1Filter;
    private EcsFilter _upgrade2Filter;
    private EcsFilter _moneyFilter;
    private EcsPool<MoneyComponent> _poolMoney;
    private EcsPool<BusinessComponent> _poolBusiness;
    private ConfigHandler _config;
    private MoneyComponent _money;
    private EcsFilter _addMoneyEventFilter;
    private EcsFilter _deleteMoneyEventFilter;
    private EcsFilter _upgradeEventFilter;
    private EcsPool<UpgradeEventComponent> _poolUpgradeEvent;
    private EcsPool<UpgradeProcessedFlag> _poolUpgradeProcessedFlag;
    private EcsFilter _upgradeProcessedFlagFilter;
    public void Setup(ConfigHandler configHandler)
    {
        _config = configHandler;
    }
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _poolUpgrade1 = _world.GetPool<Upgrade1Component>();
        _poolUpgrade2 = _world.GetPool<Upgrade2Component>();
        _upgrade1Filter = _world.Filter<Upgrade1Component>().End();
        _upgrade2Filter = _world.Filter<Upgrade2Component>().End();
        _moneyFilter = _world.Filter<MoneyComponent>().End();
        _poolMoney = _world.GetPool<MoneyComponent>();
        _poolBusiness = _world.GetPool<BusinessComponent>();
        _addMoneyEventFilter = _world.Filter<AddMoneyEventComponent>().End();
        _deleteMoneyEventFilter = _world.Filter<DeleteMoneyEventComponent>().End();
        _upgradeEventFilter = _world.Filter<UpgradeEventComponent>().End();
        _poolUpgradeEvent = _world.GetPool<UpgradeEventComponent>();
        _poolUpgradeProcessedFlag = _world.GetPool<UpgradeProcessedFlag>();
        _upgradeProcessedFlagFilter = _world.Filter<UpgradeProcessedFlag>().End();
    }
    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _moneyFilter)
        {
            ref var money = ref _poolMoney.Get(entity);
            _money = money;
        }

        FlagHandler();
        HandleUpgradeEvent();
        HandleAddMoneyEvent();
        HandleDeleteMoneyEvent();
    }
    private void FlagHandler()
    {
        foreach (var entity in _upgradeProcessedFlagFilter)
        {
            _poolUpgradeProcessedFlag.Del(entity);

            if (_poolUpgrade1.Has(entity))
            {
                _poolUpgrade1.Del(entity);
            }
            if (_poolUpgrade2.Has(entity))
            {
                _poolUpgrade2.Del(entity);
            }
        }
    }
    private void HandleUpgradeEvent()
    {
        foreach (var entity in _upgradeEventFilter)
        {
            ref var upgradeEvent = ref _poolUpgradeEvent.Get(entity);

            if (upgradeEvent._ecsPackedEntity.Unpack(_world, out int upgradeUnpackedEntity))
            {
                ref var business = ref _poolBusiness.Get(upgradeUnpackedEntity);

                if (upgradeEvent._upgradeType == 0)
                {
                    ref var upgrade1 = ref _poolUpgrade1.Get(upgradeUnpackedEntity);
                    upgrade1._isBought = true;

                    DisableUpgrade1Button(business._view);
                    SetUpgrade1Text(business._view, _config.GetConfig(business._id)._upgrade1, upgrade1._isBought);
                }
                if (upgradeEvent._upgradeType == 1)
                {
                    ref var upgrade2 = ref _poolUpgrade2.Get(upgradeUnpackedEntity);
                    upgrade2._isBought = true;
                    DisableUpgrade2Button(business._view);
                    SetUpgrade2Text(business._view, _config.GetConfig(business._id)._upgrade2, upgrade2._isBought);
                }

                SaveToPlayerPrefs(true, business._id, upgradeEvent._upgradeType);
            }
            _poolUpgradeProcessedFlag.Add(entity);
        }
    }
    private void HandleAddMoneyEvent()
    {
        foreach (var entity in _addMoneyEventFilter)
        {
            Button1Handler();
            Button2Handler();
        }
    }
    private void HandleDeleteMoneyEvent()
    {
        foreach (var entity in _deleteMoneyEventFilter)
        {
            Button1Handler();
            Button2Handler();
        }
    }

    private void Button1Handler()
    {
        foreach (var entity in _upgrade1Filter)
        {
            ref var upgrade1 = ref _poolUpgrade1.Get(entity);
            ref var business = ref _poolBusiness.Get(entity);

            if (upgrade1._isBought)
            {
                DisableUpgrade1Button(business._view);
                return;
            }

            SetUpgrade1Text(business._view, _config.GetConfig(business._id)._upgrade1, upgrade1._isBought);

            if (_money._balance >= _config.GetConfig(business._id)._upgrade1._price)
            {
                EnableUpgrade1Button(business._view);
            }
            else
            {
                DisableUpgrade1Button(business._view);
            }
        }
    }
    private void Button2Handler()
    {
        foreach (var entity in _upgrade2Filter)
        {
            ref var upgrade2 = ref _poolUpgrade2.Get(entity);
            ref var business = ref _poolBusiness.Get(entity);

            if (upgrade2._isBought)
            {
                DisableUpgrade2Button(business._view);
                return;
            }

            SetUpgrade2Text(business._view, _config.GetConfig(business._id)._upgrade2, upgrade2._isBought);

            if (_money._balance >= _config.GetConfig(business._id)._upgrade2._price)
            {
                EnableUpgrade2Button(business._view);
            }
            else
            {
                DisableUpgrade2Button(business._view);
            }

        }
    }

    private void SaveToPlayerPrefs(bool isBought, int businessId, int upgradeType)
    {
        var key = $"Upgrade{upgradeType}_{businessId}";
        PlayerPrefs.SetInt(key, isBought ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void SetUpgrade1Text(BusinessView bw, UpgradeData data, bool isBought)
    {
        if (isBought)
        {
            bw.UpdateUpgrade1($"{data._upgradeName}\nДоход {data._incomeMultiplier}%\nКуплено");
        }
        else
        {
            bw.UpdateUpgrade1($"{data._upgradeName}\nДоход {data._incomeMultiplier}%\nЦена {data._price}");
        }
    }
    private void SetUpgrade2Text(BusinessView bw, UpgradeData data, bool isBought)
    {
        if (isBought)
        {
            bw.UpdateUpgrade2($"{data._upgradeName}\nДоход {data._incomeMultiplier}%\nКуплено");
        }
        else
        {
            bw.UpdateUpgrade2($"{data._upgradeName}\nДоход {data._incomeMultiplier}%\nЦена {data._price}");
        }
    }
    private void EnableUpgrade1Button(BusinessView bw)
    {
        bw.EnableUpgrade1Button();
    }
    private void DisableUpgrade1Button(BusinessView bw)
    {
        bw.DisableUpgrade1Button();
    }
    private void EnableUpgrade2Button(BusinessView bw)
    {
        bw.EnableUpgrade2Button();
    }
    private void DisableUpgrade2Button(BusinessView bw)
    {
        bw.DisableUpgrade2Button();
    }
}
