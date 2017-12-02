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
        private uint m_controllingDevice;

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

        public override void UpdateDeviceState(IVRModuleDeviceState[] prevState, IVRModuleDeviceStateRW[] currState)
        {
            // Reset to default state
            if (m_resetDevices)
            {
                m_resetDevices = false;
                m_controllingDevice = VRModule.HMD_DEVICE_INDEX;

                foreach (var state in currState)
                {
                    switch (state.deviceIndex)
                    {
                        case VRModule.HMD_DEVICE_INDEX:
                            {
                                state.isConnected = true;
                                state.deviceClass = VRModuleDeviceClass.HMD;
                                state.serialNumber = "VIU Simulator HMD Device";
                                state.modelNumber = "VIU Simulator HMD Device";
                                state.renderModelName = "VIU Simulator HMD Device";
                                state.deviceModel = VRModuleDeviceModel.ViveHMD;

                                state.isPoseValid = true;
                                state.position = new Vector3(0f, 1.75f, 0f);
                                state.rotation = Quaternion.identity;
                                break;
                            }
                        case RIGHT_INDEX:
                            {
                                state.isConnected = true;
                                state.deviceClass = VRModuleDeviceClass.Controller;
                                state.serialNumber = "VIU Simulator Controller Device 1";
                                state.modelNumber = "VIU Simulator Controller Device 1";
                                state.renderModelName = "VIU Simulator Controller Device 1";
                                state.deviceModel = VRModuleDeviceModel.ViveController;

                                state.isPoseValid = true;
                                state.position = new Vector3(0.3f, 1.05f, 0.4f);
                                state.rotation = Quaternion.identity;
                                state.buttonPressed = 0ul;
                                state.buttonTouched = 0ul;
                                state.ResetAxisValues();
                                break;
                            }
                        case LEFT_INDEX:
                            {
                                state.isConnected = true;
                                state.deviceClass = VRModuleDeviceClass.Controller;
                                state.serialNumber = "VIU Simulator Controller Device 2";
                                state.modelNumber = "VIU Simulator Controller Device 2";
                                state.renderModelName = "VIU Simulator Controller Device 2";
                                state.deviceModel = VRModuleDeviceModel.ViveController;

                                state.isPoseValid = true;
                                state.position = new Vector3(-0.3f, 1.05f, 0.4f);
                                state.rotation = Quaternion.identity;
                                state.buttonPressed = 0ul;
                                state.buttonTouched = 0ul;
                                state.ResetAxisValues();
                                break;
                            }
                        default:
                            state.Reset();
                            break;
                    }
                }
            }

            // keyboard/mouse control

            if (onUpdateDeviceState != null)
            {
                onUpdateDeviceState(prevState, currState);
            }

            if (autoMainCameraTrackingEnabled && Camera.main != null)
            {
                RigidPose.SetPose(Camera.main.transform, currState[VRModule.HMD_DEVICE_INDEX].pose);
            }
        }
    }
}