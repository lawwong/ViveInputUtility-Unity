//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HTC.UnityPlugin.Vive
{
    [CustomEditor(typeof(ViveInputEventTrigger))]
    public class ViveInputEventTriggerEditor : Editor
    {
        private SerializedProperty m_activateProp;
        private SerializedProperty m_viveRoleProp;
        private SerializedProperty m_delegatesProp;

        private static GUIContent s_iconToolbarMinus;

        private static List<GUIContent> s_buttonNames;
        private static List<int> s_buttonValues;

        private static GUIContent s_eventName;

        private static GUIContent s_addPressContent = new GUIContent("Add Press Event");
        private static GUIContent s_addClickContent = new GUIContent("Add Click Event");
        private static GUIContent s_addDownContent = new GUIContent("Add Down Event");
        private static GUIContent s_addUpContent = new GUIContent("Add Up Event");

        static ViveInputEventTriggerEditor()
        {
            s_eventName = new GUIContent("");

            // Have to create a copy since otherwise the tooltip will be overwritten.
            s_iconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
            s_iconToolbarMinus.tooltip = "Remove all events in this list.";

            s_buttonNames = new List<GUIContent>();
            s_buttonValues = new List<int>();
            for (int i = EnumUtils.GetMinValue(typeof(ControllerButton)), imax = EnumUtils.GetMaxValue(typeof(ControllerButton)); i < imax; ++i)
            {
                if (!Enum.IsDefined(typeof(ControllerButton), i)) { continue; }
                s_buttonNames.Add(new GUIContent(((ControllerButton)i).ToString()));
                s_buttonValues.Add(i);
            }
        }

        protected virtual void OnEnable()
        {
            m_activateProp = serializedObject.FindProperty("m_active");
            m_viveRoleProp = serializedObject.FindProperty("m_viveRole");
            m_delegatesProp = serializedObject.FindProperty("m_delegates");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_activateProp);
            EditorGUILayout.PropertyField(m_viveRoleProp);

            EditorGUILayout.Space();

            var removeButtonSize = GUIStyle.none.CalcSize(s_iconToolbarMinus);

            for (int i = 0; i < m_delegatesProp.arraySize; ++i)
            {
                var delegateProp = m_delegatesProp.GetArrayElementAtIndex(i);

                var buttonProp = delegateProp.FindPropertyRelative("button");
                var eventTypeProp = delegateProp.FindPropertyRelative("eventType");
                var callbackProp = delegateProp.FindPropertyRelative("callback");

                s_eventName.text = "On " + buttonProp.enumDisplayNames[buttonProp.enumValueIndex] + " " + eventTypeProp.enumDisplayNames[eventTypeProp.enumValueIndex];

                EditorGUILayout.PropertyField(callbackProp, s_eventName);
                var callbackRect = GUILayoutUtility.GetLastRect();

                var removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, s_iconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry(toBeRemovedEntry);
            }

            var buttonWidth = (EditorGUIUtility.currentViewWidth / 2) - 12f;

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(s_addPressContent, GUILayout.Width(buttonWidth))) { ShowAddTriggermenu((int)ButtonEventType.Press); }
                if (GUILayout.Button(s_addDownContent, GUILayout.Width(buttonWidth))) { ShowAddTriggermenu((int)ButtonEventType.Down); }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(s_addClickContent, GUILayout.Width(buttonWidth))) { ShowAddTriggermenu((int)ButtonEventType.Click); }
                if (GUILayout.Button(s_addUpContent, GUILayout.Width(buttonWidth))) { ShowAddTriggermenu((int)ButtonEventType.Up); }
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry)
        {
            m_delegatesProp.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        private struct ManuArgs
        {
            public int buttonIndex;
            public int eventTypeValue;
        }

        private void ShowAddTriggermenu(int eventTypeValue)
        {
            // Now create the menu, add items and show it
            var menu = new GenericMenu();

            for (int i = 0; i < s_buttonNames.Count; ++i)
            {
                var active = true;

                if (s_buttonValues[i] == (int)ControllerButton.None)
                {
                    active = false;
                }
                else
                {
                    // Check if we already have a Entry for the current eventType, if so, disable it
                    for (int p = 0; p < m_delegatesProp.arraySize; ++p)
                    {
                        var delegateProp = m_delegatesProp.GetArrayElementAtIndex(p);

                        var buttonProp = delegateProp.FindPropertyRelative("button");
                        var eventTypeProp = delegateProp.FindPropertyRelative("eventType");

                        if (buttonProp.enumValueIndex == i && eventTypeProp.intValue == eventTypeValue)
                        {
                            active = false;
                            break;
                        }
                    }
                }

                if (active)
                {
                    menu.AddItem(s_buttonNames[i], false, OnAddNewSelected, new ManuArgs() { buttonIndex = i, eventTypeValue = eventTypeValue });
                }
                else
                {
                    menu.AddDisabledItem(s_buttonNames[i]);
                }
            }

            menu.ShowAsContext();

            Event.current.Use();
        }

        private void OnAddNewSelected(object args)
        {
            var menuArgs = (ManuArgs)args;

            m_delegatesProp.arraySize += 1;
            var delegateProp = m_delegatesProp.GetArrayElementAtIndex(m_delegatesProp.arraySize - 1);

            var buttonProp = delegateProp.FindPropertyRelative("button");
            var eventTypeProp = delegateProp.FindPropertyRelative("eventType");

            buttonProp.enumValueIndex = menuArgs.buttonIndex;
            eventTypeProp.intValue = menuArgs.eventTypeValue;

            serializedObject.ApplyModifiedProperties();
        }
    }
}