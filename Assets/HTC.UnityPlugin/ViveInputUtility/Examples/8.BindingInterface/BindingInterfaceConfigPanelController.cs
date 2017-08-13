using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.UI;

public class BindingInterfaceConfigPanelController : MonoBehaviour
{
    [SerializeField]
    private Text m_textConfigPath;
    [SerializeField]
    private Toggle m_toggleApplyOnStart;

    private void Awake()
    {
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
