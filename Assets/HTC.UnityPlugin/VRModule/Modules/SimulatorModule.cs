//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using UnityEngine;
using System;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
using XRDevice = UnityEngine.VR.VRDevice;
#endif

namespace HTC.UnityPlugin.VRModuleManagement
{
    public interface ISimulatorVRModule
    {
        bool autoMainCameraTrackingEnabled { get; set; }
        event Action onActivated;
        event Action onDeactivated;
        event Action<IVRModuleDeviceState[], IVRModuleDeviceStateRW[]> onUpdateDeviceState;
    }

    public class SimulatorVRModule : VRModule.ModuleBase, ISimulatorVRModule
    {
        private const uint RIGHT_INDEX = 1;
        private const uint LEFT_INDEX = 2;

        private bool m_prevXREnabled;
        private bool m_autoCamTracking = true;
        private bool m_resetDevices;
        private float m_moveSpeed = 1.5f; // meter/second
        private float m_rotateSpeed = 45f; // angle/unit
        private float m_rotateKeySpeed = 60f; // angle/second

        public event Action onActivated;
        public event Action onDeactivated;
        public event Action<IVRModuleDeviceState[], IVRModuleDeviceStateRW[]> onUpdateDeviceState;

        public bool autoMainCameraTrackingEnabled
        {
            get { return m_autoCamTracking; }
            set { m_autoCamTracking = value; }
        }

        public override bool ShouldActiveModule() { return VIUSettings.simulatorSupport; }

        public override void OnActivated()
        {
            if (m_prevXREnabled = XRSettings.enabled)
            {
                XRSettings.enabled = false;
            }

            m_resetDevices = true;

            if (onActivated != null)
            {
                onActivated();
            }
        }

        public override void OnDeactivated()
        {
            XRSettings.enabled = m_prevXREnabled;

            if (onDeactivated != null)
            {
                onDeactivated();
            }
        }

        public override uint GetRightControllerDeviceIndex() { return RIGHT_INDEX; }

        public override uint GetLeftControllerDeviceIndex() { return LEFT_INDEX; }

        public override void Update()
        {
            UpdateAlphaKeyDown();
        }

        public override void UpdateDeviceState(IVRModuleDeviceState[] prevState, IVRModuleDeviceStateRW[] currState)
        {
            // Reset to default state
            if (m_resetDevices)
            {
                m_resetDevices = false;

                foreach (var state in currState)
                {
                    switch (state.deviceIndex)
                    {
                        case VRModule.HMD_DEVICE_INDEX:
                        case RIGHT_INDEX:
                        case LEFT_INDEX:
                            InitializeDevice(state);
                            break;
                        default:
                            if (state.isConnected)
                            {
                                state.Reset();
                            }
                            break;
                    }
                }

                SelectDevice(currState[VRModule.HMD_DEVICE_INDEX]);
            }

            // keyboard/mouse control

            //// handle number key down
            var keySelectDevice = default(IVRModuleDeviceStateRW);
            if (GetDeviceByInputDownKeyCode(currState, out keySelectDevice))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (keySelectDevice.isConnected && keySelectDevice.deviceIndex != VRModule.HMD_DEVICE_INDEX)
                    {
                        keySelectDevice.Reset();
                    }
                }
                else
                {
                    if (!IsSelectedDevice(keySelectDevice))
                    {
                        // select
                        if (!keySelectDevice.isConnected)
                        {
                            InitializeDevice(keySelectDevice);
                        }

                        SelectDevice(keySelectDevice);
                    }
                    else
                    {
                        // deselect
                        DeselectDevice();
                    }
                }
            }

            if (VRModule.IsValidDeviceIndex(m_selectedDeviceIndex))
            {
                ControlDevice(currState[m_selectedDeviceIndex]);
            }

            if (onUpdateDeviceState != null)
            {
                onUpdateDeviceState(prevState, currState);
            }

            if (autoMainCameraTrackingEnabled && Camera.main != null)
            {
                RigidPose.SetPose(Camera.main.transform, currState[VRModule.HMD_DEVICE_INDEX].pose);
            }
        }

        private uint m_selectedDeviceIndex;
        private float m_selectedDeviceRotZ;
        private bool IsSelectedDevice(IVRModuleDeviceStateRW state)
        {
            return m_selectedDeviceIndex == state.deviceIndex;
        }

        private void SelectDevice(IVRModuleDeviceStateRW state)
        {
            m_selectedDeviceIndex = state.deviceIndex;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            m_selectedDeviceRotZ = state.pose.rot.z;
        }

        private void DeselectDevice()
        {
            m_selectedDeviceIndex = VRModule.INVALID_DEVICE_INDEX;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Input.GetKeyDown in UpdateDeviceState is not working
        private bool[] m_alphaKeyDownState = new bool[10];
        private void UpdateAlphaKeyDown()
        {
            m_alphaKeyDownState[0] = Input.GetKeyDown(KeyCode.Alpha0);
            m_alphaKeyDownState[1] = Input.GetKeyDown(KeyCode.Alpha1);
            m_alphaKeyDownState[2] = Input.GetKeyDown(KeyCode.Alpha2);
            m_alphaKeyDownState[3] = Input.GetKeyDown(KeyCode.Alpha3);
            m_alphaKeyDownState[4] = Input.GetKeyDown(KeyCode.Alpha4);
            m_alphaKeyDownState[5] = Input.GetKeyDown(KeyCode.Alpha5);
            m_alphaKeyDownState[6] = Input.GetKeyDown(KeyCode.Alpha6);
            m_alphaKeyDownState[7] = Input.GetKeyDown(KeyCode.Alpha7);
            m_alphaKeyDownState[8] = Input.GetKeyDown(KeyCode.Alpha8);
            m_alphaKeyDownState[9] = Input.GetKeyDown(KeyCode.Alpha9);
        }

        private bool GetAlphaKeyDown(int num)
        {
            return m_alphaKeyDownState[num];
        }

        private bool GetDeviceByInputDownKeyCode(IVRModuleDeviceStateRW[] deviceStates, out IVRModuleDeviceStateRW deviceState)
        {
            if (GetAlphaKeyDown(1)) { deviceState = deviceStates[1]; return true; }
            if (GetAlphaKeyDown(2)) { deviceState = deviceStates[2]; return true; }
            if (GetAlphaKeyDown(3)) { deviceState = deviceStates[3]; return true; }
            if (GetAlphaKeyDown(4)) { deviceState = deviceStates[4]; return true; }
            if (GetAlphaKeyDown(5)) { deviceState = deviceStates[5]; return true; }
            if (GetAlphaKeyDown(6)) { deviceState = deviceStates[6]; return true; }
            if (GetAlphaKeyDown(7)) { deviceState = deviceStates[7]; return true; }
            if (GetAlphaKeyDown(8)) { deviceState = deviceStates[8]; return true; }
            if (GetAlphaKeyDown(9)) { deviceState = deviceStates[9]; return true; }
            if (GetAlphaKeyDown(0)) { deviceState = deviceStates[0]; return true; }

            deviceState = null;
            return false;
        }

        private void InitializeDevice(IVRModuleDeviceStateRW deviceState)
        {
            switch (deviceState.deviceIndex)
            {
                case VRModule.HMD_DEVICE_INDEX:
                    {
                        deviceState.isConnected = true;
                        deviceState.deviceClass = VRModuleDeviceClass.HMD;
                        deviceState.serialNumber = "VIU Simulator HMD Device";
                        deviceState.modelNumber = deviceState.serialNumber;
                        deviceState.renderModelName = deviceState.serialNumber;
                        deviceState.deviceModel = VRModuleDeviceModel.ViveHMD;

                        deviceState.isPoseValid = true;
                        deviceState.position = new Vector3(0f, 1.75f, 0f);
                        deviceState.rotation = Quaternion.identity;

                        break;
                    }
                case RIGHT_INDEX:
                    {
                        deviceState.isConnected = true;
                        deviceState.deviceClass = VRModuleDeviceClass.Controller;
                        deviceState.serialNumber = "VIU Simulator Controller Device " + RIGHT_INDEX;
                        deviceState.modelNumber = deviceState.serialNumber;
                        deviceState.renderModelName = deviceState.serialNumber;
                        deviceState.deviceModel = VRModuleDeviceModel.ViveController;

                        deviceState.isPoseValid = true;
                        deviceState.position = new Vector3(0.3f, 1.05f, 0.4f);
                        deviceState.rotation = Quaternion.identity;
                        deviceState.buttonPressed = 0ul;
                        deviceState.buttonTouched = 0ul;
                        deviceState.ResetAxisValues();
                        break;
                    }
                case LEFT_INDEX:
                    {
                        deviceState.isConnected = true;
                        deviceState.deviceClass = VRModuleDeviceClass.Controller;
                        deviceState.serialNumber = "VIU Simulator Controller Device " + LEFT_INDEX;
                        deviceState.modelNumber = deviceState.serialNumber;
                        deviceState.renderModelName = deviceState.serialNumber;
                        deviceState.deviceModel = VRModuleDeviceModel.ViveController;

                        deviceState.isPoseValid = true;
                        deviceState.position = new Vector3(-0.3f, 1.05f, 0.4f);
                        deviceState.rotation = Quaternion.identity;
                        deviceState.buttonPressed = 0ul;
                        deviceState.buttonTouched = 0ul;
                        deviceState.ResetAxisValues();
                        break;
                    }
                default:
                    {
                        deviceState.isConnected = true;
                        deviceState.deviceClass = VRModuleDeviceClass.GenericTracker;
                        deviceState.serialNumber = "VIU Simulator Generic Tracker Device " + deviceState.deviceIndex;
                        deviceState.modelNumber = deviceState.serialNumber;
                        deviceState.renderModelName = deviceState.serialNumber;
                        deviceState.deviceModel = VRModuleDeviceModel.ViveTracker;

                        deviceState.isPoseValid = true;
                        deviceState.position = new Vector3(0f, 1.05f, 0.4f);
                        deviceState.rotation = Quaternion.identity;
                        deviceState.buttonPressed = 0ul;
                        deviceState.buttonTouched = 0ul;
                        deviceState.ResetAxisValues();
                        break;
                    }
            }
        }

        private void ControlDevice(IVRModuleDeviceStateRW deviceState)
        {
            var deltaDis = Time.unscaledDeltaTime * m_moveSpeed;
            var deltaPos = Vector3.zero;
            if (Input.GetKey(KeyCode.D)) { deltaPos.x += deltaDis; }
            if (Input.GetKey(KeyCode.A)) { deltaPos.x -= deltaDis; }
            if (Input.GetKey(KeyCode.E)) { deltaPos.y += deltaDis; }
            if (Input.GetKey(KeyCode.Q)) { deltaPos.y -= deltaDis; }
            if (Input.GetKey(KeyCode.W)) { deltaPos.z += deltaDis; }
            if (Input.GetKey(KeyCode.S)) { deltaPos.z -= deltaDis; }

            var deltaEular = Vector3.zero;

            var deltaAngle = Time.unscaledDeltaTime * m_rotateSpeed;
            deltaEular.x = -Input.GetAxisRaw("Mouse Y") * deltaAngle;
            deltaEular.y = Input.GetAxisRaw("Mouse X") * deltaAngle;

            var deltaKeyAngle = Time.unscaledDeltaTime * m_rotateKeySpeed;
            if (Input.GetKey(KeyCode.C)) { deltaEular.z += deltaKeyAngle; }
            if (Input.GetKey(KeyCode.Z)) { deltaEular.z -= deltaKeyAngle; }

            var destEular = deviceState.rotation.eulerAngles + deltaEular;

            if (Input.GetKey(KeyCode.X)) { destEular.z = 0f; }

            deviceState.rotation = Quaternion.Euler(destEular);
            deviceState.pose *= new RigidPose(deltaPos, Quaternion.identity);
        }

    }
}