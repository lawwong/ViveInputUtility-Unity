using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BindingInterfaceRoleSetPanelController : MonoBehaviour
{
    public Type DEFAULT_SELECTED_ROLE = typeof(BodyRole);

    [SerializeField]
    private BindingInterfaceRoleSetButtonItem m_roleButtonItem;
    [SerializeField]
    private BindingInterfaceRoleSetBindingItem m_bindingItem;

    private int m_maxBindingCount;
    private List<BindingInterfaceRoleSetButtonItem> m_roleButtonList = new List<BindingInterfaceRoleSetButtonItem>();
    private int m_selectedIndex = -1;

    private List<BindingInterfaceRoleSetBindingItem> m_bindingList = new List<BindingInterfaceRoleSetBindingItem>();

    public ViveRole.IMap selectedRoleMap { get { return m_roleButtonList[m_selectedIndex].roleMap; } }

    private void Awake()
    {
        ViveRole.Initialize();
        ViveRoleBindingsHelper.AutoLoadConfig();

        RefreshRoleSelection();

        // select the role that have largest binding count
        for (int i = 0, imax = m_roleButtonList.Count; i < imax; ++i)
        {
            if (!m_roleButtonList[i].roleMap.Handler.BlockBindings && m_roleButtonList[i].roleMap.BindingCount == m_maxBindingCount)
            {
                SelectRole(i);
                break;
            }
        }
    }

    private void OnEnable()
    {
        VRModule.onDeviceConnected += OnDeviceConnected;
    }

    private void OnDisable()
    {
        VRModule.onDeviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(uint deviceIndex, bool connected)
    {
        RefreshSelectedRoleBindings();
    }

    public void DisableSelection()
    {
        for (int i = 0, imax = m_roleButtonList.Count; i < imax; ++i)
        {
            m_roleButtonList[i].interactable = false;
        }
    }

    public void EableSelection()
    {
        for (int i = 0, imax = m_roleButtonList.Count; i < imax; ++i)
        {
            m_roleButtonList[i].interactable = true;
        }
    }

    public void SelectRole(int index)
    {
        if (!m_roleButtonList[index].isOn)
        {
            m_roleButtonList[index].isOn = true;
        }

        if (m_selectedIndex == index) { return; }

        m_selectedIndex = index;

        RefreshSelectedRoleBindings();
    }

    public void EditBinding(int index)
    {

    }

    public void RemoveBinding(int index)
    {

    }

    public void AddBinding()
    {

    }

    public void RefreshRoleSelection()
    {
        if (m_roleButtonList.Count == 0)
        {
            m_roleButtonList.Add(m_roleButtonItem);
            m_roleButtonItem.isOn = false;
            m_roleButtonItem.index = 0;
            m_roleButtonItem.onSelected += SelectRole;
        }

        m_maxBindingCount = 0;
        var buttonIndex = 0;
        for (int i = 0, imax = ViveRoleEnum.ValidViveRoleTable.Count; i < imax; ++i)
        {
            var roleType = ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i);
            var roleMap = ViveRole.GetMap(roleType);

            if (roleMap.Handler.BlockBindings) { continue; }

            BindingInterfaceRoleSetButtonItem item;
            if (buttonIndex >= m_roleButtonList.Count)
            {
                var itemObj = Instantiate(m_roleButtonItem.gameObject);
                itemObj.transform.SetParent(m_roleButtonItem.transform.parent, false);

                m_roleButtonList.Add(item = itemObj.GetComponent<BindingInterfaceRoleSetButtonItem>());
                item.isOn = false;
                item.index = buttonIndex;
                item.onSelected += SelectRole;
            }
            else
            {
                item = m_roleButtonList[buttonIndex];
            }

            m_maxBindingCount = Mathf.Max(m_maxBindingCount, roleMap.BindingCount);
            item.roleMap = roleMap;

            ++buttonIndex;
        }
    }

    public void RefreshSelectedRoleBindings()
    {
        if (m_bindingList.Count == 0)
        {
            m_bindingList.Add(m_bindingItem);
            m_bindingItem.index = 0;
        }

        var bindingIndex = 0;
        var roleMap = m_roleButtonList[m_selectedIndex].roleMap;
        for (int max = roleMap.BindingCount; bindingIndex < max; ++bindingIndex)
        {
            BindingInterfaceRoleSetBindingItem item;
            if (bindingIndex >= m_bindingList.Count)
            {
                var itemObj = Instantiate(m_bindingItem.gameObject);
                itemObj.transform.SetParent(m_bindingItem.transform.parent, false);

                // set child index to secnd last, last index is for add item
                itemObj.transform.SetSiblingIndex(itemObj.transform.parent.childCount - 2);

                m_bindingList.Add(item = itemObj.GetComponent<BindingInterfaceRoleSetBindingItem>());
                item.index = bindingIndex;
            }
            else
            {
                item = m_bindingList[bindingIndex];
            }

            item.gameObject.SetActive(true);
            item.RefreshDisplayInfo(roleMap);
        }

        for (int max = m_bindingList.Count; bindingIndex < max; ++bindingIndex)
        {
            m_bindingList[bindingIndex].gameObject.SetActive(false);
        }
    }
}
