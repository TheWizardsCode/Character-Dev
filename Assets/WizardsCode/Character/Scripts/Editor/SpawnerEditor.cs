using UnityEditor;
using WizardsCode.Utility;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(Spawner), true)]
    public class SpawnerEditor : Editor
    {
        int mask;
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            Spawner spawner = (Spawner)target;

            string[] areas = GameObjectUtility.GetNavMeshAreaNames();
            spawner.navMeshAreaMask = EditorGUILayout.MaskField("NavMesh Area Mask", spawner.navMeshAreaMask, areas);
        }
    }
}
