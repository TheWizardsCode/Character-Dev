using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

namespace WizardsCode.Character
{
    /// <summary>
    /// The AnimatorActorController is a base class for characters that use an animator to control their movement and animations.
    /// It provides functionality for controlling the character's movement using a NavMeshAgent, updating animations based on the character's speed and direction, and using IK for look-at and foot placement.
    /// It also provides functionality for playing animation clips and switching back to the animator controller.
    /// </summary>
    public class AnimatorActorController : BaseActorController
    {
        #region InspectorParameters
        [Space]
        // IK Configuration
        [SerializeField, Tooltip("Should the actor use IK to look at a given target."), BoxGroup("IK Configuration")]
        bool m_IsLookAtIKActive = true;
        [SerializeField, Tooltip("The time it takes for the head to start moving when it needs to turn to look at something."), ShowIf("m_IsLookAtIKActive"), BoxGroup("IK Configuration")]
        float m_LookAtHeatTime = 0.2f;
        [SerializeField, Tooltip("The time it takes for the look IK rig to cool after reaching the correct look angle."), ShowIf("m_IsLookAtIKActive"), BoxGroup("IK Configuration")]
        float m_LookAtCoolTime = 0.2f;
        [SerializeField, Tooltip("If true then this script will control foot IK."), BoxGroup("IK Configuration")]
        public bool isFootIKActive = false;
        
        
        [Space]
        // Animation
        [SerializeField, Tooltip("The animation controller for updating animations of the model representing this actor. If left empty no animations will be played."), BoxGroup("Animation")]
        protected Animator m_Animator;
        [SerializeField, Tooltip("Should the character use Root Motion baked into the animations?"), BoxGroup("Animation")]
        bool m_UseRootMotion = true;
        [SerializeField, Tooltip("The smoothing factor for the NavMeshAgent to Animation sync. The higher this is the more smoothly they will sync, but at the cost of responsiveness"), Range(0.1f, 1.0f), BoxGroup("Animation")]
        float m_SyncSmoothing = 0.5f;
        [SerializeField, Tooltip("The maximum distance from the intended next position of the NavMeshAgent and the current position of the character. If the character drifts further than this away they will be forced back together. This is expressed as a multiple of the NavMeshAgents radius. Higher values will allow greater deltas, resulting in more natural motion until it needs to be corrected, which can be sudden and noticeable."), Range(0.1f, 1f), BoxGroup("Animation")]
        float m_MaxPositionDeltaRatio = 0.5f;
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the forward speed of the character."), BoxGroup("Animation")]
        private string m_SpeedParameterName = "Forward";
        [SerializeField, Tooltip("The damping time for the speed animation parameter. Higher values will result in more gradual speed changes, but can lead to sluggishness."), BoxGroup("Animation")]
        float speedDampTime = 0f;
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the turn angle of the character."), BoxGroup("Animation")]
        private string m_TurnParameterName = "Turn";
        [SerializeField, Tooltip("The damping time for the direction animation parameter. Higher values will result in more gradual turns, but can lead to sluggishness."), BoxGroup("Animation")]
        float directionDampTime = 0.5f;
        
        #endregion

        protected NavMeshAgent m_Agent;
        private float lookAtWeight = 0.0f;
        Transform m_LeftFootPosition = default;
        Transform m_RightFootPosition = default;

        private PlayableGraph _playableGraph;
        private RuntimeAnimatorController m_AnimatorController;

        public Animator Animator
        {
            get { return m_Animator; }
        }
        
        protected override bool IsArriving
        {
            get
            {
                return !m_Agent.pathPending && m_Agent.remainingDistance <= ArrivingDistance;
            }
        }

        protected override bool HasArrived
        {
            get
            {
                return !m_Agent.pathPending && m_Agent.remainingDistance <= m_Agent.stoppingDistance;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_Agent = GetComponentInChildren<NavMeshAgent>();

            if (m_Agent.isOnNavMesh)
            {
                MoveTargetPosition = transform.position;
            } else
            {
                m_Agent.enabled = false;
            }

            Animator animator = GetComponentInChildren<Animator>();
            if (animator)
            {
                if (animator.applyRootMotion != m_UseRootMotion)
                {
                    Debug.LogWarning($"`{displayName}` does not have a consistent root motion setting in the AnimatorActorController and the Animator. Overriding to `{m_UseRootMotion}` from the AnimatorActorController.");
                    animator.applyRootMotion = m_UseRootMotion;
                }

                if (m_UseRootMotion)
                {
                    m_Agent.updatePosition = false;
                    m_Agent.updateRotation = false;
                }
                else
                {
                    m_Agent.updatePosition = true;
                    m_Agent.updateRotation = true;
                }
            }

            m_AnimatorController = m_Animator.runtimeAnimatorController;
            if (m_AnimatorController == null)
            {
                Debug.LogWarning($"`{displayName}` does not have an Animator Controller set in the Animator. No animations will be played.");
            }

            if (!HeadBone || !animator)
            {
                m_IsLookAtIKActive = false;
            }

            m_WalkSpeed = m_MaxSpeed * m_WalkSpeedFactor;
            m_RunSpeed = m_MaxSpeed * m_RunSpeedFactor;
            m_runSqrMagnitude = m_WalkSpeed * m_WalkSpeed;
            m_sprintSqrMagnitude = m_RunSpeed * m_RunSpeed;

            m_Agent = GetComponent<NavMeshAgent>();
            if (m_Agent != null)
            {
                m_Agent.stoppingDistance = ArrivingDistance / 2;
            }
        }

        void OnAnimatorMove()
        {
            Vector3 rootPosition = m_Animator.rootPosition;
            rootPosition.y = m_Agent.nextPosition.y;
            transform.position = rootPosition;
            transform.rotation = m_Animator.rootRotation;
            m_Agent.nextPosition = rootPosition;    
        }

        protected override void Update()
        {
            base.Update();
            UpdateMovement();
        }

        public override void Prompt(ActorCue cue, float duration) {
            if (cue == null) return;

            cue.Duration = duration;
            if (cue is ActorCueAnimator animationCue)
            {
                animationCue.DurationMatchesAnimation = false;
            }

            Prompt(cue);
        }

#region Movement

        /// <summary>
        /// Get or set the position of the move target. If the character is able to 
        /// move and the distance to the target is greater than the stopping distance then
        /// the character will move to the target.
        /// </summary>
        public override Vector3 MoveTargetPosition
        {
            get { return base.MoveTargetPosition; }
            set
            {
                if (MoveTargetPosition == value)
                {
                    return;
                }
                
                base.MoveTargetPosition = value;
                if (Vector3.Distance(m_Agent.destination, value) > m_Agent.stoppingDistance)
                {
                    m_Agent.SetDestination(value);
                    state = States.Moving;
                }
            }
        }

        public Vector2 m_smoothDeltaPosition { get; private set; }

        private Vector2 m_Velocity;



        /// <summary>
        /// Stop the actor from moving. Clearing the current path if there is one.
        /// </summary>
        public override void StopMoving()
        {
            if (!m_Agent) return;

            m_Agent.ResetPath();
        }

        protected override void UpdateMovement()
        {
            if (m_Agent.remainingDistance <= m_MinRunDistance)
            {
                m_Agent.speed = Mathf.Lerp(m_Agent.speed, m_WalkSpeed, Time.deltaTime * 2);
            } else if (m_Agent.remainingDistance >= m_MinSprintDistance)
            {
                m_Agent.speed = Mathf.Lerp(m_Agent.speed, m_MaxSpeed, Time.deltaTime * 2);
            } else
            {
                m_Agent.speed = Mathf.Lerp(m_Agent.speed, m_RunSpeed, Time.deltaTime * 2);
            }
            
            SynchronizeAnimatorAndAgent();
        }

        /// <summary>
        /// Teleport the actor to a location. The actor will inherit the rotation and position of the location.
        /// </summary>
        /// <param name="location">The location to teleport to.</param>
        /// <param name="aiActive">Should the AI brain, if the actor has one, be active after the teleport?</param>
        public override void Teleport(Transform location, bool aiActive)
        {
            if (m_Agent)
            {
                m_Agent.Warp(location.position);
            }

            base.Teleport(location, aiActive);
        }
#endregion // Movement

#region IK
        private void OnAnimatorIK(int layerIndex)
        {
            LookAtIK();
        }

        void OnAnimatorIK()
        {
            LookAtIK();
            FeetIK();
        }

        protected void LookAtIK()
        {
            if (!m_IsLookAtIKActive)
            {
                return;
            }

            Vector3 pos = LookAtTarget.position;
            //pos.y = head.position.y;

            float lookAtTargetWeight = m_IsLookAtIKActive ? 1.0f : 0.0f;

            Vector3 curDir = m_CurrentLookAtPosition - HeadBone.position;
            Vector3 futDir = pos - HeadBone.position;

            curDir = Vector3.RotateTowards(curDir, futDir, m_LookAtSpeed * Time.deltaTime, float.PositiveInfinity);
            m_CurrentLookAtPosition = HeadBone.position + curDir;

            float blendTime = lookAtTargetWeight > lookAtWeight ? m_LookAtHeatTime : m_LookAtCoolTime;
            lookAtWeight = Mathf.MoveTowards(lookAtWeight, lookAtTargetWeight, Time.deltaTime / blendTime);
            m_Animator.SetLookAtWeight(lookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
            m_Animator.SetLookAtPosition(m_CurrentLookAtPosition);
        }

        private void FeetIK()
        {
            if (!m_Animator || !isFootIKActive) return;

            if (m_RightFootPosition != null)
            {
                m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, m_RightFootPosition.position);
                m_Animator.SetIKRotation(AvatarIKGoal.RightFoot, m_RightFootPosition.rotation);
            }
            if (m_LeftFootPosition != null)
            {
                m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, m_LeftFootPosition.position);
                m_Animator.SetIKRotation(AvatarIKGoal.LeftFoot, m_LeftFootPosition.rotation);
            }
        }
#endregion // IK

#region Animation
        private void SynchronizeAnimatorAndAgent()
        {
            if (m_UseRootMotion)
            {
                Vector3 worldDeltaPosition = m_Agent.nextPosition - transform.position;
                worldDeltaPosition.y = 0;

                float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                Vector2 delta = new Vector2(dx, dy);

                float smooth = Mathf.Min(1, Time.deltaTime / m_SyncSmoothing);
                m_smoothDeltaPosition = Vector2.Lerp(m_smoothDeltaPosition, delta, smooth);

                m_Velocity = m_smoothDeltaPosition / Time.deltaTime;
                if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
                {
                    m_Velocity = Vector2.Lerp(Vector2.zero, m_Velocity, m_Agent.remainingDistance / m_Agent.stoppingDistance);
                }

                m_Velocity = m_Velocity.normalized;

                float turn = 0;
                if (m_Agent.path != null && m_Agent.path.corners.Length > 1)
                {
                    Vector3 toNextCorner = m_Agent.path.corners[1] - transform.position;
                    toNextCorner.y = 0;
                    toNextCorner.Normalize();

                    Vector3 localDirection = transform.InverseTransformDirection(toNextCorner);
                    turn = localDirection.x;
                }
                else
                {
                    // fallback to velocity-based turn
                    Vector3 direction = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
                    turn = direction.x;
                }

                bool isMoving = (m_Velocity.magnitude > 0.03f || Mathf.Abs(turn) > 0.05f)
                    && m_Agent.remainingDistance > m_Agent.stoppingDistance;
                if (isMoving)
                {   
                    // OPTIMIZATION: Use hashes for animation parameters
                    m_Animator.SetFloat(m_SpeedParameterName, m_Velocity.magnitude / m_MaxSpeed, speedDampTime, Time.deltaTime);
                    m_Animator.SetFloat(m_TurnParameterName, turn, directionDampTime, Time.deltaTime);
                    state = States.Moving;
                }
                else
                {
                    // OPTIMIZATION: Use hashes for animation parameters
                    m_Animator.SetFloat(m_SpeedParameterName, 0);
                    m_Animator.SetFloat(m_TurnParameterName, 0);
                    state = States.Idle;
                }

                float deltaMagnitude = worldDeltaPosition.magnitude;
                if (deltaMagnitude > m_Agent.radius * m_MaxPositionDeltaRatio)
                {
                    transform.position = Vector3.Lerp(m_Animator.rootPosition, m_Agent.nextPosition, smooth);
                }
            }
            else
            {
                float magVelocity = m_Agent.velocity.magnitude;
                float speedParam = 0;
                if (!Mathf.Approximately(magVelocity, 0))
                {
                    speedParam = magVelocity / m_MaxSpeed;
                }

                if (Mathf.Abs(speedParam) > 0.05) 
                {
                    // OPTIMIZATION: Use hashes for animation parameters
                    m_Animator.SetFloat(m_SpeedParameterName, speedParam, speedDampTime, Time.deltaTime);
                    state = States.Moving;
                }
                else
                {
                    m_Animator.SetFloat(m_SpeedParameterName, 0);
                }

                Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
                float turn = s.x;
                if (Mathf.Abs(turn) > 0.05) {
                    // OPTIMIZATION: Use hashes for animation parameters
                    m_Animator.SetFloat(m_TurnParameterName, turn, directionDampTime, Time.deltaTime);
                    state = States.Moving;
                } else {
                    m_Animator.SetFloat(m_TurnParameterName, 0);
                }
            }
        }
        
        /// <summary>
        /// If the clip is non null then it will be played using a Playable. This will stop the animator from controlling the character.
        /// To set the animator back to using the animator controller use `PlayAnimatorController()`.
        /// </summary>
        /// <param name="clip"></param>
        /// <seealso cref="PlayAnimatorController"/>
        public void PlayAnimationClip(AnimationClip clip)
        {
            if (Animator == null) return;
            if (clip)
            {
                AnimationPlayableUtilities.PlayClip(Animator, clip, out _playableGraph);
            }
        }

        /// <summary>
        /// Switch back to animating using the animation controller after playing a clip using a playable.
        /// </summary>
        /// <seealso cref="PlayAnimationClip"/>
        public void PlayAnimatorController()
        {           
            if (Animator == null) return;
            AnimationPlayableUtilities.PlayAnimatorController(Animator, m_AnimatorController, out _playableGraph);
        }
#endregion // Animation


        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_Animator == null)
            {
                m_Animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnDrawGizmosSelected() {
            if (m_Agent != null && m_Agent.hasPath)
            {
                Gizmos.color = Color.cyan;
                var path = m_Agent.path;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                    Gizmos.DrawSphere(path.corners[i], 0.08f);
                }
            }
        }
        #endif
    }
}