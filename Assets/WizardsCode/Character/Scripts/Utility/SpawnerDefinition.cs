using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Utility
{
    /// <summary>
    /// A SpawnerDefinition describes what can be spawned by a spawning event.
    /// It contains references to a number of SpawnPrefab configurations which
    /// define the rules by which individual prefabs may spawn. It provides 
    /// utility methods for selecting a prefab to spawn.
    /// </summary>
    [CreateAssetMenu(fileName = "New Spawner Definition", menuName = "Wizards Code/Spawner/New Spawner Definition")]
    public class SpawnerDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("An ordered list of spawn prefabs that might be spawned by this spawner definition.")]
        SpawnPrefab[] m_SpawnPrefabs;

        /// <summary>
        /// Instantiate and return all the prefabs that should be spawned at or around the supplied position.
        /// </summary>
        public GameObject[] InstantiatePrefabs(Vector3 position)
        {
            List<GameObject> spawned = new List<GameObject>();

            Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359.9f), 0));

            for (int prefabIdx = 0; prefabIdx < m_SpawnPrefabs.Length; prefabIdx++)
            {
                SpawnPrefab spawnedPrefab = m_SpawnPrefabs[prefabIdx];
                if (spawnedPrefab.probability >= Random.value)
                {
                    spawned.Add(Instantiate(spawnedPrefab.prefab, (Vector3)position, rotation));

                    if (spawnedPrefab.stopSpawning)
                    {
                        break;
                    }
                }
            }

            return spawned.ToArray();
        }

    }
}
