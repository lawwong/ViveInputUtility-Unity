using System;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.Vive;

public class BindingInterfaceRoleSetButtonItem : MonoBehaviour
{
    [SerializeField]
    private Toggle m_toggle;
    [SerializeField]
    private Text m_textName;

    private ViveRole.IMap m_roleMap;

    public event Action<int> onSelected;

    public bool isOn { get { return m_toggle.isOn; } set { m_toggle.isOn = value; } }
    public bool interactable { get { return m_toggle.interactable; } set { m_toggle.interactable = value; } }
    public int index { get; set; }

    public ViveRole.IMap roleMap
    {
        get { return m_roleMap; }
        set
        {
            m_roleMap = value;
            
            if (m_roleMap.BindingCount > 0)
            {
                m_textName.text = value.RoleValueInfo.RoleEnumType.Name + "(" + value.BindingCount + ")";
            }
            else
            {
                m_textName.text = value.RoleValueInfo.RoleEnumType.Name;
            }
        }
    }

    public void OnValueChanged(bool isOn)
    {
        if (isOn)
        {
            if (onSelected != null) { onSelected(index); }
        }
    }
}
