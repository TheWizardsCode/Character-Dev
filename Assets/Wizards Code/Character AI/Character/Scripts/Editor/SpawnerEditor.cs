using UnityEditor;
using WizardsCode.BackgroundAI;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(Spawner), true)]
    public class SpawnerEditor : Editor
    {
        SerializedProperty navMeshMask;
        protected virtual void OnEnable()
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