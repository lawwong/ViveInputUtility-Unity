using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BindingInterfaceRoleSetPanelController : MonoBehaviour
{
    public Type DEFAULT_SELECTED_ROLE = typeof(BodyRole);

    [Serializable]
    public class UnityEventBinding : UnityEvent<ViveRole.IMap, string> { }

    [SerializeField]
    private BindingInterfaceRoleSetButtonItem m_roleSetButtonItem;
    [SerializeField]
    private BindingInterfaceRoleSetBindingItem m_bindingItem;
    [SerializeField]
    private UnityEventBinding m_onEditBinding;
    [SerializeField]
    private UnityEvent m_onFinishEditBinding;

    private int m_maxBindingCount;
    private List<BindingInterfaceRoleSetButtonItem> m_roleSetButtonList = new List<BindingInterfaceRoleSetButtonItem>();
    private int m_selectedRoleIndex = -1;

    private List<BindingInterfaceRoleSetBindingItem> m_bindingList = new List<BindingInterfaceRoleSetBindingItem>();
    private string m_editingDevice = string.Empty;
    private OrderedIndexedSet<string> m_boundDevices = new OrderedIndexedSet<string>();

    public ViveRole.IMap selectedRoleMap { get { return m_roleSetButtonList[m_selectedRoleIndex].roleMap; } }
    public string editingDevice { get { return m_editingDevice; } }
    public bool isEditing { get { return !string.IsNullOrEmpty(m_editingDevice); } }

    private void Awake()
    {
        ViveRole.Initialize();
        ViveRoleBindingsHelper.AutoLoadConfig();

        RefreshRoleSelection();

        // select the role that have largest binding count
        for (int i = 0, imax = m_roleSetButtonList.Count; i < imax; ++i)
        {
            if (!m_roleSetButtonList[i].roleMap.Handler.BlockBindings && m_roleSetButtonList[i].roleMap.BindingCount == m_maxBindingCount)
            {
                SelectRoleSet(i);
                break;
            }
        }
    }

    public void Test(ViveRole.IMap roleMap, string deviceSN) { }

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
        for (int i = 0, imax = m_roleSetButtonList.Count; i < imax; ++i)
        {
            m_roleSetButtonList[i].interactable = false;
        }
    }

    public void EableSelection()
    {
        for (int i = 0, imax = m_roleSetButtonList.Count; i < imax; ++i)
        {
            m_roleSetButtonList[i].interactable = true;
        }
    }

    public void SelectRoleSet(int index)
    {
        m_roleSetButtonList[index].SetIsOn();

        if (m_selectedRoleIndex == index) { return; }

        m_selectedRoleIndex = index;

        m_boundDevices.Clear();

        RefreshSelectedRoleBindings();
    }

    public void StartEditBinding(string deviceSN)
    {
        if (isEditing)
        {
            InternalFinishEditBinding();
        }

        m_editingDevice = deviceSN;
        m_bindingList[m_boundDevices.IndexOf(deviceSN)].isEditing = true;

        if (m_onEditBinding != null)
        {
            m_onEditBinding.Invoke(selectedRoleMap, editingDevice);
        }
    }

    private void InternalFinishEditBinding()
    {
        if (isEditing)
        {
            m_bindingList[m_boundDevices.IndexOf(m_editingDevice)].isEditing = false;
        }

        m_editingDevice = string.Empty;
    }

    public void FinishEditBinding()
    {
        InternalFinishEditBinding();

        if (m_onFinishEditBinding != null)
        {
            m_onFinishEditBinding.Invoke();
        }
    }

    public void RemoveBinding(string deviceSN)
    {
        if (isEditing && m_editingDevice == deviceSN)
        {
            FinishEditBinding();
        }

        selectedRoleMap.UnbindDevice(deviceSN);

        RefreshRoleSelection();
        RefreshSelectedRoleBindings();
    }

    public void AddBinding()
    {

    }

    public void RefreshRoleSelection()
    {
        if (m_roleSetButtonList.Count == 0)
        {
            m_roleSetButtonList.Add(m_roleSetButtonItem);
            m_roleSetButtonItem.index = 0;
            m_roleSetButtonItem.onSelected += SelectRoleSet;
        }

        m_maxBindingCount = 0;
        var buttonIndex = 0;
        for (int i = 0, imax = ViveRoleEnum.ValidViveRoleTable.Count; i < imax; ++i)
        {
            var roleType = ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i);
            var roleMap = ViveRole.GetMap(roleType);

            if (roleMap.Handler.BlockBindings) { continue; }

            BindingInterfaceRoleSetButtonItem item;
            if (buttonIndex >= m_roleSetButtonList.Count)
            {
                var itemObj = Instantiate(m_roleSetButtonItem.gameObject);
                itemObj.transform.SetParent(m_roleSetButtonItem.transform.parent, false);

                m_roleSetButtonList.Add(item = itemObj.GetComponent<BindingInterfaceRoleSetButtonItem>());
                item.index = buttonIndex;
                item.onSelected += SelectRoleSet;
            }
            else
            {
                item = m_roleSetButtonList[buttonIndex];
            }

            m_maxBindingCount = Mathf.Max(m_maxBindingCount, roleMap.BindingCount);
            item.roleMap = roleMap;

            ++buttonIndex;
        }
    }

    private IndexedSet<string> m_tempDevices = new OrderedIndexedSet<string>();
    public void RefreshSelectedRoleBindings()
    {
        var roleMap = m_roleSetButtonList[m_selectedRoleIndex].roleMap;
        var bindingTable = roleMap.BindingTable;

        // update bound device list and keep the original order
        for (int i = 0, imax = m_boundDevices.Count; i < imax; ++i) { m_tempDevices.Add(m_boundDevices[i]); }
        for (int i = 0, imax = bindingTable.Count; i < imax; ++i)
        {
            var boundDevice = bindingTable.GetKeyByIndex(i);
            if (!m_tempDevices.Remove(boundDevice))
            {
                m_boundDevices.Add(boundDevice);
            }
        }
        for (int i = 0, imax = m_tempDevices.Count; i < imax; ++i) { m_boundDevices.Remove(m_tempDevices[i]); }
        m_tempDevices.Clear();

        if (m_bindingList.Count == 0)
        {
            m_bindingList.Add(m_bindingItem);
            m_bindingItem.onEditPress += StartEditBinding;
            m_bindingItem.onRemovePress += RemoveBinding;
        }

        var bindingIndex = 0;
        for (int max = m_boundDevices.Count; bindingIndex < max; ++bindingIndex)
        {
            BindingInterfaceRoleSetBindingItem item;
            if (bindingIndex >= m_bindingList.Count)
            {
                var itemObj = Instantiate(m_bindingItem.gameObject);
                itemObj.transform.SetParent(m_bindingItem.transform.parent, false);

                // set child index to secnd last, last index is for add item
                itemObj.transform.SetSiblingIndex(itemObj.transform.parent.childCount - 2);

                m_bindingList.Add(item = itemObj.GetComponent<BindingInterfaceRoleSetBindingItem>());
                item.onEditPress += StartEditBinding;
                item.onRemovePress += RemoveBinding;
            }
            else
            {
                item = m_bindingList[bindingIndex];
            }

            item.gameObject.SetActive(true);
            item.deviceSN = m_boundDevices[bindingIndex];
            item.isEditing = isEditing && item.deviceSN == m_editingDevice;
            item.RefreshDisplayInfo(roleMap);
        }

        for (int max = m_bindingList.Count; bindingIndex < max; ++bindingIndex)
        {
            m_bindingList[bindingIndex].gameObject.SetActive(false);
        }
    }
}
