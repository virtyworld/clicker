using Leopotam.EcsLite;

using UnityEngine;


public sealed class SpawnBusinessSystem : IEcsInitSystem
{
    private EcsWorld _world;
    private GameObject _prefab;
    private Transform _parentToSpawnUI;
    private ConfigHandler _configHandler;
    public void Setup(GameObject prefab, Transform parentToSpawnUI, ConfigHandler configHandler)
    {
        _prefab = prefab;
        _parentToSpawnUI = parentToSpawnUI;
        _configHandler = configHandler;
    }
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        SpawnBusiness();
    }

    private void SpawnBusiness()
    {
        for (int i = 0; i < 5; i++)
        {
            var config = _configHandler.GetConfig(i);
            Spawn(i, config);
        }
    }

    private void Spawn(int businessId, BusinessConfigSO config)
    {
        var entity = _world.NewEntity();
        InitBusiness(entity, businessId, config);
        var gm = Object.Instantiate(_prefab, _parentToSpawnUI);
        var bw = gm.GetComponent<BusinessView>();
        bw.Setup(_world, _world.PackEntity(entity), businessId);
        float money = PlayerPrefs.GetFloat("Money", 4);
        Upgrade1Component upgrade1 = InitUpgrade1(entity, config, businessId, bw, money);
        Upgrade2Component upgrade2 = InitUpgrade2(entity, config, businessId, bw, money);
        int lvl = InitLvl(entity, businessId, bw);
        InitIncome(entity, bw, config, upgrade1, upgrade2, lvl);
        InitProgress(entity, businessId, bw, config);
        InitLvlUp(entity, businessId, bw, config, lvl, money);
        ref var business = ref _world.GetPool<BusinessComponent>().Get(entity);
        business._view = bw;
        SetTitle(config, bw);
    }

    private void InitBusiness(int entity, int businessId, BusinessConfigSO config)
    {
        ref var business = ref _world.GetPool<BusinessComponent>().Add(entity);
        business._id = businessId;
        business._incomeDelay = config._incomeDelay;
        business._baseCost = config._baseCost;
        business._baseIncome = config._baseIncome;
    }
    private int InitLvl(int entity, int businessId, BusinessView bw)
    {
        ref var level = ref _world.GetPool<LevelComponent>().Add(entity);
        var key = $"Level_{businessId}";
        level._level = PlayerPrefs.GetInt(key, 0);
        bw.UpdateLvl($"LVL {level._level}");
        return level._level;
    }
    private Upgrade1Component InitUpgrade1(int entity, BusinessConfigSO config, int businessId, BusinessView bw, float money)
    {
        ref var upgrade1 = ref _world.GetPool<Upgrade1Component>().Add(entity);
        var key = $"Upgrade0_{businessId}";
        upgrade1._isBought = PlayerPrefs.GetInt(key, 0) == 1;
        upgrade1._incomeMultiplier = config._upgrade1._incomeMultiplier;
        var text = "";

        if (upgrade1._isBought)
        {
            text = $"{config._upgrade1._upgradeName}\n Доход +{upgrade1._incomeMultiplier}% \nКуплено";
            bw.DisableUpgrade1Button();
        }
        else
        {
            text = $"{config._upgrade1._upgradeName}\n Доход +{upgrade1._incomeMultiplier}% \n Цена {config._upgrade1._price}";
            Debug.Log(money + " " + config._upgrade1._price);
            if (money < config._upgrade1._price)
            {
                bw.DisableUpgrade1Button();
            }
        }

        bw.UpdateUpgrade1($"{text}");
        return upgrade1;
    }

    private Upgrade2Component InitUpgrade2(int entity, BusinessConfigSO config, int businessId, BusinessView bw, float money)
    {
        ref var upgrade2 = ref _world.GetPool<Upgrade2Component>().Add(entity);
        var key = $"Upgrade1_{businessId}";
        upgrade2._isBought = PlayerPrefs.GetInt(key, 0) == 1;
        upgrade2._incomeMultiplier = config._upgrade2._incomeMultiplier;
        var text = "";

        if (upgrade2._isBought)
        {
            text = $"{config._upgrade2._upgradeName}\n Доход +{upgrade2._incomeMultiplier}% \nКуплено";
            bw.DisableUpgrade2Button();
        }
        else
        {
            text = $"{config._upgrade2._upgradeName}\n Доход +{upgrade2._incomeMultiplier}% \n Цена {config._upgrade2._price}";
            Debug.Log(money + " " + config._upgrade2._price);
            if (money < config._upgrade2._price)
            {
                bw.DisableUpgrade2Button();
            }
        }

        bw.UpdateUpgrade2($"{text}");
        return upgrade2;
    }
    private void InitIncome(int entity, BusinessView bw, BusinessConfigSO config, Upgrade1Component upgrade1,
    Upgrade2Component upgrade2, int lvl)
    {
        ref var income = ref _world.GetPool<IncomeComponent>().Add(entity);
        var incomeMultiplier1 = 0f;
        var incomeMultiplier2 = 0f;

        if (upgrade1._isBought)
        {
            incomeMultiplier1 = upgrade1._incomeMultiplier;
        }
        if (upgrade2._isBought)
        {
            incomeMultiplier2 = upgrade2._incomeMultiplier;
        }

        income._income = (int)(lvl * config._baseIncome * (1 + (incomeMultiplier1 / 100) + (incomeMultiplier2 / 100)));
        bw.UpdateIncome($"Доход\n{income._income}");
    }
    private void InitProgress(int entity, int businessId, BusinessView bw, BusinessConfigSO config)
    {
        ref var progress = ref _world.GetPool<ProgressComponent>().Add(entity);
        progress._businessId = businessId;
        progress._maxProgress = config._incomeDelay;
        progress._businessView = bw;
        bw.UpdateProgress(0);
    }
    private void InitLvlUp(int entity, int businessId, BusinessView bw, BusinessConfigSO config, int lvl, float money)
    {
        ref var lvlUp = ref _world.GetPool<LvlUpComponent>().Add(entity);
        lvlUp._businessId = businessId;
        lvlUp._cost = config._baseCost;
        lvlUp._businessView = bw;
        var key = $"Level_{businessId}";
        int currentLevel = PlayerPrefs.GetInt(key, 0);
        float cost = (currentLevel + 1) * config._baseCost;
        bw.UpdateLvlUpButton($"LVL UP\n Цена {cost}$");

        if (cost > money)
        {
            bw.DisableLvlUpButton();
        }

    }
    private void SetTitle(BusinessConfigSO config, BusinessView bw)
    {
        bw.UpdateTitle(config._businessName);
    }
}
