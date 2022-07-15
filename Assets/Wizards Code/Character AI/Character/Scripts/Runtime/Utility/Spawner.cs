using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using WizardsCode.Character;
using WizardsCode.Character.WorldState;

namespace WizardsCode.BackgroundAI
{
    /// <summary>
    /// A really simple spawner that will create a number of a given prefab within a defined area.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [SerializeField, Tooltip("A name that will be appended to each instance. This will also have a number appended to make each unique.")]
        string m_SpawnerName = "Spawned";
        [SerializeField, Tooltip("The rules this spawner should obey when spawning.")]
        SpawnerDefinition m_SpawnDefinition;

        [Header("Spawn Restrictors")]
        [SerializeField, Tooltip("The tag that is used to mark players.")]
        string m_PlayerTag = "Player";
        [SerializeField, Tooltip("The minimum distance the nearest player can be for this spawn to spawn a new object. If there are no players in the scene this is ignored.")]
        float m_MinSpawnDistance = 100;

        [Header("Spawn Definition")]
        [SerializeField, Tooltip("The number of items to spawn on start." +
            " If this is set to 0 then no items will spawn until the duration of the spawn frequency has passed.")]
        int m_SpawnsOnStart = 0;
        [SerializeField, Tooltip("The maximum number of these items to be spawned in world at any one time. " +
            " That is, if there are this many already in the world no new instances will be spawned." +
            " Note that depending on the Spawn Definition each spawn may be more than one prefab.")]
        protected int m_NumberOfSpawns = 5;
        [SerializeField, Tooltip("The radius within which to spawn")]
        float m_Radius = 10;
        [SerializeField, Tooltip("The frequency at which new instances will be spawned if there are fewer than the maximum allowed number.")]
        protected float m_SpawnFrequency = 5;
        [SerializeField, Tooltip("Should the character only be placed on a NavMesh?")]
        bool onNavMesh = false;
        [HideInInspector, SerializeField, Tooltip("The area mask that indicates NavMesh areas that the spawner can spawn characters into.")]
        public int navMeshAreaMask = NavMesh.AllAreas;

        [Header("Spawn Events")]
        [SerializeField, Tooltip("An event that is fired whenever an object is spawned.")]
        public UnityEvent<GameObject> OnSpawn;
        
        protected List<Transform> m_Spawned = new List<Transform>();
        protected int m_TotalSpawnedCount = 0;
        protected float m_TimeOfNextSpawn = 0;
        protected float m_MinDistanceSqr;
        protected Transform m_Player;

        /// <summary>
        /// Get all the objects spawned by this spawner.
        /// </summary>
        public List<Transform> Spawned
        {
            get { return m_Spawned; }
        }

        protected Transform Player
        {
            get
            {
                if (m_Player == null)
                {
                    GameObject go = GameObject.FindGameObjectWithTag(m_PlayerTag);
                    if (go != null) {
                        m_Player = go.transform;
                    }
                }
                return m_Player;
            }
        }

        protected virtual void Start()
        {
            ActorManager.Instance.RegisterSpawner(this);

            for (int i = m_SpawnsOnStart; i > 0; i--)
            {
                m_TotalSpawnedCount++;
                Spawn(m_TotalSpawnedCount.ToString());
            }

            m_TimeOfNextSpawn = m_SpawnFrequency;

            m_MinDistanceSqr = m_MinSpawnDistance * m_MinSpawnDistance;
        }

        protected virtual void Update()
        {
            if (!ShouldSpawn())
            {
                return;
            }

            Spawn(m_TotalSpawnedCount.ToString());
            m_TotalSpawnedCount++;
            m_TimeOfNextSpawn = Time.time + m_SpawnFrequency;
        }

        protected virtual bool ShouldSpawn()
        {
            if (Player != null && (m_Player.position - transform.position).sqrMagnitude < m_MinDistanceSqr) return false;
            if (m_Spawned.Count >= m_NumberOfSpawns || m_TimeOfNextSpawn <= Time.time) return false;

            return true;
        }

        protected virtual GameObject[] Spawn(string namePostfix)
        {
            Vector3? position = GetPosition();

            if (position != null)
            {
                //Optimization: Use a pool
                GameObject[] spawned = m_SpawnDefinition.InstantiatePrefabs((Vector3)position, $"{m_SpawnerName} {namePostfix}");
                for (int idx = 0; idx < spawned.Length; idx++)
                {
                    m_Spawned.Add(spawned[idx].transform);
                    OnSpawn?.Invoke(spawned[idx]);
                }

                return spawned;
            }

            return null;
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
            if (Terrain.activeTerrain != null)
            {
                position.y = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;
            }
            
            if (onNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(position, out hit, 2, navMeshAreaMask))
                {
                    position = hit.position;
                } else
                {
                    return GetPosition(attemptNumber);
                }
            }

            return position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_Radius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_MinSpawnDistance);
        }
    }
}
