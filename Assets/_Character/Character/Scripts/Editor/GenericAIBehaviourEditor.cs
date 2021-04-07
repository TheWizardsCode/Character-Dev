using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using WizardsCode.Stats;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(AbstractAIBehaviour), true)]
    public class GenericAIBehaviourEditor : Editor
    {
        Color darkSkinHeaderColor = (Color)new Color32(62, 62, 62, 255);
        Color lightSkinHeaderColor = (Color)new Color32(194, 194, 194, 255);

        /// <summary>
        /// This creates an overlapping label with the title bar for this
        /// component which allows us to overwrite the title. unfortunately
        /// it only works when the component is expanded.
        /// </summary>
        protected override void OnHeaderGUI()
        {
            var rect = EditorGUILayout.GetControlRect(false, 0f);
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y -= rect.height * 1.4f;
            rect.x = 60;
            rect.xMax -= rect.x * 2f;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? darkSkinHeaderColor : lightSkinHeaderColor);

            string header = (target as AbstractAIBehaviour).DisplayName + " (AI Behaviour)";
            if (string.IsNullOrEmpty(header))
                header = target.ToString();

            EditorGUI.LabelField(rect, header, EditorStyles.boldLabel);
        }
    }
}
