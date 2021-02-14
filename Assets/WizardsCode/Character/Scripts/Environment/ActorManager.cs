using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Utility;

namespace WizardsCode.Character
{
    /// <summary>
    /// The ActorManager keeps tracks of all the actors in the world. It provides references to
    /// every single Actor and tracks aggregrate stats across them.
    /// </summary>
    public class ActorManager : AbstractSingleton<ActorManager>
    {
        List<Spawner> m_Spawners = new List<Spawner>();

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
        /// Get all items spawned into this world.
        /// </summary>
        public List<Transform> SpawnedItems
        {
            get
            {
                List<Transform> result = new List<Transform>();

                //TODO cache the spawned items for a time
                for (int i = 0; i < Spawners.Count; i++)
                {
                    result.AddRange(Spawners[i].Spawned);
                }

                return result;
            }
        }

    }
}
