
using Leopotam.EcsLite;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleTMP;
    [SerializeField] private Button _upgrade1Button;
    [SerializeField] private Button _upgrade2Button;
    [SerializeField] private Button _lvlUpButton;
    [SerializeField] private TextMeshProUGUI _lvlUpButtonText;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _incomeText;
    [SerializeField] private TextMeshProUGUI _lvlText;
    [SerializeField] private TextMeshProUGUI _upgrade1TMP;
    [SerializeField] private TextMeshProUGUI _upgrade2TMP;

    private int _businessId;
    private EcsPackedEntity _ecsPackedEntity;
    private EcsWorld _world;
    public void Setup(EcsWorld world, EcsPackedEntity ecsPackedEntity, int businessId)
    {
        _ecsPackedEntity = ecsPackedEntity;
        _businessId = businessId;
        _world = world;
        _upgrade1Button.onClick.AddListener(OnUpgrade1ButtonClick);
        _upgrade2Button.onClick.AddListener(OnUpgrade2ButtonClick);
        _lvlUpButton.onClick.AddListener(OnLvlUpButtonClick);
    }

    private void OnUpgrade1ButtonClick()
    {
        ref var uec = ref _world.GetPool<UpgradeEventComponent>().Add(_world.NewEntity());
        uec._businessId = _businessId;
        uec._upgradeType = 0;
        uec._ecsPackedEntity = _ecsPackedEntity;
    }

    private void OnUpgrade2ButtonClick()
    {
        ref var uec = ref _world.GetPool<UpgradeEventComponent>().Add(_world.NewEntity());
        uec._businessId = _businessId;
        uec._upgradeType = 1;
        uec._ecsPackedEntity = _ecsPackedEntity;
    }

    private void OnLvlUpButtonClick()
    {
        ref var luec = ref _world.GetPool<LvlUpEventComponent>().Add(_world.NewEntity());
        luec._businessId = _businessId;
        luec._ecsPackedEntity = _ecsPackedEntity;

    }
    public void UpdateTitle(string text)
    {
        _titleTMP.text = text;
    }
    public void UpdateProgress(float value)
    {
        _progressSlider.value = value;
    }
    public void UpdateLvl(string text)
    {
        _lvlText.text = text;
    }
    public void EnableLvlUpButton()
    {
        _lvlUpButton.interactable = true;
    }
    public void DisableLvlUpButton()
    {
        _lvlUpButton.interactable = false;
    }
    public void UpdateLvlUpButton(string text)
    {
        _lvlUpButtonText.text = text;
    }
    public void UpdateIncome(string text)
    {
        _incomeText.text = text;
    }
    public void UpdateUpgrade1(string text)
    {
        _upgrade1TMP.text = text;
    }
    public void UpdateUpgrade2(string text)
    {
        _upgrade2TMP.text = text;
    }
    public void EnableUpgrade1Button()
    {
        _upgrade1Button.interactable = true;
    }
    public void DisableUpgrade1Button()
    {
        _upgrade1Button.interactable = false;
    }
    public void EnableUpgrade2Button()
    {
        _upgrade2Button.interactable = true;
    }
    public void DisableUpgrade2Button()
    {
        _upgrade2Button.interactable = false;
    }

}
