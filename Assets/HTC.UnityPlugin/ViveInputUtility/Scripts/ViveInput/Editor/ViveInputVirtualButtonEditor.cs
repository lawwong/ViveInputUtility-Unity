//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HTC.UnityPlugin.Vive
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ViveInputVirtualButton))]
    public class ViveInputVirtualButtonEditor : Editor
    {
        private SerializedProperty m_activateProp;
        private SerializedProperty m_logicGateProp;
        private SerializedProperty m_inputsProp;
        private SerializedProperty m_outputPressProp;
        private SerializedProperty m_outputDownProp;
        private SerializedProperty m_outputUpProp;
        private SerializedProperty m_outputClickProp;
        private SerializedProperty m_toggleGameObjProp;
        private SerializedProperty m_toggleCompProp;

        protected virtual void OnEnable()
        {
            m_activateProp = serializedObject.FindProperty("m_active");
            m_logicGateProp = serializedObject.FindProperty("m_logicGate");
            m_inputsProp = serializedObject.FindProperty("m_inputs");
            m_outputPressProp = serializedObject.FindProperty("m_onOutputPress");
            m_outputDownProp = serializedObject.FindProperty("m_onOutputClick");
            m_outputUpProp = serializedObject.FindProperty("m_onOutputPressDown");
            m_outputClickProp = serializedObject.FindProperty("m_onOutputPressUp");
            m_toggleGameObjProp = serializedObject.FindProperty("m_toggleGameObjectOnOutputClick");
            m_toggleCompProp = serializedObject.FindProperty("m_toggleComponentOnOutputClick");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_activateProp);

            if (m_inputsProp.arraySize > 1)
            {
                EditorGUILayout.PropertyField(m_logicGateProp);
            }

            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 1f;
            for (int i = 0, imax = m_inputsProp.arraySize; i < imax; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                
                var inputProp = m_inputsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(inputProp.FindPropertyRelative("viveRole"));
                EditorGUILayout.PropertyField(inputProp.FindPropertyRelative("button"));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUIUtility.labelWidth = prevLabelWidth;

            //DrawDefaultInspector();
            //


            // inputs



            serializedObject.ApplyModifiedProperties();
        }
    }
}