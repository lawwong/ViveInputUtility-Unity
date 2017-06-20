//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HTC.UnityPlugin.Vive
{
    public class ViveInputEventTrigger : MonoBehaviour, IViveRoleComponent
    {
        [Serializable]
        public class Entry
        {
            public ControllerButton button;
            public ButtonEventType eventType;
            public TriggerEvent callback = new TriggerEvent();
        }

        [Serializable]
        public struct EventArgs
        {
            public ControllerButton button;
            public ButtonEventType eventType;
            public ViveInputEventTrigger senderObj;
        }

        [Serializable]
        public class TriggerEvent : UnityEvent<EventArgs> { }

        [SerializeField]
        private bool m_active = true;
        [SerializeField]
        private ViveRoleProperty m_viveRole = ViveRoleProperty.New(HandRole.RightHand);
        [SerializeField]
        private List<Entry> m_delegates;

        private bool m_activated = false;
        private Type m_listeningRoleType;
        private int m_listeningRoleValue;

        public bool active
        {
            get
            {
                return m_active;
            }
            set
            {
                m_active = value;
                if (!m_activated && m_active)
                {
                    AddAllInputListeners();
                }
                else if (m_activated && !m_active)
                {
                    RemoveAllInputListeners();
                }
                m_activated = value;
            }
        }

        public ViveRoleProperty viveRole { get { return m_viveRole; } }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (m_activated != m_active)
                {
                    active = m_active;
                }
                else
                {
                    RemoveAllInputListeners();
                    AddAllInputListeners();
                }
            }
        }
#endif

        private void Awake()
        {
            m_viveRole.Changed += OnRoleChanged;

            active = m_active;
        }

        private void OnRoleChanged()
        {
            if (m_activated)
            {
                RemoveAllInputListeners();
                AddAllInputListeners();
            }
        }

        private void AddAllInputListeners()
        {
            m_listeningRoleType = m_viveRole.roleType;
            m_listeningRoleValue = m_viveRole.roleValue;

            if (m_listeningRoleType == null) { return; }

            for (int i = 0, imax = m_delegates.Count; i < imax; ++i)
            {
                if (m_delegates[i] == null) { continue; }
                if (m_delegates[i].callback.GetPersistentEventCount() == 0) { continue; }

                ViveInput.AddListenerEx(m_listeningRoleType, m_listeningRoleValue, m_delegates[i].button, m_delegates[i].eventType, OnInputEvent);
            }
        }

        private void RemoveAllInputListeners()
        {
            if (m_listeningRoleType == null) { return; }

            for (int i = 0, imax = m_delegates.Count; i < imax; ++i)
            {
                if (m_delegates[i] == null) { continue; }
                if (m_delegates[i].callback.GetPersistentEventCount() == 0) { continue; }

                ViveInput.RemoveListenerEx(m_listeningRoleType, m_listeningRoleValue, m_delegates[i].button, m_delegates[i].eventType, OnInputEvent);
            }

            m_listeningRoleType = null;
        }

        private bool TryGetTriggerEvent(ControllerButton button, ButtonEventType eventType, out TriggerEvent triggerEvent)
        {
            triggerEvent = null;

            if (m_delegates == null) { return false; }

            for (int i = 0, imax = m_delegates.Count; i < imax; ++i)
            {
                if (m_delegates[i].button == button && m_delegates[i].eventType == eventType)
                {
                    triggerEvent = m_delegates[i].callback;
                    return true;
                }
            }

            return false;
        }

        private TriggerEvent GetTriggerEvent(ControllerButton button, ButtonEventType eventType)
        {
            TriggerEvent triggerEvent;

            if (!TryGetTriggerEvent(button, eventType, out triggerEvent))
            {
                if (m_delegates == null) { m_delegates = new List<Entry>(); }
                m_delegates.Add(new Entry()
                {
                    button = button,
                    eventType = eventType,
                    callback = new TriggerEvent(),
                });
            }

            return triggerEvent;
        }

        public void AddListener(ControllerButton button, ButtonEventType eventType, UnityAction<EventArgs> callback)
        {
            if (button == ControllerButton.None || callback == null) { return; }

            var triggerEvent = GetTriggerEvent(button, eventType);
            var triggerEventCount = triggerEvent.GetPersistentEventCount();

            triggerEvent.AddListener(callback);

            if (triggerEvent.GetPersistentEventCount() > triggerEventCount)
            {
                ViveInput.AddListenerEx(m_viveRole.roleType, m_viveRole.roleValue, button, eventType, OnInputEvent);
            }
        }

        public void RemoveListener(ControllerButton button, ButtonEventType eventType, UnityAction<EventArgs> callback)
        {
            TriggerEvent triggerEvent;
            if (callback == null || button == ControllerButton.None || !TryGetTriggerEvent(button, eventType, out triggerEvent)) { return; }

            triggerEvent.RemoveListener(callback);

            if (triggerEvent.GetPersistentEventCount() == 0)
            {
                ViveInput.RemoveListenerEx(m_viveRole.roleType, m_viveRole.roleValue, button, eventType, OnInputEvent);
            }
        }

        public void RemoveAllListener(ControllerButton button, ButtonEventType eventType)
        {
            TriggerEvent triggerEvent;
            if (button == ControllerButton.None || !TryGetTriggerEvent(button, eventType, out triggerEvent)) { return; }

            var triggerEventCount = triggerEvent.GetPersistentEventCount();

            if (triggerEventCount > 0)
            {
                triggerEvent.RemoveAllListeners();

                ViveInput.RemoveListenerEx(m_viveRole.roleType, m_viveRole.roleValue, button, eventType, OnInputEvent);
            }
        }

        private void OnInputEvent(Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
        {
            TriggerEvent triggerEvent;
            if (button != ControllerButton.None && TryGetTriggerEvent(button, eventType, out triggerEvent))
            {
                triggerEvent.Invoke(new EventArgs()
                {
                    senderObj = this,
                    button = button,
                    eventType = eventType,
                });
            }
        }
    }
}