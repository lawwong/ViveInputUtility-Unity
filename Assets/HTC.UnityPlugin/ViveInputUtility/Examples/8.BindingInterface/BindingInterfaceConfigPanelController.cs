using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BindingInterfaceConfigPanelController : MonoBehaviour
{
    [SerializeField]
    private Toggle m_toggleApplyOnStart;

    private void Awake()
    {
        if (EventSystem.current == null)
        {
            new GameObject("[EventSystem]", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        else if (EventSystem.current.GetComponent<StandaloneInputModule>() == null)
        {
            EventSystem.current.gameObject.AddComponent<StandaloneInputModule>();
        }

        ViveRoleBindingsHelper.AutoLoadConfig();

        Refresh();
    }

    private void Refresh()
    {
        m_toggleApplyOnStart.isOn = ViveRoleBindingsHelper.bindingConfig.apply_bindings_on_load;
    }

    public void LoadConfig()
    {
        ViveRoleBindingsHelper.LoadBindingConfigFromFile(ViveRoleBindingsHelper.AUTO_LOAD_CONFIG_PATH);
        ViveRoleBindingsHelper.ApplyBindingConfigToRoleMap();

        Refresh();
    }

    public void SaveConfig()
    {
        ViveRoleBindingsHelper.LoadBindingConfigFromRoleMap();
        ViveRoleBindingsHelper.SaveBindingConfigToFile(ViveRoleBindingsHelper.AUTO_LOAD_CONFIG_PATH);
    }
}
