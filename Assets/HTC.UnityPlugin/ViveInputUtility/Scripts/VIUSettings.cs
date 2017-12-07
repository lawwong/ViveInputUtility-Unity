//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;

namespace HTC.UnityPlugin.Vive
{
    public class VIUSettings : ScriptableObject
    {
        public const string SETTING_DATA_RESOURCE_PATH = "VIUSettings";
        public const string BIND_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + B to open the binding interface in play mode.";
        public const string EX_CAM_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + M to toggle the quad view while external camera config file exist.";

        public const bool ENABLE_BINDING_INTERFACE_SWITCH_DEFAULT_VALUE = false;
        public const bool ENABLE_EXTERNAL_CAMERA_SWITCH_DEFAULT_VALUE = false;
        public const bool SIMULATOR_SUPPORT_DEFAULT_VALUE = false;
        public const bool UNITYN_ATIVEVR_SUPPORT_DEFAULT_VALUE = true;
        public const bool STEAMVR_SUPPORT_DEFAULT_VALUE = true;
        public const bool OCULUS_VRSUPPORT_DEFAULT_VALUE = true;

        private static VIUSettings s_instance = null;

        private static VIUSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
#if UNITY_EDITOR
                    EditorLoad();
#else
                    Load();
#endif
                }

                return s_instance;
            }
        }

        [SerializeField, Tooltip(BIND_UI_SWITCH_TOOLTIP)]
        private bool m_enableBindingInterfaceSwitch = ENABLE_BINDING_INTERFACE_SWITCH_DEFAULT_VALUE;
        [SerializeField, Tooltip(EX_CAM_UI_SWITCH_TOOLTIP)]
        private bool m_enableExternalCameraSwitch = ENABLE_EXTERNAL_CAMERA_SWITCH_DEFAULT_VALUE;
        [SerializeField]
        private bool m_simulatorSupport = SIMULATOR_SUPPORT_DEFAULT_VALUE;
        [SerializeField]
        private bool m_unityNativeVRSupport = UNITYN_ATIVEVR_SUPPORT_DEFAULT_VALUE;
        [SerializeField]
        private bool m_steamVRSupport = STEAMVR_SUPPORT_DEFAULT_VALUE;
        [SerializeField]
        private bool m_oculusVRSupport = OCULUS_VRSUPPORT_DEFAULT_VALUE;

        private string m_loadedAssetPath;

        public static bool isLoaded { get { return s_instance != null; } }

        public static string loadedAssetPath { get { return s_instance == null ? null : s_instance.m_loadedAssetPath; } }

        public static bool enableBindingInterfaceSwitch { get { return Instance == null ? ENABLE_BINDING_INTERFACE_SWITCH_DEFAULT_VALUE : s_instance.m_enableBindingInterfaceSwitch; } set { if (Instance != null) { Instance.m_enableBindingInterfaceSwitch = value; } } }

        public static bool enableExternalCameraSwitch { get { return Instance == null ? ENABLE_EXTERNAL_CAMERA_SWITCH_DEFAULT_VALUE : s_instance.m_enableExternalCameraSwitch; } set { if (Instance != null) { Instance.m_enableExternalCameraSwitch = value; } } }

        public static bool simulatorSupport { get { return Instance == null ? SIMULATOR_SUPPORT_DEFAULT_VALUE : s_instance.m_simulatorSupport; } set { if (Instance != null) { Instance.m_simulatorSupport = value; } } }

        public static bool unityNativeVRSupport { get { return Instance == null ? UNITYN_ATIVEVR_SUPPORT_DEFAULT_VALUE : s_instance.m_unityNativeVRSupport; } set { if (Instance != null) { Instance.m_unityNativeVRSupport = value; } } }

        public static bool steamVRSupport { get { return Instance == null ? STEAMVR_SUPPORT_DEFAULT_VALUE : s_instance.m_steamVRSupport; } set { if (Instance != null) { Instance.m_steamVRSupport = value; } } }

        public static bool oculusVRSupport { get { return Instance == null ? OCULUS_VRSUPPORT_DEFAULT_VALUE : s_instance.m_oculusVRSupport; } set { if (Instance != null) { Instance.m_oculusVRSupport = value; } } }

        public static void Load(string resourcePath = null)
        {
#if UNITY_EDITOR
            // load resource in playing mode only?
            if (!Application.isPlaying) { return; }
#endif
            if (string.IsNullOrEmpty(resourcePath))
            {
                resourcePath = SETTING_DATA_RESOURCE_PATH;
            }

            if ((s_instance = Resources.Load<VIUSettings>(resourcePath)) == null)
            {
                s_instance = CreateInstance<VIUSettings>();
            }
            else
            {
                s_instance.m_loadedAssetPath = resourcePath;
            }
        }



        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
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
                    var obj = CreateInstance<VIUSettings>();
                    var ms = UnityEditor.MonoScript.FromScriptableObject(obj);
                    var path = System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(ms));
                    s_assetPath = path.Substring(0, path.Length - "Scripts".Length) + "Resources/" + SETTING_DATA_RESOURCE_PATH + ".asset";
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
            else
            {
                s_instance.m_loadedAssetPath = path;
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

        public static void EditorRemoveLoadedAsset()
        {
            if (isLoaded && !string.IsNullOrEmpty(loadedAssetPath))
            {
                UnityEditor.AssetDatabase.DeleteAsset(loadedAssetPath);
            }
        }

        [UnityEditor.PreferenceItem("VIU Settings")]
        private static void OnVIUPreferenceGUI()
        {
            if (GUILayout.Button("Reset to Default Settings"))
            {
                EditorRemoveLoadedAsset();
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