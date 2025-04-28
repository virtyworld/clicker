using UnityEngine;

[CreateAssetMenu(fileName = "BusinessConfig", menuName = "Configs/Business")]
public class BusinessConfigSO : ScriptableObject
{
    public string _businessName;

    [Header("Бизнес параметры")]
    public float _incomeDelay;
    public int _baseCost;
    public int _baseIncome;

    [Header("Улучшение 1")]
    public UpgradeData _upgrade1;

    [Header("Улучшение 2")]
    public UpgradeData _upgrade2;
}
