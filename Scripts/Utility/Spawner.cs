using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.Utility
{
    /// <summary>
    /// A really simple spawner that will create a number of a given prefab within a defined area.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to spawn.")]
        SpawnedPrefab[] m_SpawnedPrefabs;
        [SerializeField, Tooltip("The number of the prefab to create.")]
        int m_Number = 10;
        [SerializeField, Tooltip("The radius within which to spawn")]
        float m_Radius = 10;
        [SerializeField, Tooltip("Should the character only be placed on a NavMesh?")]
        bool onNavMesh = false;
        [HideInInspector, SerializeField, Tooltip("The area mask that indicates NavMesh areas that the spawner can spawn characters into.")]
        public int navMeshAreaMask = NavMesh.AllAreas;

        private void Start()
        {
            for (int i = 0; i < m_Number; i++)
            {
                Vector3? position = GetPosition();
                if (position != null)
                {
                    Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359.9f), 0));

                    for (int prefabIdx = 0; prefabIdx < m_SpawnedPrefabs.Length; prefabIdx++)
                    {
                        SpawnedPrefab spawnedPrefab = m_SpawnedPrefabs[prefabIdx];
                        if (spawnedPrefab.probability >= Random.value)
                        {
                            GameObject go = Instantiate(spawnedPrefab.prefab, (Vector3)position, rotation);
                            go.name += " " + i;

                            if (spawnedPrefab.stopSpawning)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to find a suitable spawn position for a spawned prefab.
        /// A limited number of attempts will be tried before aborting. This prevents
        /// endless loops. 
        /// </summary>
        /// <param name="attemptNumber">The current attempt number</param>
        /// <param name="maxTries">The maximum number of attempts to find a spawn location before aborting.</param>
        /// <returns></returns>
        private Vector3? GetPosition(int attemptNumber = 0, int maxTries = 10)
        {
            attemptNumber++;
            if (attemptNumber > maxTries)
            {
                Debug.LogWarning("Unable to find a suitable location on the navmesh for " + gameObject.name + ". Check you have baked the NavMesh and that you have at least one allowed area within the spawn radius.");
                return null;
            }

            Vector2 pos2D = Random.insideUnitCircle * m_Radius;
            Vector3 position = transform.position + new Vector3(pos2D.x, 0, pos2D.y);
            Vector3 finalPos = position;
            if (Terrain.activeTerrain != null) {
                finalPos.y = Terrain.activeTerrain.SampleHeight(finalPos) + Terrain.activeTerrain.transform.position.y;
            }

            if (onNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(finalPos, out hit, 2, navMeshAreaMask))
                {
                    finalPos = hit.position;
                    return finalPos;
                } else
                {
                    return GetPosition(attemptNumber);
                }
            } else
            {
                return finalPos;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, m_Radius);
        }

    }
}
