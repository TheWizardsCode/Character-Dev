using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Character.Stats;
using WizardsCode.BackgroundAI;

namespace WizardsCode.Character
{
    /// <summary>
    /// Make the cahracter wander semi-randomly. They won't necessarily change
    /// direction frequently but will, instead, continue in roughly the same
    /// direction for some time. Eventually they will get bored and change
    /// direction.
    /// </summary>
    public class Wander : AbstractAIBehaviour
#if UNITY_EDITOR
        , IDebug
#endif
    {
        [SerializeField, Tooltip("The minimum time that a character will continue on a random. If the character reaches a waypoint within this time then they will continue in roughly the same direction.")]
        private float minTimeBetweenRandomPathChanges = 10;
        [SerializeField, Tooltip("The maximum time that a character will continue on a random path.")]
        private float maxTimeBetweenRandomPathChanges = 20;
        [SerializeField, Tooltip("The minimum distance the agent will typically travel on a given path before they change direction.")]
        private float minDistanceOfRandomPathChange = 15;
        [SerializeField, Tooltip("The maximum distance the agent will typically travel on a given path before they change direction.")]
        private float maxDistanceOfRandomPathChange = 30;
        [SerializeField, Tooltip("The minimum angle that the character will deviate from the current path when changing the wander direction.")]
        private float minAngleOfRandomPathChange = -60;
        [SerializeField, Tooltip("The maximum angle that the character will deviate from the current path when changing the wander direction.")]
        private float maxAngleOfRandomPathChange = 60;
        [SerializeField, Tooltip("The approximate maximum range the agent will normally wander from their start position.")]
        private float m_MaxWanderRange = 50f;
        [SerializeField, NavMeshAreaMask, Tooltip("The area mask for allowed areas for this agent to wander within.")]
        public int navMeshAreaMask = NavMesh.AllAreas;

        private Vector3 m_TargetPosition;
        private float timeToNextWanderPathChange;
        private Vector3 m_StartPosition;
        private Terrain m_Terrain;
        
        protected override void Init()
        {
            base.Init();

            m_StartPosition = transform.position;
            timeToNextWanderPathChange = float.MinValue;
        }

        /// <summary>
        /// Get or set the current target.
        /// </summary>
        virtual public Vector3 currentTarget
        {
            get { return m_TargetPosition; }
            set
            {
                if (m_TargetPosition != value)
                {
                    m_TargetPosition = value;
                    ActorController.MoveTargetPosition = value;
                    timeToNextWanderPathChange = Random.Range(minTimeBetweenRandomPathChanges, maxTimeBetweenRandomPathChanges);
                }
            }
        }

        internal override void StartBehaviour()
        {
            base.StartBehaviour();

            Brain.Actor.Prompt(m_OnStartCue);
            Brain.Actor.Prompt(m_OnPrepareCue);

            UpdateMove();
        }

        /*
        protected override void OnUpdate()
        {
            timeToNextWanderPathChange -= Time.deltaTime;

            if (timeToNextWanderPathChange <= 0)
            {
                OnReachedTarget();
                FinishBehaviour();
            }
        }
        */

        internal override float FinishBehaviour()
        {
            float endTime = base.FinishBehaviour();
            Brain.Actor.StopMoving();
            timeToNextWanderPathChange = float.MinValue;
            return endTime;
        }

        /// <summary>
        /// Called whenever this agent is considering where to move to next.
        /// </summary>
        virtual protected void UpdateMove()
        {   
            UpdateWanderTarget();
        }

        /// <summary>
        /// Update the WanderTarget position.
        /// A new position for the target is chosen within a cone defined by the
        /// minAngleOfRandomPathChange and maxAngleOfRandomPathChange. Optionally,
        /// the cone can extend behind the current agent, which has the effect of 
        /// turning the agent around.
        /// </summary>
        internal void UpdateWanderTarget(int maxAttemptCount = 10)
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
                }
                else
                {
                    // TODO Rather than turning 180 degress we should turn a multiple of the max or min angles.
                    position = transform.position + ((randAng * -transform.forward) * Random.Range(minDistance, maxDistance));
                }

                if (Vector3.Distance(m_StartPosition, position) <= m_MaxWanderRange)
                {
                    if (m_Terrain != null)
                    {
                        position.y = m_Terrain.SampleHeight(position);
                    }

                    NavMeshHit hit;
                    if (!NavMesh.SamplePosition(position, out hit, transform.lossyScale.y * 5, navMeshAreaMask))
                    {
                        // This is not a valid NavMesh position, abort for this frame
                        continue;
                    }

                    if (Memory != null)
                    {
                        Collider[] hitColliders = Physics.OverlapSphere(position, 5f);
                        for (int i = 0; i < hitColliders.Length; i++)
                        {
                            if (hitColliders[i].gameObject != gameObject)
                            {
                                MemorySO[] memories = Memory.GetAllMemoriesAbout(hitColliders[i].gameObject);
                                for (int y = 0; y < memories.Length; y++)
                                {
                                    // TODO This avoids an endpoint within a space we don't like, but we also need to avoid paths that include area we don't like
                                    if (!memories[y].isGood)
                                    {
                                        //Debug.Log(gameObject.name + " does not like " + memories[y].about + " avoiding that area.");
                                        // need to avoid this area as we have bad memories of something here, abort for this frame
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    currentTarget = hit.position;
                    return;
                }
            }

            if (Vector3.Distance(transform.position, m_StartPosition) > minDistanceOfRandomPathChange)
            {
                currentTarget = m_StartPosition;
                return;
            }
            else
            {
                if (m_Terrain != null)
                {
                    float y = m_Terrain.SampleHeight(Vector3.zero);
                    currentTarget = new Vector3(m_Terrain.terrainData.heightmapResolution / 2, y, m_Terrain.terrainData.heightmapResolution / 2);
                    return;
                }
                else
                {
                    currentTarget = Vector3.zero;
                    return;
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            DrawWanderAreaGizmo();
            DrawWanderTargetGizmo();
            DrawWanderRangeGizmo();
        }

        protected void DrawWanderRangeGizmo()
        {
            Gizmos.DrawWireSphere(m_StartPosition, m_MaxWanderRange);
        }

        protected void DrawWanderTargetGizmo()
        {
            if (m_TargetPosition != null)
            {
                Gizmos.DrawSphere(currentTarget, 0.5f);
                Gizmos.DrawLine(transform.position, currentTarget);            }
        }

        protected void DrawWanderAreaGizmo()
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

        public virtual string StatusText()
        {
            return null;
        }
#endif
    }
}