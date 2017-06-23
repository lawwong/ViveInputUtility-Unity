//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HTC.UnityPlugin.Vive
{
    public class ViveInputVirtualButton : MonoBehaviour
    {
        public enum InputsOperatorEnum
        {
            Or,
            And,
        }

        [Serializable]
        public class InputEntry
        {
            public ViveRoleProperty viveRole;
            public ControllerButton button;
        }

        [Serializable]
        public struct OutputEventArgs
        {
            public ViveInputVirtualButton senderObj;
            public ButtonEventType eventType;
        }

        [Serializable]
        public class OutputEvent : UnityEvent<OutputEventArgs> { }

        [SerializeField]
        private bool m_active = true;
        [SerializeField]
        private InputsOperatorEnum m_inputsOperator = InputsOperatorEnum.Or;
        [SerializeField]
        private List<InputEntry> m_inputs = new List<InputEntry>();
        [SerializeField]
        private OutputEvent m_onPress = new OutputEvent();
        [SerializeField]
        private OutputEvent m_onClick = new OutputEvent();
        [SerializeField]
        private OutputEvent m_onPressDown = new OutputEvent();
        [SerializeField]
        private OutputEvent m_onPressUp = new OutputEvent();
        [SerializeField]
        private List<GameObject> m_toggleGameObjectOnClick = new List<GameObject>();
        [SerializeField]
        private List<Behaviour> m_toggleComponentOnClick = new List<Behaviour>();

        private int m_updatedFrameCount;
        private bool m_updateActivated = false;
        private bool m_prevState = false;
        private bool m_currState = false;
        private float m_lastPressDownTime = 0f;
        private int m_clickCount = 0;

        public bool active
        {
            get
            {
                return m_active;
            }
            set
            {
                m_active = value;
                TryListenUpdateEvent();
            }
        }

        public InputsOperatorEnum logicGate { get { return m_inputsOperator; } }
        public List<InputEntry> inputs { get { return m_inputs; } }

        public OutputEvent onPress { get { return m_onPress; } }
        public OutputEvent onClick { get { return m_onClick; } }
        public OutputEvent onPressDown { get { return m_onPressDown; } }
        public OutputEvent onPressUp { get { return m_onPressUp; } }

        private bool isPress { get { return m_currState; } }
        private bool isDown { get { return !m_prevState && m_currState; } }
        private bool isUp { get { return m_prevState && !m_currState; } }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryListenUpdateEvent();
        }

        private void Reset()
        {
            m_inputs.Add(new InputEntry()
            {
                viveRole = ViveRoleProperty.New(HandRole.RightHand),
                button = ControllerButton.Trigger,
            });
            m_toggleGameObjectOnClick.Add(null);
            m_toggleComponentOnClick.Add(null);
        }
#endif

        private void Awake()
        {
            TryListenUpdateEvent();
        }

        private void TryListenUpdateEvent()
        {
            if (Application.isPlaying && m_active && !m_updateActivated)
            {
                // register update event
                ViveInput.onUpdate.AddListener(OnNewInput);
                m_updateActivated = true;
            }
        }

        private void UpdateState()
        {
            if (!ChangeProp.Set(ref m_updatedFrameCount, Time.frameCount)) { return; }

            m_prevState = m_currState;
            m_currState = false;

            if (m_inputs.Count == 0) { return; }

            switch (m_inputsOperator)
            {
                case InputsOperatorEnum.Or:

                    m_currState = false;

                    for (int i = 0, imax = m_inputs.Count; i < imax; ++i)
                    {
                        if (ViveInput.GetPressEx(m_inputs[i].viveRole.roleType, m_inputs[i].viveRole.roleValue, m_inputs[i].button))
                        {
                            m_currState = true;
                            break;
                        }
                    }

                    break;
                case InputsOperatorEnum.And:

                    m_currState = true;

                    for (int i = 0, imax = m_inputs.Count; i < imax; ++i)
                    {
                        if (!ViveInput.GetPressEx(m_inputs[i].viveRole.roleType, m_inputs[i].viveRole.roleValue, m_inputs[i].button))
                        {
                            m_currState = false;
                            break;
                        }
                    }

                    break;
            }
        }

        private void OnNewInput()
        {
            var timeNow = Time.time;

            if (m_active)
            {
                UpdateState();

                if (isPress)
                {
                    if (isDown)
                    {
                        // record click count
                        if (timeNow - m_lastPressDownTime < ViveInput.clickInterval)
                        {
                            ++m_clickCount;
                        }
                        else
                        {
                            m_clickCount = 1;
                        }

                        // record press down time
                        m_lastPressDownTime = timeNow;

                        // PressDown event
                        if (m_onPressDown != null)
                        {
                            m_onPressDown.Invoke(new OutputEventArgs()
                            {
                                senderObj = this,
                                eventType = ButtonEventType.Down,
                            });
                        }
                    }

                    // Press event
                    if (m_onPress != null)
                    {
                        m_onPress.Invoke(new OutputEventArgs()
                        {
                            senderObj = this,
                            eventType = ButtonEventType.Press,
                        });
                    }
                }
                else if (isUp)
                {
                    // PressUp event
                    if (m_onPressUp != null)
                    {
                        m_onPressUp.Invoke(new OutputEventArgs()
                        {
                            senderObj = this,
                            eventType = ButtonEventType.Up,
                        });
                    }

                    if (timeNow - m_lastPressDownTime < ViveInput.clickInterval)
                    {
                        for (int i = m_toggleGameObjectOnClick.Count - 1; i >= 0; --i)
                        {
                            if (m_toggleGameObjectOnClick[i] != null) { m_toggleGameObjectOnClick[i].SetActive(!m_toggleGameObjectOnClick[i].activeSelf); }
                        }

                        for (int i = m_toggleComponentOnClick.Count - 1; i >= 0; --i)
                        {
                            if (m_toggleComponentOnClick[i] != null) { m_toggleComponentOnClick[i].enabled = !m_toggleComponentOnClick[i].enabled; }
                        }

                        // Click event
                        if (m_onClick != null)
                        {
                            m_onClick.Invoke(new OutputEventArgs()
                            {
                                senderObj = this,
                                eventType = ButtonEventType.Click,
                            });
                        }
                    }
                }
            }
            else
            {
                // unregister update event
                ViveInput.onUpdate.RemoveListener(OnNewInput);
                m_updateActivated = false;

                // clean up
                m_prevState = m_currState;
                m_currState = false;

                if (isUp)
                {
                    // PressUp event
                    if (m_onPressUp != null)
                    {
                        m_onPressUp.Invoke(new OutputEventArgs()
                        {
                            senderObj = this,
                            eventType = ButtonEventType.Up,
                        });
                    }

                    if (timeNow - m_lastPressDownTime < ViveInput.clickInterval)
                    {
                        for (int i = m_toggleGameObjectOnClick.Count - 1; i >= 0; --i)
                        {
                            if (m_toggleGameObjectOnClick[i] != null) { m_toggleGameObjectOnClick[i].SetActive(!m_toggleGameObjectOnClick[i].activeSelf); }
                        }

                        for (int i = m_toggleComponentOnClick.Count - 1; i >= 0; --i)
                        {
                            if (m_toggleComponentOnClick[i] != null) { m_toggleComponentOnClick[i].enabled = !m_toggleComponentOnClick[i].enabled; }
                        }

                        // Click event
                        if (m_onClick != null)
                        {
                            m_onClick.Invoke(new OutputEventArgs()
                            {
                                senderObj = this,
                                eventType = ButtonEventType.Click,
                            });
                        }
                    }
                }

                m_prevState = false;
            }
        }

        public bool GetPress()
        {
            UpdateState();
            return isPress;
        }

        public bool GetPressDown()
        {
            UpdateState();
            return isDown;
        }

        public bool GetPressUp()
        {
            UpdateState();
            return isUp;
        }

        public int GetClickCount()
        {
            UpdateState();
            return m_clickCount;
        }

        public float GetLastPressDownTime()
        {
            UpdateState();
            return m_lastPressDownTime;
        }
    }
}