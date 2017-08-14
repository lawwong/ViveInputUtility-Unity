using UnityEngine;
using System.Collections;
using HTC.UnityPlugin.Vive;
using UnityEngine.UI;
using System;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections.Generic;

public class BindingInterfaceRoleSetBindingItem : MonoBehaviour
{
    public const string MODEL_SPRITE_PREFIX = "binding_ui_icons_";

    [SerializeField]
    private Image m_modelIcon;
    [SerializeField]
    private Text m_deviceSN;
    [SerializeField]
    private Text m_roleName;

    public int index { get; set; }
    public event Action<int> onEdit;
    public event Action<int> onRemove;

    public void RefreshDisplayInfo(ViveRole.IMap roleMap)
    {
        var roleInfo = roleMap.RoleValueInfo;
        var deviceSN = roleMap.BindingTable.GetKeyByIndex(index);
        var roleValue = roleMap.BindingTable.GetValueByIndex(index);
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
        //if(OnEdit)
    }

    public void OnRemove()
    {

    }
}
