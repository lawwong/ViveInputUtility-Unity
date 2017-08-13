//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.VRModuleManagement;
using HTC.UnityPlugin.Utility;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace HTC.UnityPlugin.Vive
{
    public class ViveRoleBindingsHelper : SingletonBehaviour<ViveRoleBindingsHelper>
    {
        [Serializable]
        public struct Binding
        {
            [FormerlySerializedAs("sn")]
            public string device_sn;
            public VRModuleDeviceModel device_model;
            public string role_name;
            [FormerlySerializedAs("sv")]
            public int role_value;
        }

        [Serializable]
        public struct RoleData
        {
            public string type;
            public Binding[] bindings;
        }

        [Serializable]
        public class BindingConfig
        {
            public bool apply_bindings_on_load = true;
            public string toggle_interface_key_code = KeyCode.B.ToString();
            public string toggle_interface_modifier = KeyCode.RightShift.ToString();
            public string interface_prefab = DEFAULT_INTERFACE_PREFAB;
            public RoleData[] roles = new RoleData[0];
        }

        public const string AUTO_LOAD_CONFIG_PATH = "vive_role_bindings.cfg";
        public const string DEFAULT_INTERFACE_PREFAB = "VIUBindingInterface";

        private static bool s_isAutoLoaded;
        private static BindingConfig s_bindingConfig = new BindingConfig();

        private static KeyCode s_toggleKey;
        private static KeyCode s_toggleModifier;
        private static GameObject s_interfaceObj;

        public static BindingConfig bindingConfig { get { return s_bindingConfig; } }

        [SerializeField]
        private string m_overrideConfigPath = AUTO_LOAD_CONFIG_PATH;

        [RuntimeInitializeOnLoadMethod]
        public static void AutoLoadConfig()
        {
            if (s_isAutoLoaded) { return; }
            s_isAutoLoaded = true;

            var configPath = AUTO_LOAD_CONFIG_PATH;

            if (Active && string.IsNullOrEmpty(Instance.m_overrideConfigPath))
            {
                configPath = Instance.m_overrideConfigPath;
            }

            if (File.Exists(configPath))
            {
                LoadBindingConfigFromFile(configPath);

                if (s_bindingConfig.apply_bindings_on_load)
                {
                    var appliedCount = ApplyBindingConfigToRoleMap();

                    Debug.Log("ViveRoleBindingsHelper: " + appliedCount + " bindings applied from " + configPath);
                }
            }
            else
            {
                UpdateInterfaceKeyMonitor();
            }
        }

        private static void UpdateInterfaceKeyMonitor()
        {
            Debug.Log("UpdateInterfaceKeyMonitor " + s_bindingConfig.toggle_interface_modifier + " " + s_bindingConfig.toggle_interface_key_code);
            // Moniter input key to open up the binding interface
            if (!string.IsNullOrEmpty(s_bindingConfig.toggle_interface_key_code) && Enum.IsDefined(typeof(KeyCode), s_bindingConfig.toggle_interface_key_code))
            {
                s_toggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), s_bindingConfig.toggle_interface_key_code);

                if (!string.IsNullOrEmpty(s_bindingConfig.toggle_interface_modifier) && Enum.IsDefined(typeof(KeyCode), s_bindingConfig.toggle_interface_modifier))
                {
                    s_toggleModifier = (KeyCode)Enum.Parse(typeof(KeyCode), s_bindingConfig.toggle_interface_modifier);
                }
                else
                {
                    s_toggleModifier = KeyCode.None;
                }

                ViveInput.onUpdate -= UpdateInterfaceToggleKey;
                ViveInput.onUpdate += UpdateInterfaceToggleKey;
                ViveInput.Initialize();
            }
            else
            {
                s_toggleKey = KeyCode.None;
                s_toggleModifier = KeyCode.None;

                ViveInput.onUpdate -= UpdateInterfaceToggleKey;
            }
        }

        private static void UpdateInterfaceToggleKey()
        {
            if (Input.GetKeyDown(s_toggleKey) && (s_toggleModifier == KeyCode.None || Input.GetKey(s_toggleModifier)))
            {
                if (s_interfaceObj == null)
                {
                    if (string.IsNullOrEmpty(s_bindingConfig.interface_prefab))
                    {
                        s_bindingConfig.interface_prefab = DEFAULT_INTERFACE_PREFAB;
                    }

                    s_interfaceObj = Resources.Load<GameObject>(s_bindingConfig.interface_prefab);

                    if (s_interfaceObj == null)
                    {
                        Debug.LogWarning("Binding interface prefab \"" + s_bindingConfig.interface_prefab + "\" not found");
                        return;
                    }

                    s_interfaceObj = Instantiate(s_interfaceObj);
                }

                s_interfaceObj.SetActive(!s_interfaceObj.activeSelf);
            }
        }

        private void Awake()
        {
            AutoLoadConfig();
        }

        public static void LoadBindingConfigFromRoleMap(params Type[] roleTypeFilter)
        {
            var roleCount = ViveRoleEnum.ValidViveRoleTable.Count;
            var roleDataList = ListPool<RoleData>.Get();
            var filterUsed = roleTypeFilter != null && roleTypeFilter.Length > 0;

            if (filterUsed)
            {
                roleDataList.AddRange(s_bindingConfig.roles);
            }

            for (int i = 0; i < roleCount; ++i)
            {
                var roleType = ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i);
                var roleName = ViveRoleEnum.ValidViveRoleTable.GetKeyByIndex(i);
                var roleMap = ViveRole.GetMap(roleType);

                if (filterUsed)
                {
                    // apply filter
                    var filtered = false;
                    foreach (var t in roleTypeFilter) { if (roleType == t) { filtered = true; break; } }
                    if (!filtered) { continue; }
                }

                if (roleMap.BindingCount > 0)
                {
                    var bindingTable = roleMap.BindingTable;

                    var roleData = new RoleData()
                    {
                        type = roleName,
                        bindings = new Binding[bindingTable.Count],
                    };

                    for (int j = 0, jmax = bindingTable.Count; j < jmax; ++j)
                    {
                        var binding = new Binding();
                        binding.device_sn = bindingTable.GetKeyByIndex(j);
                        binding.device_model = VRModule.GetCurrentDeviceState(VRModule.GetConnectedDeviceIndex(binding.device_sn)).deviceModel; // save the device_model for better recognition of the device
                        binding.role_value = bindingTable.GetValueByIndex(j);
                        binding.role_name = roleMap.RoleValueInfo.GetNameByRoleValue(binding.role_value);

                        roleData.bindings[j] = binding;
                    }

                    if (filterUsed)
                    {
                        // merge with existing role data
                        var roleDataIndex = roleDataList.FindIndex((item) => item.type == roleName);
                        if (roleDataIndex >= 0)
                        {
                            roleDataList[roleDataIndex] = roleData;
                        }
                        else
                        {
                            roleDataList.Add(roleData);
                        }
                    }
                    else
                    {
                        roleDataList.Add(roleData);
                    }
                }
                else
                {
                    if (roleDataList.Count > 0)
                    {
                        // don't write to config if no bindings
                        roleDataList.RemoveAll((item) => item.type == roleName);
                    }
                }
            }

            s_bindingConfig.roles = roleDataList.ToArray();

            ListPool<RoleData>.Release(roleDataList);
        }

        public static int ApplyBindingConfigToRoleMap(params Type[] roleTypeFilter)
        {
            var appliedCount = 0;
            var filterUsed = roleTypeFilter != null && roleTypeFilter.Length > 0;

            foreach (var roleData in s_bindingConfig.roles)
            {
                Type roleType;
                if (string.IsNullOrEmpty(roleData.type) || !ViveRoleEnum.ValidViveRoleTable.TryGetValue(roleData.type, out roleType)) { continue; }

                if (filterUsed)
                {
                    // apply filter
                    var filtered = false;
                    foreach (var t in roleTypeFilter) { if (roleType == t) { filtered = true; break; } }
                    if (!filtered) { continue; }
                }

                var roleMap = ViveRole.GetMap(roleType);
                roleMap.UnbindAll();

                foreach (var binding in roleData.bindings)
                {
                    if (roleMap.IsDeviceBound(binding.device_sn)) { continue; } // skip if device already bound

                    // bind device according to role_name first
                    // if role_name is invalid then role_value is used
                    int roleValue;
                    if (string.IsNullOrEmpty(binding.role_name) || !roleMap.RoleValueInfo.TryGetRoleValueByName(binding.role_name, out roleValue))
                    {
                        roleValue = binding.role_value;
                    }

                    roleMap.BindDeviceToRoleValue(binding.device_sn, roleValue);
                    ++appliedCount;
                }
            }

            return appliedCount;
        }

        public static void SaveBindingConfigToFile(string configPath, bool prettyPrint = true)
        {
            using (var outputFile = new StreamWriter(configPath))
            {
                outputFile.Write(JsonUtility.ToJson(s_bindingConfig, prettyPrint));
            }
        }

        public static void LoadBindingConfigFromFile(string configPath)
        {
            using (var inputFile = new StreamReader(configPath))
            {
                s_bindingConfig = JsonUtility.FromJson<BindingConfig>(inputFile.ReadToEnd());

                UpdateInterfaceKeyMonitor();
            }
        }

        public static void BindAllCurrentDeviceClassMappings(VRModuleDeviceClass deviceClass)
        {
            for (int i = 0, imax = ViveRoleEnum.ValidViveRoleTable.Count; i < imax; ++i)
            {
                var roleMap = ViveRole.GetMap(ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i));
                var roleInfo = roleMap.RoleValueInfo;
                for (int rv = roleInfo.MinValidRoleValue, rvmax = roleInfo.MaxValidRoleValue; rv <= rvmax; ++rv)
                {
                    if (!roleInfo.IsValidRoleValue(rv)) { continue; }
                    if (roleMap.IsRoleValueBound(rv)) { continue; }

                    var mappedDevice = roleMap.GetMappedDeviceByRoleValue(rv);
                    var mappedDeviceState = VRModule.GetCurrentDeviceState(mappedDevice);
                    if (mappedDeviceState.deviceClass != deviceClass) { continue; }

                    roleMap.BindDeviceToRoleValue(mappedDeviceState.serialNumber, rv);
                }
            }
        }

        public static void BindAllCurrentMappings()
        {
            for (int i = 0, imax = ViveRoleEnum.ValidViveRoleTable.Count; i < imax; ++i)
            {
                var roleMap = ViveRole.GetMap(ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i));
                roleMap.BindAll();
            }
        }

        public static void UnbindAllCurrentBindings()
        {
            for (int i = 0, imax = ViveRoleEnum.ValidViveRoleTable.Count; i < imax; ++i)
            {
                var roleMap = ViveRole.GetMap(ViveRoleEnum.ValidViveRoleTable.GetValueByIndex(i));
                roleMap.UnbindAll();
            }
        }

        public static void SaveBindings(string configPath, bool prettyPrint = true)
        {
            LoadBindingConfigFromRoleMap();
            SaveBindingConfigToFile(configPath, prettyPrint);
        }

        public static void LoadBindings(string configPath)
        {
            LoadBindingConfigFromFile(configPath);
            ApplyBindingConfigToRoleMap();
        }

        [Obsolete("Use SaveBindings instead")]
        public static void SaveRoleBindings(string filePath, bool prettyPrint = false)
        {
            SaveBindings(filePath, prettyPrint);
        }

        [Obsolete("Use LoadBindings instead")]
        public static void LoadRoleBindings(string filePath)
        {
            LoadBindings(filePath);
        }
    }
}