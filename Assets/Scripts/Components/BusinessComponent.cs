
using UnityEngine;

public struct BusinessComponent
{
    public int _id;               // ID бизнеса (0–4)
    public float _incomeDelay;    // Задержка дохода (сек)
    public int _baseCost;         // Базовая стоимость
    public int _baseIncome;       // Базовый доход
    public BusinessView _view;
}
