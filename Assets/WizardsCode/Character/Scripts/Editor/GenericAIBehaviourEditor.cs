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
        List<bool> showRequiredStat = new List<bool>();
        float labelWidth = 200;

        public override void OnInspectorGUI()
        {
            OnHeaderGUI();

            AbstractAIBehaviour behaviour = (AbstractAIBehaviour)target;
            RequiredStat[] stats = behaviour.RequiredStats;

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Display Name");
            behaviour.DisplayName = EditorGUILayout.TextField(behaviour.DisplayName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Required Stats section
            EditorGUILayout.BeginVertical("Box");
            if (stats != null)
            {
                EditorGUILayout.LabelField("Required Stats (" + stats.Length + ")", EditorStyles.boldLabel);
            } else
            {
                EditorGUILayout.LabelField("No Required Stats");
            }
            
            for (int i = 0; i < stats.Length; i++)
            {
                if (i >= showRequiredStat.Count) showRequiredStat.Add(false);

                string label = stats[i].statTemplate.DisplayName;
                switch (stats[i].objective)
                {
                    case Objective.GreaterThan:
                        label += " > ";
                        break;
                    case Objective.Approximately:
                        label += " ~= ";
                        break;
                    case Objective.LessThan:
                        label += " < ";
                        break;
                }
                label += stats[i].Value;
                label += " (" + stats[i].NormalizedValue + ")";

                showRequiredStat[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showRequiredStat[i], label);

                if (showRequiredStat[i])
                {
                    GUILayout.BeginHorizontal();
                    stats[i].objective = (Objective)EditorGUILayout.EnumPopup("Objective", stats[i].objective);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Value");
                    stats[i].Value = EditorGUILayout.FloatField(stats[i].Value);
                    EditorGUILayout.PrefixLabel("Normalized");
                    stats[i].NormalizedValue = EditorGUILayout.FloatField(stats[i].NormalizedValue);
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            base.DrawDefaultInspector();
        }

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

            string header = (target as GenericAIBehaviour).DisplayName + " (AI Behaviour)";
            if (string.IsNullOrEmpty(header))
                header = target.ToString();

            EditorGUI.LabelField(rect, header, EditorStyles.boldLabel);
        }
    }
}
