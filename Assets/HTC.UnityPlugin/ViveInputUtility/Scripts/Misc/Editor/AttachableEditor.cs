//========= Copyright 2016-2017, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive.Misc
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Attachable))]
    public class AttachableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        protected virtual void OnSceneGUI()
        {
            var targetObj = (Attachable)target;
            var worldAnchorPos = targetObj.transform.TransformPoint(targetObj.anchorPos);
            var worldAnchorRot = targetObj.transform.rotation * Quaternion.Euler(targetObj.anchorEular);


            EditorGUI.BeginChangeCheck();
            var newWorldPos = Handles.PositionHandle(worldAnchorPos, worldAnchorRot);
            var newWorldRot = Handles.RotationHandle(worldAnchorRot, worldAnchorPos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObj, "Change Attachable Anchor");
                targetObj.anchorPos = targetObj.transform.InverseTransformPoint(newWorldPos);
                targetObj.anchorEular = (newWorldRot * Quaternion.Inverse(targetObj.transform.rotation)).eulerAngles;
            }
        }
    }
}