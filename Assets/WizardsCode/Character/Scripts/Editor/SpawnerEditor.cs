using UnityEditor;
using WizardsCode.Utility;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(Spawner), true)]
    public class SpawnerEditor : Editor
    {
        SerializedProperty navMeshMask;
        void OnEnable()
        {
            navMeshMask = serializedObject.FindProperty("navMeshAreaMask");
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            serializedObject.Update();

            string[] areas = GameObjectUtility.GetNavMeshAreaNames();
            navMeshMask.intValue = EditorGUILayout.MaskField("NavMesh Area Mask", navMeshMask.intValue, areas);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
