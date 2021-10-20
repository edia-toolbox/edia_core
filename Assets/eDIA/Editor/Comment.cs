using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace eDIA
{
    [CustomEditor(typeof(Comment), true)]
    [CanEditMultipleObjects]
    public class CommentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            string comment = serializedObject.FindProperty("comment").stringValue;
            EditorGUILayout.HelpBox(comment, MessageType.Info);
        }
    }
}