using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Utility
{
    /// <summary>
    /// A representation of an object that can be spawned by the spawner.
    /// </summary>
    [CreateAssetMenu(fileName = "Spawned Prefab Config", menuName = "Wizards Code/Spawned Prefab", order = 1)]
    public class SpawnedPrefab : ScriptableObject
    {
        [SerializeField, Tooltip("The prefab to spawn.")]
        internal GameObject prefab;
        [SerializeField, Tooltip("The probability that this object will be spawned."), Range(0f, 1f)]
        internal float probability = 1f;
        [SerializeField, Tooltip("If true and this prefab is spawned then no other prefabs in the spawner will be processed for this iteration.")]
        internal bool stopSpawning = true;

        public void Reset()
        {
        }
    }
}
