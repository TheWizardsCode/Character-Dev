using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(EmotionalState))]
    [CanEditMultipleObjects]
    public class EmotionalStateEditor : UnityEditor.Editor
    {
        EmotionalState state;
        SerializedProperty emotions;

        void OnEnable()
        {
            state = serializedObject.targetObject as EmotionalState;
            emotions = serializedObject.FindProperty("emotions");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Current enjoyment: (" + state.enjoymentValue + ") " + state.enjoymentDescriptor );
            EditorGUILayout.LabelField("Current activation: (" + state.activationValue + ") " + state.activationDescriptor);
            EditorGUILayout.PropertyField(emotions , true);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}