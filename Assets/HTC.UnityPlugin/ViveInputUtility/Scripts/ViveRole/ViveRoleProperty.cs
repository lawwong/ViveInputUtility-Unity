//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.VRModuleManagement;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HTC.UnityPlugin.Vive
{
    // ViveRoleProperty is a serializable class that preserve vive role using 2 strings.
    // There also has a property drawer so you can use it as a serialized field in your MonoBevaviour.
    // Note that when deserializing, result of type and value is based on the valid role info stored in ViveRoleEnum class
    [Serializable]
    public class ViveRoleProperty : ISerializationCallbackReceiver
    {
        public delegate void RoleChangedListener();
        public delegate void DeviceIndexChangedListener(uint deviceIndex);

        public static readonly Type DefaultRoleType = typeof(DeviceRole);
        public static readonly int DefaultRoleValue = (int)DeviceRole.Hmd;

        [SerializeField]
        private string m_roleTypeFullName;
        [SerializeField]
        private string m_roleValueName;

        private bool m_isTypeDirty = true;
        private bool m_isValueDirty = true;

        private Type m_roleType = DefaultRoleType;
        private int m_roleValue = DefaultRoleValue;
        private uint m_sentDeviceIndex = VRModule.INVALID_DEVICE_INDEX;

        private ViveRole.IMap m_roleMap = null;

        private Action m_onChanged;
        private RoleChangedListener m_onRoleChanged;

        private bool m_monitoringIndexChanged;
        private DeviceIndexChangedListener m_onDeviceIndexChanged;

        public Type roleType
        {
            get
            {
                Update();
                return m_roleType;
            }
            set
            {
                Update();
                m_isTypeDirty |= ChangeProp.Set(ref m_roleTypeFullName, value.FullName);
                Update();
            }
        }

        public int roleValue
        {
            get
            {
                Update();
                return m_roleValue;
            }
            set
            {
                Update();
                m_isValueDirty |= ChangeProp.Set(ref m_roleValueName, ViveRoleEnum.GetInfo(m_roleType).GetNameByRoleValue(value));
                Update();
            }
        }

        public string roleTypeFullName { get { return m_roleTypeFullName; } }

        public string roleValueName { get { return m_roleValueName; } }

        [Obsolete("Use onRoleChanged instead")]
        public event Action Changed
        {
            add { m_onChanged += value; }
            remove { m_onChanged -= value; }
        }

        public event RoleChangedListener onRoleChanged
        {
            add { m_onRoleChanged += value; }
            remove { m_onRoleChanged -= value; }
        }

        public event DeviceIndexChangedListener onDeviceIndexChanged
        {
            add
            {
                if (Application.isPlaying)
                {
                    Update();

                    if (m_roleMap != null && m_onDeviceIndexChanged == null)
                    {
                        m_roleMap.OnRoleValueMappingChanged += OnMappingChanged;
                        m_monitoringIndexChanged = true;
                    }

                    m_onDeviceIndexChanged += value;
                }
            }
            remove
            {
                if (Application.isPlaying)
                {
                    m_onDeviceIndexChanged -= value;

                    if (m_roleMap != null && m_onDeviceIndexChanged == null)
                    {
                        m_monitoringIndexChanged = false;
                        m_roleMap.OnRoleValueMappingChanged -= OnMappingChanged;
                    }
                }
            }
        }

        public static ViveRoleProperty New()
        {
            return New(DefaultRoleType, DefaultRoleValue);
        }

        public static ViveRoleProperty New(Type type, int value)
        {
            return New(type.FullName, ViveRoleEnum.GetInfo(type).GetNameByRoleValue(value));
        }

        public static ViveRoleProperty New<TRole>(TRole role)
        {
            return New(typeof(TRole).FullName, role.ToString());
        }

        public static ViveRoleProperty New(string typeFullName, string valueName)
        {
            var prop = new ViveRoleProperty();
            prop.m_roleTypeFullName = typeFullName;
            prop.m_roleValueName = valueName;
            return prop;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            SetTypeDirty();
            SetValueDirty();
        }

        public void SetTypeDirty() { m_isTypeDirty = true; }
        public void SetValueDirty() { m_isValueDirty = true; }

        private void OnMappingChanged(ViveRole.IMap map, ViveRole.MappingChangedEventArg args)
        {
            if (args.roleValue == m_roleValue)
            {
                Update();
            }
        }

        // update type and value if type string or value string is/are dirty
        private void Update()
        {
            var roleChanged = false;
            var indexChanged = false;

            if (m_isTypeDirty || m_isValueDirty)
            {
                if (m_isTypeDirty)
                {
                    m_isTypeDirty = false;

                    Type newType;
                    if (string.IsNullOrEmpty(m_roleTypeFullName) || !ViveRoleEnum.ValidViveRoleTable.TryGetValue(m_roleTypeFullName, out newType))
                    {
                        newType = DefaultRoleType;
                    }

                    roleChanged = ChangeProp.Set(ref m_roleType, newType);

                    // maintain m_roleMap cache
                    if (roleChanged || (m_roleMap == null && newType != null))
                    {
                        if (m_roleMap != null && m_onDeviceIndexChanged != null)
                        {
                            m_monitoringIndexChanged = false;
                            m_roleMap.OnRoleValueMappingChanged -= OnMappingChanged;
                        }

                        m_roleMap = ViveRole.GetMap(m_roleType);

                        if (m_roleMap != null && m_onDeviceIndexChanged != null)
                        {
                            m_roleMap.OnRoleValueMappingChanged += OnMappingChanged;
                            m_monitoringIndexChanged = true;
                        }
                    }
                }

                if (m_isValueDirty || roleChanged)
                {
                    m_isValueDirty = false;

                    int newValue;
                    var info = ViveRoleEnum.GetInfo(m_roleType);
                    if (string.IsNullOrEmpty(m_roleValueName) || !info.TryGetRoleValueByName(m_roleValueName, out newValue))
                    {
                        newValue = info.InvalidRoleValue;
                    }

                    roleChanged |= ChangeProp.Set(ref m_roleValue, newValue);
                }
            }

            if (m_monitoringIndexChanged)
            {
                var currentDeviceIndex = m_roleMap == null ? VRModule.INVALID_DEVICE_INDEX : m_roleMap.GetMappedDeviceByRoleValue(m_roleValue);
                if (VRModule.IsValidDeviceIndex(m_sentDeviceIndex) || VRModule.IsValidDeviceIndex(currentDeviceIndex))
                {
                    indexChanged = ChangeProp.Set(ref m_sentDeviceIndex, currentDeviceIndex);
                }
            }

            if (roleChanged)
            {
                if (m_onChanged != null)
                {
                    m_onChanged();
                }

                if (m_onRoleChanged != null)
                {
                    m_onRoleChanged();
                }
            }

            if (indexChanged)
            {
                if (m_onDeviceIndexChanged != null)
                {
                    m_onDeviceIndexChanged(m_sentDeviceIndex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRole">Can be DeviceRole, HandRole or TrackerRole</typeparam>
        /// <param name="role"></param>
        public void SetEx<TRole>(TRole role)
        {
            Set(typeof(TRole).FullName, role.ToString());
        }

        public void Set(Type type, int value)
        {
            Set(type.FullName, ViveRoleEnum.GetInfo(type).GetNameByRoleValue(value));
        }

        public void Set(ViveRoleProperty prop)
        {
            Set(prop.m_roleTypeFullName, prop.m_roleValueName);
        }

        // set by value name to preserve the enum element, since different enum element could have same enum value
        public void Set(string typeFullName, string valueName)
        {
            m_isTypeDirty |= ChangeProp.Set(ref m_roleTypeFullName, typeFullName);
            m_isValueDirty |= ChangeProp.Set(ref m_roleValueName, valueName);

            Update();
        }

        public uint GetDeviceIndex()
        {
            Update();

            if (m_monitoringIndexChanged)
            {
                return m_sentDeviceIndex;
            }
            else if (m_roleMap != null)
            {
                return m_roleMap.GetMappedDeviceByRoleValue(m_roleValue);
            }
            else
            {
                return VRModule.INVALID_DEVICE_INDEX;
            }
        }

        public TRole ToRole<TRole>()
        {
            Update();

            TRole role;
            var roleInfo = ViveRoleEnum.GetInfo<TRole>();
            if (m_roleType != typeof(TRole) || !roleInfo.TryGetRoleByName(m_roleValueName, out role))
            {
                // return invalid if role type not match or the value name not found in roleInfo
                return roleInfo.InvalidRole;
            }

            return role;
        }

        public bool IsRole(Type type, int value)
        {
            Update();

            return m_roleType == type && m_roleValue == value;
        }

        public bool IsRole<TRole>(TRole role)
        {
            Update();

            if (m_roleType != typeof(TRole)) { return false; }
            var roleInfo = ViveRoleEnum.GetInfo<TRole>();

            return m_roleValue == roleInfo.ToRoleValue(role);
        }

        public static bool operator ==(ViveRoleProperty p1, ViveRoleProperty p2)
        {
            if (ReferenceEquals(p1, p2)) { return true; }
            if (ReferenceEquals(p1, null)) { return false; }
            if (ReferenceEquals(p2, null)) { return false; }
            if (p1.roleType != p2.roleType) { return false; }
            if (p1.roleValue != p2.roleValue) { return false; }
            return true;
        }

        public static bool operator !=(ViveRoleProperty p1, ViveRoleProperty p2)
        {
            return !(p1 == p2);
        }

        public bool Equals(ViveRoleProperty prop)
        {
            return this == prop;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ViveRoleProperty);
        }

        public override int GetHashCode()
        {
            Update();

            var hash = 17;
            hash = hash * 23 + (m_roleType == null ? 0 : m_roleType.GetHashCode());
            hash = hash * 23 + m_roleValue.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            Update();
            return m_roleType.Name + "." + ViveRoleEnum.GetInfo(m_roleType).GetNameByRoleValue(m_roleValue);
        }
    }
}