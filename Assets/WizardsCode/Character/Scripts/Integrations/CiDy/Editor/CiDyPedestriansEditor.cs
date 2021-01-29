#if CiDy
using CiDy;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Character;

namespace WizardsCode.CiDYExtension
{
    [CustomEditor(typeof(CiDyPedestrians), true)]
    public class CiDyPedestriansEditor : SpawnerEditor
    {
        SerializedProperty serializedGraph;

        protected override void OnEnable()
        {
            base.OnEnable();
            serializedGraph = serializedObject.FindProperty("cidyGraph");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            CiDyGraph graph = (CiDyGraph)serializedGraph.objectReferenceValue;
            if (graph != null)
            {
                if (GUILayout.Button("Setup NavMesh Areas"))
                {
                    // Setup Roads
                    List<GameObject> roads = graph.roads;
                    for (int i = 0; i < roads.Count; i++)
                    {
                        SetNavMeshArea(roads[i], "Road");

                        // Setup crossings
                        List<Transform> decals = roads[i].GetComponent<CiDyRoad>().decals;
                        for (int idx = 0; idx < decals.Count; idx++) {
                            SetNavMeshArea(decals[idx].gameObject, "Pedestrian Crossing");
                        }
                    }

                    // Setup Intersections
                    List<CiDyEdge> edges = graph.graphEdges;
                    for (int i = 0; i < edges.Count ; i++)
                    {
                        SetNavMeshArea(edges[i].v1.intersection, "Road");
                        SetNavMeshArea(edges[i].v2.intersection, "Road");
                    }

                    // Setup Sidewalks
                    List<CiDyCell> cells = graph.cells;
                    for (int i = 0; i < cells.Count; i++)
                    {
                        //TODO is there a more robust way of getting the sidewalks?
                        Transform sidewalk = cells[i].transform.Find("SideWalk");
                        if (sidewalk != null)
                        {
                            SetNavMeshArea(sidewalk.gameObject, "Pavement");
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Set the navmesh area on all MeshRenderer children of an object.
        /// Also set isStatic to true.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nameOfNavMeshArea"></param>
        private static void SetNavMeshArea(GameObject obj, string nameOfNavMeshArea)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
            for (int idx = 0; idx < renderers.Length; idx++)
            {
                GameObjectUtility.SetNavMeshArea(renderers[idx].gameObject, NavMesh.GetAreaFromName(nameOfNavMeshArea));
                renderers[idx].gameObject.isStatic = true;
            }
        }
    }
}
#endif