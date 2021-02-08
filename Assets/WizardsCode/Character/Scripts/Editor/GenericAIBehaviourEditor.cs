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
        //TODO how do I change the title bar for this editor

        List<bool> showRequiredStat = new List<bool>();
        float labelWidth = 200;

        public override void OnInspectorGUI()
        {
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
            EditorGUILayout.LabelField("Required Stats (" + stats.Length + ")", EditorStyles.boldLabel);
            
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
    }
}
