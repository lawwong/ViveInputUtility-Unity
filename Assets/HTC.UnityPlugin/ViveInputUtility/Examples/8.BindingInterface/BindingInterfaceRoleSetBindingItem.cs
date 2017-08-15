using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BindingInterfaceRoleSetBindingItem : MonoBehaviour
{
    public const string MODEL_SPRITE_PREFIX = "binding_ui_icons_";

    [SerializeField]
    private Image m_modelIcon;
    [SerializeField]
    private Text m_deviceSN;
    [SerializeField]
    private Text m_roleName;
    [SerializeField]
    private Button m_editButton;

    public string deviceSN { get; set; }
    public bool isEditing { get { return m_editButton.interactable; } set { m_editButton.interactable = !value; } }
    public event Action<string> onEditPress;
    public event Action<string> onRemovePress;

    public void RefreshDisplayInfo(ViveRole.IMap roleMap)
    {
        var roleInfo = roleMap.RoleValueInfo;
        var roleValue = roleMap.GetBoundRoleValueByDevice(deviceSN);
        var deviceModel = ViveRoleBindingsHelper.GetDeviceModelHint(deviceSN);

        m_deviceSN.text = deviceSN;
        m_roleName.text = roleInfo.GetNameByRoleValue(roleValue);

        string spriteName;
        switch (deviceModel)
        {
            case VRModuleDeviceModel.KnucklesLeft:
                spriteName = MODEL_SPRITE_PREFIX + VRModuleDeviceModel.KnucklesRight;
                m_modelIcon.transform.localScale = new Vector3(-1f, 1f, 1f);
                break;
            case VRModuleDeviceModel.OculusTouchLeft:
                spriteName = MODEL_SPRITE_PREFIX + VRModuleDeviceModel.OculusTouchRight;
                m_modelIcon.transform.localScale = new Vector3(-1f, 1f, 1f);
                break;
            default:
                spriteName = MODEL_SPRITE_PREFIX + deviceModel.ToString();
                m_modelIcon.transform.localScale = new Vector3(1f, 1f, 1f);
                break;
        }

        m_modelIcon.sprite = BindingInterfaceSpriteManager.GetSprite(spriteName);

        if (VRModule.IsDeviceConnected(deviceSN))
        {
            m_modelIcon.color = new Color32(0x53, 0xBB, 0x00, 0xFF);
        }
        else
        {
            m_modelIcon.color = new Color32(0x56, 0x56, 0x56, 0xFF);
        }
    }

    public void OnEdit()
    {
        if (onEditPress != null) { onEditPress(deviceSN); }
    }

    public void OnRemove()
    {
        if (onRemovePress != null) { onRemovePress(deviceSN); }
    }
}
