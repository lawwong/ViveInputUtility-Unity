﻿//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;

namespace HTC.UnityPlugin.Vive
{
    public class VIUSettings : ScriptableObject
    {
        public const string SETTING_DATA_RESOURCE_PATH = "VIUSettings.asset";
        public const string BIND_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + B to open the binding interface in play mode.";
        public const string EX_CAM_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + M to toggle the quad view while external camera config file exist.";

        private static VIUSettings s_instance = null;

        private static VIUSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    if (Application.isPlaying)
                    {
                        Load();
                    }
#if UNITY_EDITOR
                    else
                    {
                        EditorLoad();
                    }
#endif
                }

                return s_instance;
            }
        }

        public static bool isLoaded { get { return s_instance != null; } }

        [SerializeField, Tooltip(BIND_UI_SWITCH_TOOLTIP)]
        private bool m_enableBindingInterfaceSwitch;
        [SerializeField, Tooltip(EX_CAM_UI_SWITCH_TOOLTIP)]
        private bool m_enableExternalCameraSwitch;
        [SerializeField]
        private bool m_simulatorSupport = false;
        [SerializeField]
        private bool m_unityNativeVRSupport = true;
        [SerializeField]
        private bool m_steamVRSupport = true;
        [SerializeField]
        private bool m_oculusVRSupport = true;

        public static bool enableBindingInterfaceSwitch { get { return Instance.m_enableBindingInterfaceSwitch; } set { Instance.m_enableBindingInterfaceSwitch = value; } }

        public static bool enableExternalCameraSwitch { get { return Instance.m_enableExternalCameraSwitch; } set { Instance.m_enableExternalCameraSwitch = value; } }

        public static bool simulatorSupport { get { return Instance.m_simulatorSupport; } set { Instance.m_simulatorSupport = value; } }

        public static bool unityNativeVRSupport { get { return Instance.m_unityNativeVRSupport; } set { Instance.m_unityNativeVRSupport = value; } }

        public static bool steamVRSupport { get { return Instance.m_steamVRSupport; } set { Instance.m_steamVRSupport = value; } }

        public static bool oculusVRSupport { get { return Instance.m_oculusVRSupport; } set { Instance.m_oculusVRSupport = value; } }

        public static void Load(string path = null)
        {
#if UNITY_EDITOR
            // load resource in playing mode only?
            if (!Application.isPlaying) { return; }
#endif
            if (string.IsNullOrEmpty(path) || (s_instance = Resources.Load<VIUSettings>(SETTING_DATA_RESOURCE_PATH)) == null)
            {
                s_instance = CreateInstance<VIUSettings>();
            }
        }

#if UNITY_EDITOR
        private static string s_assetPath;

        private static string assetPath
        {
            get
            {
                if (string.IsNullOrEmpty(s_assetPath))
                {
                    var ms = UnityEditor.MonoScript.FromScriptableObject(s_instance ?? CreateInstance<VIUSettings>());
                    var path = System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(ms));
                    s_assetPath = path.Substring(0, path.Length - "Scripts".Length) + "Resources/" + SETTING_DATA_RESOURCE_PATH;
                }

                return s_assetPath;
            }
        }

        public static void EditorLoad(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = assetPath;
            }

            if ((s_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<VIUSettings>(path)) == null)
            {
                s_instance = CreateInstance<VIUSettings>();
            }
        }

        public static void EditorSave(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = assetPath;
            }

            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<VIUSettings>(path);
            if (settings == null)
            {
                UnityEditor.AssetDatabase.CreateAsset(s_instance, path);
            }
            else
            {
                UnityEditor.EditorUtility.CopySerialized(settings, s_instance);
            }
        }

        public static void EditorRemove(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = assetPath;
            }

            UnityEditor.AssetDatabase.DeleteAsset(path);
        }

        [UnityEditor.PreferenceItem("VIU Settings")]
        private static void OnVIUPreferenceGUI()
        {
            if (GUILayout.Button("Reset to Default Settings"))
            {
                EditorRemove();
            }
            else
            {
                var changed = false;
                changed |= enableBindingInterfaceSwitch != (enableBindingInterfaceSwitch = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Enable Binding Interface Switch", BIND_UI_SWITCH_TOOLTIP), enableBindingInterfaceSwitch));
                changed |= enableExternalCameraSwitch != (enableExternalCameraSwitch = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Enable External Camera Switch", EX_CAM_UI_SWITCH_TOOLTIP), enableExternalCameraSwitch));
                changed |= simulatorSupport != (simulatorSupport = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Simulator Module Support"), simulatorSupport));
                changed |= unityNativeVRSupport != (unityNativeVRSupport = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Unity Native VR Module Support"), unityNativeVRSupport));
                changed |= steamVRSupport != (steamVRSupport = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Steam VR Module Support"), steamVRSupport));
                changed |= oculusVRSupport != (oculusVRSupport = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Oculus VR Module Support"), oculusVRSupport));

                if (changed)
                {
                    EditorSave();
                }
            }
        }
#endif
    }
}