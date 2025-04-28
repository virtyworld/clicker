using Leopotam.EcsLite;

using UnityEngine;

public sealed class LevelSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;
    private EcsFilter _lvlUpEventFilter;
    private EcsPool<LvlUpEventComponent> _lvlUpEventPool;
    private EcsPool<LevelComponent> _levelPool;
    private EcsFilter _levelFilter;
    private EcsPool<BusinessComponent> _businessPool;
    private ConfigHandler _configHandler;
    private EcsPool<LvlUpComponent> _lvlUpPool;

    private EcsPool<LvlUpEventProcessedFlag> _lvlUpEventProcessedFlag;
    private EcsPool<MoneyComponent> _moneyPool;
    private EcsFilter _addMoneyEventFilter;
    private EcsFilter _deleteMoneyEventFilter;
    private EcsFilter _moneyFilter;
    private MoneyComponent _money;
    public void Setup(ConfigHandler configHandler)
    {
        _configHandler = configHandler;
    }
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _lvlUpEventFilter = _world.Filter<LvlUpEventComponent>().End();
        _lvlUpEventPool = _world.GetPool<LvlUpEventComponent>();
        _levelPool = _world.GetPool<LevelComponent>();
        _levelFilter = _world.Filter<LevelComponent>().End();
        _businessPool = _world.GetPool<BusinessComponent>();
        _lvlUpPool = _world.GetPool<LvlUpComponent>();
        _moneyPool = _world.GetPool<MoneyComponent>();
        _lvlUpEventProcessedFlag = _world.GetPool<LvlUpEventProcessedFlag>();
        _addMoneyEventFilter = _world.Filter<AddMoneyEventComponent>().End();
        _deleteMoneyEventFilter = _world.Filter<DeleteMoneyEventComponent>().End();
        _moneyFilter = _world.Filter<MoneyComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _moneyFilter)
        {
            ref var money = ref _moneyPool.Get(entity);
            _money = money;
        }

        CheckFlags();
        LvlUp();

        HandleAddMoneyEvent();
        HandleDeleteMoneyEvent();
    }

    //проверяет флаги на наличие и удаляет их и LvlUpEventComponent
    private void CheckFlags()
    {
        foreach (var entity in _lvlUpEventFilter)
        {
            if (_lvlUpEventProcessedFlag.Has(entity))
            {
                _lvlUpEventProcessedFlag.Del(entity);
                _lvlUpEventPool.Del(entity);
            }
        }
    }
    private void LvlUp()
    {
        foreach (var entity in _lvlUpEventFilter)
        {
            ref var lvlUpEvent = ref _lvlUpEventPool.Get(entity);

            if (lvlUpEvent._ecsPackedEntity.Unpack(_world, out int unpackedBusinessEntity))
            {
                ref var level = ref _levelPool.Get(unpackedBusinessEntity);
                level._level++;
                SetCurrentLevelText(unpackedBusinessEntity, level._level);
                int price = level._level + 1 * _configHandler.GetConfig(lvlUpEvent._businessId)._baseCost;
                SetLvlUpTextButton(unpackedBusinessEntity, price);
                DeleteMoneyEvent(price);
                SaveToPlayerPrefs(lvlUpEvent._businessId, level._level);
            }

            //помечаем эту сущность флагом, чтобы понимать, что это мы уже обработали
            //нужно,чтобы заэвэйдить конфликты с другими системами, которым нужна инфа о событии LvlUpEventComponent
            //но они должны стоять позже этой системы, потому что по-другому либо LvlUpEventComponent будет сразу 
            //удаляться и другие системы не смогут обработать евент, либо писать еще одну систему специально для 
            //удаления евента
            _lvlUpEventProcessedFlag.Add(entity);
        }
    }
    private void HandleAddMoneyEvent()
    {
        foreach (var entity in _addMoneyEventFilter)
        {
            InteractableButtonHandler();
        }
    }
    private void HandleDeleteMoneyEvent()
    {
        foreach (var entity in _deleteMoneyEventFilter)
        {
            InteractableButtonHandler();
        }
    }
    private void InteractableButtonHandler()
    {
        foreach (var entity in _levelFilter)
        {
            ref var level = ref _levelPool.Get(entity);
            ref var lvlUp = ref _lvlUpPool.Get(entity);
            LvlUpButtonHandler(entity, lvlUp._cost);
        }
    }
    private void SetCurrentLevelText(int entity, int currentLvl)
    {
        ref var business = ref _businessPool.Get(entity);
        business._view.UpdateLvl($"LVL \n {currentLvl}");
    }
    private void LvlUpButtonHandler(int entity, int price)
    {
        ref var business = ref _businessPool.Get(entity);

        if (price <= _money._balance)
        {
            business._view.EnableLvlUpButton();
        }
        else
        {
            business._view.DisableLvlUpButton();
        }
    }
    private void SetLvlUpTextButton(int entity, int price)
    {
        ref var lvlUp = ref _lvlUpPool.Get(entity);
        lvlUp._cost = price;
        lvlUp._businessView.UpdateLvlUpButton($"LVL UP \n Цена {price}$");
    }

    private void DeleteMoneyEvent(int price)
    {
        ref var deleteMoneyEvent = ref _world.GetPool<DeleteMoneyEventComponent>().Add(_world.NewEntity());
        deleteMoneyEvent._money = price;
    }
    private void SaveToPlayerPrefs(int businessId, int level)
    {
        var key = $"Level_{businessId}";
        PlayerPrefs.SetInt(key, level);
    }
}
