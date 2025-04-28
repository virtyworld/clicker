using Leopotam.EcsLite;
using TMPro;
using UnityEngine;


public sealed class EcsStartup : MonoBehaviour
{
    [SerializeField] private GameObject _businessPrefab;
    [SerializeField] private Transform _parentToSpawnUI;
    [SerializeField] private ConfigHandler _configHandler;
    [SerializeField] private TextMeshProUGUI _moneyText;
    private EcsWorld _world;
    private IEcsSystems _start;
    private IEcsSystems _fixedUpdate;

    private void Start()
    {
        _world = new EcsWorld();

        MoneySystem moneySystem = new MoneySystem();
        moneySystem.Setup(_moneyText);
        LevelSystem levelSystem = new LevelSystem();
        levelSystem.Setup(_configHandler);
        UpgradeButtonsSystem upgradeButtonsSystem = new UpgradeButtonsSystem();
        upgradeButtonsSystem.Setup(_configHandler);

        _start = new EcsSystems(_world);
        var spawnBusinessSystem = new SpawnBusinessSystem();
        spawnBusinessSystem.Setup(_businessPrefab, _parentToSpawnUI, _configHandler);
        _start
            .Add(spawnBusinessSystem)

#if UNITY_EDITOR
            .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
            .Add(new Leopotam.EcsLite.UnityEditor.EcsSystemsDebugSystem())
#endif
            // .InjectUgui(_uguiEmitter)
            .Init();

        _fixedUpdate = new EcsSystems(_world);
        _fixedUpdate
            .Add(moneySystem)
            .Add(new FirstRunLogicSystem())
            .Add(levelSystem)
            .Add(new IncomeCalculationSystem())
            .Add(new ProgressSystem())
            .Add(upgradeButtonsSystem)

#if UNITY_EDITOR
            .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
            .Add(new Leopotam.EcsLite.UnityEditor.EcsSystemsDebugSystem())

#endif           
            .Init();

    }

    private void FixedUpdate()
    {
        _fixedUpdate?.Run();
    }

    private void OnDestroy()
    {
        if (_start != null)
        {
            _start.Destroy();
            _start = null;
        }

        if (_fixedUpdate != null)
        {
            _fixedUpdate.Destroy();
            _fixedUpdate = null;
        }

        if (_world != null)
        {
            _world.Destroy();
            _world = null;
        }
    }
}
