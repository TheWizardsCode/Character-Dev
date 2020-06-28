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
    [RequireComponent(typeof(NavMeshAgent))]
    public class Wander : MonoBehaviour
    {
        [SerializeField, Tooltip("The minimum time that a character will continue on a random. If the character reaches a waypoint within this time then they will continue in roughly the same direction.")]
        private float minTimeBetweenRandomPathChanges = 5;
        [SerializeField, Tooltip("The maximum time that a character will continue on a random path.")]
        private float maxTimeBetweenRandomPathChanges = 15;
        [SerializeField, Tooltip("The minimum distance the agent will typically travel on a given path before they change direction.")]
        private float minDistanceOfRandomPathChange = 10;
        [SerializeField, Tooltip("The maximum distance the agent will typically travel on a given path before they change direction.")]
        private float maxDistanceOfRandomPathChange = 20;
        [SerializeField, Tooltip("The minimum angle that the character will deviate from the current path when changing the wander direction.")]
        private float minAngleOfRandomPathChange = -60;
        [SerializeField, Tooltip("The maximum angle that the character will deviate from the current path when changing the wander direction.")]
        private float maxAngleOfRandomPathChange = 60;
        [SerializeField, Tooltip("The approximate maximum range the agent will normally wander from their start position.")]
        private float m_MaxWanderRange = 50f;

        private Transform m_Target;
        private float timeOfNextWanderPathChange;
        private GameObject m_WanderTarget;
        private Vector3 m_StartPosition;
        private NavMeshAgent m_Agent;
        private Terrain m_Terrain;

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
            }
        }

        internal void Awake()
        {
            m_StartPosition = transform.position;
            m_WanderTarget = new GameObject(gameObject.name + " wander target.");
            m_Agent = GetComponent<NavMeshAgent > ();
            Debug.Assert(m_Agent != null, "Characters with a wander behaviour must also have a NavMesh Agent.");

            Vector3 pos = transform.position;
            m_Terrain = Terrain.activeTerrain;
            if (m_Terrain != null)
            {
                pos.y = m_Terrain.SampleHeight(pos);
            }
            m_Agent.Warp(pos);
        }

        public bool HasReachedTarget
        {
            get
            {
                if (m_Agent.hasPath && !m_Agent.pathPending)
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
            if (HasReachedTarget)
            {
                OnReachedTarget();
            }
            
            if (Time.timeSinceLevelLoad > timeOfNextWanderPathChange || !m_Agent.hasPath || m_Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                UpdateWanderTarget();
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
        /// Get a wander target within a cone that is valid. If a valid (i.e. reachable) target is not found within the specified
        /// number of attemtpts then return the characters starting position as the target.
        /// </summary>
        /// <param name="maxAttemptCount">The maximum number of attempts to find a valid target</param>
        /// <returns></returns>
        private Vector3 GetValidWanderPosition(int maxAttemptCount = 10)
        {
            bool turning = false;
            int attemptCount = 1;

            while (attemptCount <= maxAttemptCount)
            {
                attemptCount++;
                if (!turning && attemptCount > maxAttemptCount / 2)
                {
                    turning = true;
                }

                Vector3 position;
                float minDistance = minDistanceOfRandomPathChange;
                float maxDistance = maxDistanceOfRandomPathChange;

                
                float rotation = Random.Range(minAngleOfRandomPathChange, maxAngleOfRandomPathChange);
                Quaternion randAng = Quaternion.Euler(0, rotation, 0);

                if (!turning)
                {
                    position = transform.position + ((randAng * transform.forward) * Random.Range(minDistance, maxDistance));
                } else
                {
                    position = transform.position + ((randAng * -transform.forward) * Random.Range(minDistance, maxDistance));
                }

                if (Vector3.Distance(m_StartPosition, position) <= m_MaxWanderRange)
                {
                    if (m_Terrain != null)
                    {
                        position.y = m_Terrain.SampleHeight(position);
                    }

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(position, out hit, transform.lossyScale.y * 2, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }
                }
            }

            if (Vector3.Distance(transform.position, m_StartPosition) > minDistanceOfRandomPathChange)
            {
                return m_StartPosition;
            }
            else
            {
                if (m_Terrain != null)
                {
                    float y = m_Terrain.SampleHeight(Vector3.zero);
                    return new Vector3(m_Terrain.terrainData.heightmapResolution / 2, y, m_Terrain.terrainData.heightmapResolution / 2);
                } else
                {
                    return Vector3.zero;
                }
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            DrawWanderAreaGizmo();
            DrawWanderTargetGizmo();
            DrawWanderRangeGizmo();
        }

        private void DrawWanderRangeGizmo()
        {
            Gizmos.DrawWireSphere(m_StartPosition, m_MaxWanderRange);
        }

        private void DrawWanderTargetGizmo()
        {
            if (m_WanderTarget != null)
            {
                Gizmos.DrawSphere(m_WanderTarget.transform.position, 0.2f);
            }
        }

        private void DrawWanderAreaGizmo()
        {
            float totalWanderArc = Mathf.Abs(minAngleOfRandomPathChange) + Mathf.Abs(maxAngleOfRandomPathChange);
            float rayRange = maxDistanceOfRandomPathChange;
            float halfFOV = totalWanderArc / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
            Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
        }
#endif
    }
}