using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace WizardsCode.Utility
{
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute))]
    public class NavMeshAreaMaskPropertyDrawer : PropertyDrawer
    {
        private const int MaxNavMeshAreas = 32;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var areaNames = GameObjectUtility.GetNavMeshAreaNames();

            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect, GUIContent.none, property);

            Rect valueEditPosition = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
            property.intValue = EditorGUI.MaskField(valueEditPosition, GUIContent.none, property.intValue, areaNames);

            EditorGUI.EndProperty();
        }
    }
}
