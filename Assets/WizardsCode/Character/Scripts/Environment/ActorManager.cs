using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Utility;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// The ActorManager keeps tracks of all the actors in the world. It provides references to
    /// every single Actor and tracks aggregrate stats across them.
    /// </summary>
    public class ActorManager : AbstractSingleton<ActorManager>
    {
        List<Spawner> m_Spawners = new List<Spawner>();

        List<Brain> m_CachedBrains = new List<Brain>();
        private float m_NextSpawnedItemsCacheUpdateTime;
        private List<Brain> m_SpawnedBrainsCache = new List<Brain>();

        public void RegisterSpawner(Spawner spawner)
        {
            if (m_Spawners.Contains(spawner))
            {
                Debug.LogWarning("Attempted to register a spawner that is already registered, ignoring: " + spawner);
            } else
            {
                m_Spawners.Add(spawner);
            }
        }

        /// <summary>
        /// Get all the spawners in this world.
        /// </summary>
        public List<Spawner> Spawners
        {
            get { return m_Spawners; }
        }

        /// <summary>
        /// Get all brains spawned into this world.
        /// </summary>
        public List<Brain> SpawnedBrains
        {
            get
            {
                if (Time.realtimeSinceStartup < m_NextSpawnedItemsCacheUpdateTime) return m_SpawnedBrainsCache;

                m_NextSpawnedItemsCacheUpdateTime = Time.realtimeSinceStartup + 5;
                m_SpawnedBrainsCache.Clear();

                Brain brain;
                for (int i = 0; i < Spawners.Count; i++)
                {
                    for (int idx = 0; idx < Spawners[i].Spawned.Count; idx ++)
                    {
                        brain = Spawners[i].Spawned[idx].GetComponent<Brain>();
                        if (brain != null)
                        {
                            m_SpawnedBrainsCache.Add(brain);
                        }
                    }
                }

                return m_SpawnedBrainsCache;
            }
        }

        /// <summary>
        /// Get a dictionary of behaviour names (key) and count of brains carrying out
        /// that behaviour (value).
        /// </summary>
        public Dictionary<string, int> ActiveBehaviours
        {
            get
            {
                //TODO Cache behaviour counts
                Dictionary<string, int> result = new Dictionary<string, int>();
                string key;

                for (int i = 0; i < SpawnedBrains.Count; i++)
                {
                    key = SpawnedBrains[i].CurrentBehaviour.DisplayName;
                    if (result.ContainsKey(key))
                    {
                        result[key] = result[key] + 1;
                    } else {
                        result.Add(key, 1);
                    }
                }

                return result;
            }
        }

    }
}
