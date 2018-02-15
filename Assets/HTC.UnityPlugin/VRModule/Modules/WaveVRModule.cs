using UnityEngine;
#if VIU_WAVEVR && UNITY_ANDROID
using wvr;
#endif

namespace HTC.UnityPlugin.VRModuleManagement
{
    public class WaveVRModule : VRModule.ModuleBase
    {
#if VIU_WAVEVR && UNITY_ANDROID
        private const uint HMD_INDEX = 0;
        private const uint RIGHT_INDEX = 1;
        private const uint LEFT_INDEX = 2;
        private const uint DEVICE_COUNT = 3;
        private const uint INPUT_TYPE = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);

        private WVR_DeviceType[] m_deviceTypes = new WVR_DeviceType[]
        {
            WVR_DeviceType.WVR_DeviceType_HMD,
            WVR_DeviceType.WVR_DeviceType_Controller_Right,
            WVR_DeviceType.WVR_DeviceType_Controller_Left,
        };
        private WVR_DevicePosePair_t[] m_poses = new WVR_DevicePosePair_t[DEVICE_COUNT];  // HMD, R, L controllers.
        private WVR_AnalogState_t[] m_analogStates = new WVR_AnalogState_t[2];

        public override bool ShouldActiveModule()
        {
            return true;
        }

        public override void OnActivated()
        {
            var instance = Object.FindObjectOfType<WaveVR_Init>();
            if (instance == null)
            {
                VRModule.Instance.gameObject.AddComponent<WaveVR_Init>();
            }
        }

        public override void OnDeactivated() { }

        public override uint GetRightControllerDeviceIndex() { return RIGHT_INDEX; }

        public override uint GetLeftControllerDeviceIndex() { return LEFT_INDEX; }

        public override void UpdateDeviceState(IVRModuleDeviceState[] prevState, IVRModuleDeviceStateRW[] currState)
        {
            if (WaveVR.Instance == null) { return; }

            WVR_PoseOriginModel poseOrigin;
            switch (VRModule.trackingSpaceType)
            {
                case VRModuleTrackingSpaceType.RoomScale:
                    { poseOrigin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead; break; }
                case VRModuleTrackingSpaceType.Stationary:
                default:
                    { poseOrigin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround; break; }
            }

            Interop.WVR_GetSyncPose(poseOrigin, m_poses, DEVICE_COUNT);

            for (int i = 0; i < DEVICE_COUNT; ++i)
            {
                var deviceType = m_poses[i].type;
                var devicePose = m_poses[i].pose;

                if (deviceType == m_deviceTypes[i] && devicePose.IsValidPose)
                {
                    currState[i].isConnected = true;
                    if (deviceType == WVR_DeviceType.WVR_DeviceType_HMD)
                    {
                        currState[i].deviceClass = VRModuleDeviceClass.HMD;
                        currState[i].deviceModel = VRModuleDeviceModel.ViveFocusHMD;
                    }
                    else
                    {
                        currState[i].deviceClass = VRModuleDeviceClass.Controller;
                        currState[i].deviceModel = VRModuleDeviceModel.ViveFocusFinch;
                    }

                    var rigidTransform = new WaveVR_Utils.RigidTransform(devicePose.PoseMatrix);

                    currState[i].isPoseValid = true;
                    currState[i].isOutOfRange = false;
                    currState[i].isCalibrating = false;
                    currState[i].isUninitialized = false;

                    currState[i].velocity = new Vector3(devicePose.Velocity.v0, devicePose.Velocity.v1, -devicePose.Velocity.v2);
                    currState[i].angularVelocity = new Vector3(-devicePose.AngularVelocity.v0, -devicePose.AngularVelocity.v1, devicePose.AngularVelocity.v2);

                    currState[i].position = rigidTransform.pos;
                    currState[i].rotation = rigidTransform.rot;

                    uint buttons = 0;
                    uint touches = 0;

                    // FIXME: What does WVR_GetInputTypeCount mean?
                    //var analogCount = Interop.WVR_GetInputTypeCount(deviceType, WVR_InputType.WVR_InputType_Analog);
                    //if (m_analogStates == null || m_analogStates.Length < analogCount) { m_analogStates = new WVR_AnalogState_t[analogCount]; }

                    if (Interop.WVR_GetInputDeviceState(deviceType, INPUT_TYPE, ref buttons, ref touches, m_analogStates, m_analogStates.Length))
                    {
                        currState[i].SetButtonPress(VRModuleRawButton.System, (buttons & (1 << (int)WVR_InputId.WVR_InputId_Alias1_System)) != 0u);
                        currState[i].SetButtonPress(VRModuleRawButton.ApplicationMenu, (buttons & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Menu)) != 0u);
                        currState[i].SetButtonPress(VRModuleRawButton.Grip, (buttons & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Grip)) != 0u);
                        currState[i].SetButtonPress(VRModuleRawButton.Touchpad, (buttons & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Touchpad)) != 0u);
                        currState[i].SetButtonPress(VRModuleRawButton.Trigger, (buttons & ((1 << (int)WVR_InputId.WVR_InputId_Alias1_Bumper) | (1 << (int)WVR_InputId.WVR_InputId_Alias1_Trigger))) != 0u);

                        currState[i].SetButtonTouch(VRModuleRawButton.System, (touches & (1 << (int)WVR_InputId.WVR_InputId_Alias1_System)) != 0u);
                        currState[i].SetButtonTouch(VRModuleRawButton.ApplicationMenu, (touches & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Menu)) != 0u);
                        currState[i].SetButtonTouch(VRModuleRawButton.Grip, (touches & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Grip)) != 0u);
                        currState[i].SetButtonTouch(VRModuleRawButton.Touchpad, (touches & (1 << (int)WVR_InputId.WVR_InputId_Alias1_Touchpad)) != 0u);
                        currState[i].SetButtonTouch(VRModuleRawButton.Trigger, (buttons & ((1 << (int)WVR_InputId.WVR_InputId_Alias1_Bumper) | (1 << (int)WVR_InputId.WVR_InputId_Alias1_Trigger))) != 0u);

                        for (int j = 0, jmax = m_analogStates.Length; j < jmax; ++j)
                        {
                            switch (m_analogStates[j].id)
                            {
                                case WVR_InputId.WVR_InputId_Alias1_Trigger:
                                    if (m_analogStates[j].type == WVR_AnalogType.WVR_AnalogType_Trigger)
                                    {
                                        currState[i].SetAxisValue(VRModuleRawAxis.Trigger, m_analogStates[j].axis.x);
                                    }
                                    break;
                                case WVR_InputId.WVR_InputId_Alias1_Touchpad:
                                    if (m_analogStates[j].type == WVR_AnalogType.WVR_AnalogType_TouchPad && currState[i].GetButtonTouch(VRModuleRawButton.Touchpad))
                                    {
                                        currState[i].SetAxisValue(VRModuleRawAxis.TouchpadX, m_analogStates[j].axis.x);
                                        currState[i].SetAxisValue(VRModuleRawAxis.TouchpadY, m_analogStates[j].axis.y);
                                    }
                                    else
                                    {
                                        currState[i].SetAxisValue(VRModuleRawAxis.TouchpadX, 0f);
                                        currState[i].SetAxisValue(VRModuleRawAxis.TouchpadY, 0f);
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        currState[i].buttonPressed = 0u;
                        currState[i].buttonTouched = 0u;
                        for (int j = 0, jmax = currState[i].axisValue.Length; j < jmax; ++j) { currState[i].axisValue[j] = 0f; }
                    }
                }
                else
                {
                    if (prevState[i].isConnected)
                    {
                        currState[i].Reset();
                    }
                }
            }
        }

        public override void TriggerViveControllerHaptic(uint deviceIndex, ushort durationMicroSec = 500)
        {
            Interop.WVR_TriggerVibrator(m_deviceTypes[deviceIndex], WVR_InputId.WVR_InputId_Alias1_Touchpad, durationMicroSec);
        }
#endif
    }
}