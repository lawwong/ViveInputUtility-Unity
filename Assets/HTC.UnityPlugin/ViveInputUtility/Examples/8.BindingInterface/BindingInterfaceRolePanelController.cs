using HTC.UnityPlugin.Vive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BindingInterfaceRolePanelController : MonoBehaviour
{
    [Serializable]
    public class UnityEventBinding : UnityEvent<ViveRole.IMap, string> { }

    [SerializeField]
    private BindingInterfaceRoleButtonItem m_roleButtonItem;
    [SerializeField]
    private UnityEventBinding m_onBoundDevcieToRole;

    private List<BindingInterfaceRoleButtonItem> m_roleButtonList = new List<BindingInterfaceRoleButtonItem>();
    private ViveRole.IMap m_editingRoleMap;
    private string m_editingDeviceSN;

    public void SelectRole(int roleValue)
    {
        m_editingRoleMap.BindDeviceToRoleValue(m_editingDeviceSN, roleValue);

        if (m_onBoundDevcieToRole != null)
        {
            m_onBoundDevcieToRole.Invoke(m_editingRoleMap, m_editingDeviceSN);
        }
    }

    public void RefreshForEditBinding(ViveRole.IMap roleMap, string deviceSN)
    {
        if (m_roleButtonList.Count == 0)
        {
            m_roleButtonList.Add(m_roleButtonItem);
            m_roleButtonItem.onValueChanged += SelectRole;
        }

        var roleInfo = roleMap.RoleValueInfo;

        // update buttons
        if (m_editingRoleMap != roleMap)
        {
            m_editingRoleMap = roleMap;

            m_roleButtonList[0].roleValue = roleInfo.InvalidRoleValue;
            m_roleButtonList[0].roleName = roleInfo.GetNameByRoleValue(roleInfo.InvalidRoleValue);

            var buttonIndex = 1;
            for (int roleValue = roleInfo.MinValidRoleValue, max = roleInfo.MaxValidRoleValue; roleValue <= max; ++roleValue)
            {
                if (!roleInfo.IsValidRoleValue(roleValue)) { continue; }

                BindingInterfaceRoleButtonItem item;
                if (buttonIndex >= m_roleButtonList.Count)
                {
                    var itemObj = Instantiate(m_roleButtonItem.gameObject);
                    itemObj.transform.SetParent(m_roleButtonItem.transform.parent, false);

                    m_roleButtonList.Add(item = itemObj.GetComponent<BindingInterfaceRoleButtonItem>());
                    item.onValueChanged += SelectRole;
                }
                else
                {
                    item = m_roleButtonList[buttonIndex];
                }

                item.gameObject.SetActive(true);
                item.roleValue = roleValue;
                item.roleName = roleInfo.GetNameByRoleValue(roleValue);

                ++buttonIndex;
            }

            for (int max = m_roleButtonList.Count; buttonIndex < max; ++buttonIndex)
            {
                m_roleButtonList[buttonIndex].gameObject.SetActive(false);
            }
        }

        // update selected role
        m_editingDeviceSN = deviceSN;
        if (roleMap.IsDeviceBound(deviceSN))
        {
            var validRoleFound = false;
            var boundRoleValue = roleMap.GetBoundRoleValueByDevice(deviceSN);
            for (int i = 0, imax = m_roleButtonList.Count; i < imax && m_roleButtonList[i].gameObject.activeSelf; ++i)
            {
                if (m_roleButtonList[i].roleValue == boundRoleValue)
                {
                    m_roleButtonList[i].SetIsOn();
                    validRoleFound = true;
                    break;
                }
            }

            if (!validRoleFound)
            {
                m_roleButtonList[0].SetIsOn();
            }
        }
        else
        {
            m_roleButtonList[0].SetIsOn();
        }
    }
}
