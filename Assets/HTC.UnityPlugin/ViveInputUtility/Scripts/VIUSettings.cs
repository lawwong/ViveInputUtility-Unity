//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using System;
using UnityEngine;

namespace HTC.UnityPlugin.Vive
{
    public class VIUSettings : ScriptableObject
    {
        public const string SETTING_DATA_RESOURCE_PATH = "VIUSettings";

        public const string BIND_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + B to open the binding interface in play mode.";
        public const string EX_CAM_UI_SWITCH_TOOLTIP = "When enabled, pressing RightShift + M to toggle the quad view while external camera config file exist.";
        public const string SIMULATOR_PAD_TOUCH_SWITCH_TOOLTIP = "Use Shift key to lock rotation and simulate pad touch";
        public const string SIMULATOR_KEY_MOVE_SPEED_TOOLTIP = "W/A/S/D";
        public const string SIMULATOR_KEY_ROTATE_SPEED_TOOLTIP = "Arrow Up/Down/Left/Right";

        public const bool ENABLE_BINDING_INTERFACE_SWITCH_DEFAULT_VALUE = false;
        public const bool ENABLE_EXTERNAL_CAMERA_SWITCH_DEFAULT_VALUE = false;
        public const bool SIMULATOR_SUPPORT_DEFAULT_VALUE = false;
        public const bool UNITYN_ATIVEVR_SUPPORT_DEFAULT_VALUE = true;
        public const bool STEAMVR_SUPPORT_DEFAULT_VALUE = true;
        public const bool OCULUS_VRSUPPORT_DEFAULT_VALUE = true;
        public const bool SIMULATOR_AUTO_TRACK_MAIN_CAMERA = true;
        public const bool SIMULATOR_PAD_TOUCH_SWITCH = true;
        public const float SIMULATOR_KEY_MOVE_SPEED = 1.5f;
        public const float SIMULATOR_MOUSE_ROTATE_SPEED = 90f;
        public const float SIMULATOR_KEY_ROTATE_SPEED = 90f;

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
        [SerializeField]
        private bool m_simulatorAutoTrackMainCamera = SIMULATOR_AUTO_TRACK_MAIN_CAMERA;
        [SerializeField, Tooltip(SIMULATOR_PAD_TOUCH_SWITCH_TOOLTIP)]
        private bool m_simulatorPadTouchSwitch = SIMULATOR_PAD_TOUCH_SWITCH;
        [SerializeField, Tooltip(SIMULATOR_KEY_MOVE_SPEED_TOOLTIP)]
        private float m_simulatorKeyMoveSpeed = SIMULATOR_KEY_MOVE_SPEED;
        [SerializeField]
        private float m_simulatorMouseRotateSpeed = SIMULATOR_MOUSE_ROTATE_SPEED;
        [SerializeField, Tooltip(SIMULATOR_KEY_MOVE_SPEED_TOOLTIP)]
        private float m_simulatorKeyRotateSpeed = SIMULATOR_KEY_ROTATE_SPEED;

        public static bool enableBindingInterfaceSwitch { get { return Instance == null ? ENABLE_BINDING_INTERFACE_SWITCH_DEFAULT_VALUE : s_instance.m_enableBindingInterfaceSwitch; } set { if (Instance != null) { Instance.m_enableBindingInterfaceSwitch = value; } } }

        public static bool enableExternalCameraSwitch { get { return Instance == null ? ENABLE_EXTERNAL_CAMERA_SWITCH_DEFAULT_VALUE : s_instance.m_enableExternalCameraSwitch; } set { if (Instance != null) { Instance.m_enableExternalCameraSwitch = value; } } }

        public static bool simulatorSupport { get { return Instance == null ? SIMULATOR_SUPPORT_DEFAULT_VALUE : s_instance.m_simulatorSupport; } set { if (Instance != null) { Instance.m_simulatorSupport = value; } } }

        public static bool unityNativeVRSupport { get { return Instance == null ? UNITYN_ATIVEVR_SUPPORT_DEFAULT_VALUE : s_instance.m_unityNativeVRSupport; } set { if (Instance != null) { Instance.m_unityNativeVRSupport = value; } } }

        public static bool steamVRSupport { get { return Instance == null ? STEAMVR_SUPPORT_DEFAULT_VALUE : s_instance.m_steamVRSupport; } set { if (Instance != null) { Instance.m_steamVRSupport = value; } } }

        public static bool oculusVRSupport { get { return Instance == null ? OCULUS_VRSUPPORT_DEFAULT_VALUE : s_instance.m_oculusVRSupport; } set { if (Instance != null) { Instance.m_oculusVRSupport = value; } } }

        public static bool simulatorAutoTrackMainCamera { get { return Instance == null ? SIMULATOR_AUTO_TRACK_MAIN_CAMERA : s_instance.m_simulatorAutoTrackMainCamera; } set { if (Instance != null) { Instance.m_simulatorAutoTrackMainCamera = value; } } }

        public static bool simulatorPadTouchSwitch { get { return Instance == null ? SIMULATOR_PAD_TOUCH_SWITCH : s_instance.m_simulatorPadTouchSwitch; } set { if (Instance != null) { Instance.m_simulatorPadTouchSwitch = value; } } }

        public static float simulatorKeyMoveSpeed { get { return Instance == null ? SIMULATOR_KEY_MOVE_SPEED : s_instance.m_simulatorKeyMoveSpeed; } set { if (Instance != null) { Instance.m_simulatorKeyMoveSpeed = value; } } }

        public static float simulatorMouseRotateSpeed { get { return Instance == null ? SIMULATOR_MOUSE_ROTATE_SPEED : s_instance.m_simulatorMouseRotateSpeed; } set { if (Instance != null) { Instance.m_simulatorMouseRotateSpeed = value; } } }

        public static float simulatorKeyRotateSpeed { get { return Instance == null ? SIMULATOR_KEY_ROTATE_SPEED : s_instance.m_simulatorKeyRotateSpeed; } set { if (Instance != null) { Instance.m_simulatorKeyRotateSpeed = value; } } }


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


        private static string s_loadedAssetPath;

        public static bool isLoaded { get { return s_instance != null; } }

        public static string loadedAssetPath { get { return s_loadedAssetPath; } }

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
                s_loadedAssetPath = resourcePath;
            }
        }



        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
                s_loadedAssetPath = null;
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
                s_loadedAssetPath = path;
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
                s_loadedAssetPath = path;
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

                s_instance = null;
                s_loadedAssetPath = null;
            }
        }

        private static GUIStyle s_enabledLabel;
        private static GUIStyle s_disabledLabel;
        private const float INDENT_PIXEL = 20f;
        [UnityEditor.PreferenceItem("VIU Settings")]
        private static void OnVIUPreferenceGUI()
        {
            if (s_enabledLabel == null)
            {
                s_enabledLabel = new GUIStyle();
                s_enabledLabel.fontStyle = FontStyle.Bold;
                s_disabledLabel = new GUIStyle();
                s_disabledLabel.fontStyle = FontStyle.Bold;
                s_disabledLabel.normal.textColor = Color.gray;
            }

            UnityEditor.EditorGUI.BeginChangeCheck();

            enableBindingInterfaceSwitch = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Enable Binding Interface Switch", BIND_UI_SWITCH_TOOLTIP), enableBindingInterfaceSwitch);
            enableExternalCameraSwitch = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Enable External Camera Switch", EX_CAM_UI_SWITCH_TOOLTIP), enableExternalCameraSwitch);

            // simulator
            simulatorSupport = UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Simulator Module Support"), simulatorSupport, s_enabledLabel);
            if (simulatorSupport)
            {
                UnityEditor.EditorGUI.indentLevel++;

                simulatorAutoTrackMainCamera = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Auto Main Camera Tracking"), simulatorAutoTrackMainCamera);
                simulatorPadTouchSwitch = UnityEditor.EditorGUILayout.Toggle(new GUIContent("Simulate Trackpad Touch", SIMULATOR_PAD_TOUCH_SWITCH_TOOLTIP), simulatorPadTouchSwitch);
                simulatorKeyMoveSpeed = UnityEditor.EditorGUILayout.DelayedFloatField(new GUIContent("Keyboard Move Speed", SIMULATOR_KEY_MOVE_SPEED_TOOLTIP), simulatorKeyMoveSpeed);
                simulatorKeyRotateSpeed = UnityEditor.EditorGUILayout.DelayedFloatField(new GUIContent("Keyboard Rotate Speed", SIMULATOR_KEY_ROTATE_SPEED_TOOLTIP), simulatorKeyRotateSpeed);
                simulatorMouseRotateSpeed = UnityEditor.EditorGUILayout.DelayedFloatField(new GUIContent("Mouse Rotate Speed"), simulatorMouseRotateSpeed);

                UnityEditor.EditorGUI.indentLevel--;
            }

            // native
#if UNITY_5_5_OR_NEWER
                unityNativeVRSupport = UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Unity Native VR Module Support"), unityNativeVRSupport, s_enabledLabel);
                if (unityNativeVRSupport)
                {
                    UnityEditor.EditorGUI.indentLevel++;
#if UNITY_5_6_OR_NEWER
                    UnityEditor.EditorGUILayout.HelpBox("Vive Tracker device not supprot, use Steam VR Module instead.", UnityEditor.MessageType.Warning);
#else
                    UnityEditor.EditorGUILayout.HelpBox("Vive Tracker input event not supprot, use Steam VR Module instead.", UnityEditor.MessageType.Warning);
#endif
                    UnityEditor.EditorGUI.indentLevel--;
                }
#else
            GUI.enabled = false;
            UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Unity Native VR Module Support"), false, s_enabledLabel);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(INDENT_PIXEL);
            if (GUILayout.Button("Get Unity 5.5 or later version"))
            {
                Application.OpenURL("https://unity3d.com/get-unity/download");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
#endif

            // openvr
#if VIU_STEAMVR
            steamVRSupport = UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Steam VR Module Support"), steamVRSupport, s_enabledLabel);
#else
            GUI.enabled = false;
            UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Steam VR Module Support"), false, s_enabledLabel);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(INDENT_PIXEL);
            if (GUILayout.Button("Get SteamVR Plugin"))
            {
                Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/32647");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
#endif

            // oculuas
#if VIU_OCULUSVR
            oculusVRSupport = UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Oculus VR Module Support"), oculusVRSupport, s_enabledLabel);
#else
            GUI.enabled = false;
            UnityEditor.EditorGUILayout.ToggleLeft(new GUIContent("Oculus VR Module Support"), false, s_enabledLabel);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(INDENT_PIXEL);
            if (GUILayout.Button("Get Oculus VR Plugin"))
            {
                Application.OpenURL("https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
#endif

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                EditorSave();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset"))
            {
                EditorRemoveLoadedAsset();
            }
            GUILayout.EndHorizontal();
        }
#endif
    }
}