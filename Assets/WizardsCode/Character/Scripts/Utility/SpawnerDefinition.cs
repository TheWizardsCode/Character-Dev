using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

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
        SpawnObjectDefinition[] m_SpawnObjectDefinitions;


        /// <summary>
        /// Instantiate and return all the prefabs that should be spawned at or around the supplied position.
        /// </summary>
        public GameObject[] InstantiatePrefabs(Vector3 position)
        {
            List<GameObject> spawned = new List<GameObject>();

            Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359.9f), 0));

            for (int prefabIdx = 0; prefabIdx < m_SpawnObjectDefinitions.Length; prefabIdx++)
            {
                SpawnObjectDefinition spawnedPrefab = m_SpawnObjectDefinitions[prefabIdx];
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

    [Serializable]
    public struct SpawnObjectDefinition
    {
        [SerializeField, Tooltip("The prefab to spawn.")]
        public GameObject prefab;
        [SerializeField, Tooltip("The probability that this object will be spawned."), Range(0f, 1f)]
        public  float probability;
        [SerializeField, Tooltip("If true and this prefab is spawned then no other prefabs in the spawner will be processed for this iteration.")]
        public bool stopSpawning;
    }
}
