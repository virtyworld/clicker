using Leopotam.EcsLite;
using TMPro;
using UnityEngine;

public sealed class MoneySystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;
    private EcsFilter _addMoneyEventFilter;
    private EcsPool<AddMoneyEventComponent> _addMoneyEventPool;
    private EcsPool<MoneyComponent> _moneyPool;
    private EcsFilter _moneyFilter;
    private EcsFilter _deleteMoneyEventFilter;
    private EcsPool<DeleteMoneyEventComponent> _deleteMoneyEventPool;
    private TextMeshProUGUI _moneyText;
    private EcsPool<MoneyProcessedFlag> _moneyProcessedFlagPool;
    private EcsFilter _moneyProcessedFlagFilter;

    public void Setup(TextMeshProUGUI moneyText)
    {
        _moneyText = moneyText;
    }
    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _addMoneyEventFilter = _world.Filter<AddMoneyEventComponent>().End();
        _addMoneyEventPool = _world.GetPool<AddMoneyEventComponent>();
        _moneyPool = _world.GetPool<MoneyComponent>();
        _moneyFilter = _world.Filter<MoneyComponent>().End();
        _deleteMoneyEventFilter = _world.Filter<DeleteMoneyEventComponent>().End();
        _deleteMoneyEventPool = _world.GetPool<DeleteMoneyEventComponent>();
        _moneyProcessedFlagPool = _world.GetPool<MoneyProcessedFlag>();
        _moneyProcessedFlagFilter = _world.Filter<MoneyProcessedFlag>().End();
        InitMoney();
    }

    public void Run(IEcsSystems systems)
    {
        FlagsHandler();
        AddMoney();
        TakeMoney();
    }

    private void InitMoney()
    {
        float money = PlayerPrefs.GetFloat("Money", 4);
        ref var moneyComponent = ref _moneyPool.Add(_world.NewEntity());
        moneyComponent._balance = money;
        UpdateUI(moneyComponent._balance);
    }
    private void FlagsHandler()
    {
        foreach (var entity in _moneyProcessedFlagFilter)
        {
            if (_addMoneyEventPool.Has(entity))
            {
                _addMoneyEventPool.Del(entity);
            }
            if (_deleteMoneyEventPool.Has(entity))
            {
                _deleteMoneyEventPool.Del(entity);
            }

            _moneyProcessedFlagPool.Del(entity);
        }
    }
    private void AddMoney()
    {
        foreach (var addMoneyEventEntity in _addMoneyEventFilter)
        {
            ref var addMoneyEvent = ref _addMoneyEventPool.Get(addMoneyEventEntity);

            foreach (var entity in _moneyFilter)
            {
                ref var money = ref _moneyPool.Get(entity);
                money._balance += addMoneyEvent._money;
                UpdateUI(money._balance);
                SaveToPlayerPrefs(money._balance);
            }

            _moneyProcessedFlagPool.Add(addMoneyEventEntity);
        }
    }
    private void TakeMoney()
    {
        foreach (var deleteMoneyEventEntity in _deleteMoneyEventFilter)
        {
            ref var deleteMoneyEvent = ref _deleteMoneyEventPool.Get(deleteMoneyEventEntity);

            foreach (var entity in _moneyFilter)
            {
                ref var money = ref _moneyPool.Get(entity);

                if (money._balance >= deleteMoneyEvent._money)
                {
                    money._balance -= deleteMoneyEvent._money;
                    UpdateUI(money._balance);
                    _moneyProcessedFlagPool.Add(deleteMoneyEventEntity);
                    SaveToPlayerPrefs(money._balance);

                }
            }
        }
    }


    private void UpdateUI(float money)
    {
        _moneyText.text = $"Баланс: {money}$";
    }
    private void SaveToPlayerPrefs(float money)
    {
        PlayerPrefs.SetFloat("Money", money);
        PlayerPrefs.Save();
    }
}
