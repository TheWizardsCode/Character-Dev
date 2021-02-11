using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(Wander), true)]
    public class WanderEditor : Editor
    {
        int mask;
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            Wander controller = (Wander)target;

            string[] areas = GameObjectUtility.GetNavMeshAreaNames();
            controller.navMeshAreaMask = EditorGUILayout.MaskField("NavMesh Area Mask", controller.navMeshAreaMask, areas);
        }
    }
}
