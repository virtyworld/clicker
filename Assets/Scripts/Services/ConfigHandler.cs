using System.Collections.Generic;

using UnityEngine;

public class ConfigHandler : MonoBehaviour
{
    [SerializeField] private List<BusinessConfigSO> businessConfigSO;

    public BusinessConfigSO GetConfig(int id)
    {
        return businessConfigSO[id];
    }
}
