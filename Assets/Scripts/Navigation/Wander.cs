using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace threeDTBD.Character
{
    /// <summary>
    /// Make the cahracter wander semi-randomly. They won't necessarily change
    /// direction frequently but will, instead, continue in roughly the same
    /// direction for some time. Eventually they will get bored and change
    /// direction.
    /// </summary>
    public class Wander : MonoBehaviour
    {
        [SerializeField, Tooltip("The minimum time that a character will continue on a random. If the character reaches a waypoint within this time then they will continue in roughly the same direction.")]
        private float minTimeBetweenRandomPathChanges = 5;
        [SerializeField, Tooltip("The maximum time that a character will continue on a random path.")]
        private float maxTimeBetweenRandomPathChanges = 15;
        [SerializeField, Tooltip("The minimum distance the agent will typically travel on a given path before they change direction.")]
        private float minDistanceOfRandomPathChange = 10;
        [SerializeField, Tooltip("The maximum distance the agent will typically travel on a given path before they change direction.")]
        private float maxDistanceOfRandomPathChange = 25;
        [SerializeField, Tooltip("The minimum angle that the character will deviate from the current path when changing the wander direction.")]
        private float minAngleOfRandomPathChange = -25;
        [SerializeField, Tooltip("The maximum angle that the character will deviate from the current path when changing the wander direction.")]
        private float maxAngleOfRandomPathChange = 25;

        private Transform m_Target;
        private float timeOfNextWanderPathChange;
        private GameObject m_WanderTarget;
        private Vector3 m_StartPosition;
        private NavMeshAgent m_Agent;

        /// <summary>
        /// Get or set the current target.
        /// </summary>
        virtual public Transform currentTarget
        {
            get { return m_Target; }
            set
            {
                m_Target = value;
                m_Agent.SetDestination(m_Target.position);
                timeOfNextWanderPathChange = Random.Range(minTimeBetweenRandomPathChanges, maxTimeBetweenRandomPathChanges);

                if (value)
                {
                    Vector3 pos = value.transform.position;
                    float height = Terrain.activeTerrain.SampleHeight(pos);

                    if (height < m_Agent.stoppingDistance * 0.75)
                    {
                        pos.y = height;
                    }
                }
                else
                {
                    m_Target = null;
                }
            }
        }

        internal void Awake()
        {
            m_WanderTarget = new GameObject(gameObject.name + " wander target.");
            m_Agent = GetComponent<NavMeshAgent > ();
            Debug.Assert(m_Agent != null, "Characters with a wander behaviour must also have a NavMesh Agent.");
        }

        public bool HasReachedTarget
        {
            get
            {
                if (!m_Agent.pathPending)
                {
                    if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
                    {
                        if (!m_Agent.hasPath || m_Agent.velocity.sqrMagnitude == 0f)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        internal void Update()
        {
            UpdateMove();
        }

        virtual internal void UpdateMove()
        {
            if (Time.timeSinceLevelLoad > timeOfNextWanderPathChange || !m_Agent.hasPath)
            {
                UpdateWanderTarget();
            }
            else if (HasReachedTarget)
            {
                OnReachedTarget();
            }
        }

        /// <summary>
        /// Update the WanderTarget position..
        /// A new position for the target is chosen within a cone defined by the
        /// minAngleOfRandomPathChange and maxAngleOfRandomPathChange. Optionally,
        /// the cone can extend behind the current agent, which has the effect of 
        /// turning the agent around.
        /// </summary>
        internal void UpdateWanderTarget()
        {
            Vector3 position = GetValidWanderPosition();

            m_WanderTarget.transform.position = position;

            currentTarget = m_WanderTarget.transform;

            timeOfNextWanderPathChange = Time.timeSinceLevelLoad + Random.Range(minTimeBetweenRandomPathChanges, maxTimeBetweenRandomPathChanges);
        }

        /// <summary>
        /// Called when a target has been reached.
        /// </summary>
        internal virtual void OnReachedTarget()
        {

        }

        /// <summary>
        /// Test to see if a given point is a valid waypoint for this agent.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        virtual internal bool IsValidWaypoint(Vector3 position)
        {
            Bounds bounds = Terrain.activeTerrain.terrainData.bounds;
            return bounds.Contains(position);
        }

        /// <summary>
        /// Get a wander target within a cone that is valid. If a valid (i.e. reachable) target is not found within the specified
        /// number of attemtpts then return the characters starting position as the target.
        /// </summary>
        /// <param name="maxAttemptCount">The maximum number of attempts to find a valid target</param>
        /// <returns></returns>
        private Vector3 GetValidWanderPosition(int maxAttemptCount = 10)
        {
            bool turnAround = false;
            int attemptCount = 1;

            while (attemptCount <= maxAttemptCount)
            {
                attemptCount++;
                if (attemptCount > maxAttemptCount / 2)
                {
                    turnAround = true;
                }

                Vector3 position;
                float minDistance = minDistanceOfRandomPathChange;
                float maxDistance = maxDistanceOfRandomPathChange;

                Quaternion randAng;
                if (!turnAround)
                {
                    randAng = Quaternion.Euler(0, Random.Range(minAngleOfRandomPathChange, maxAngleOfRandomPathChange), 0);
                }
                else
                {
                    randAng = Quaternion.Euler(0, Random.Range(180 - minAngleOfRandomPathChange, 180 + maxAngleOfRandomPathChange), 0);
                    minDistance = maxDistance;
                }
                transform.rotation = transform.rotation * randAng;
                position = transform.position + randAng * Vector3.forward * Random.Range(minDistance, maxDistance);

                float terrainHeight = Terrain.activeTerrain.SampleHeight(position);
                position.y = terrainHeight;

                if (IsValidWaypoint(position))
                {
                    return position;
                }
            }

            if (attemptCount > maxAttemptCount)
            {
                return m_StartPosition;
            }
            else
            {
                // should never reach here, but just in case...
                return Vector3.zero;
            }
        }
    }
}