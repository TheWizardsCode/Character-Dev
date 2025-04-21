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
    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimatorActorController : BaseActorController
    {
        #region InspectorParameters
        [Header("Look IK")]
        [SerializeField, Tooltip("Should the actor use IK to look at a given target.")]
        bool m_IsLookAtIKActive = true;
        [SerializeField, Tooltip("The time it takes for the head to start moving when it needs to turn to look at something.")]
        float m_LookAtHeatTime = 0.2f;
        [SerializeField, Tooltip("The time it takes for the look IK rig to cool after reaching the correct look angle.")]
        float m_LookAtCoolTime = 0.2f;
        
        
        [Header("Animation")]
        [SerializeField, Tooltip("The animation controller for updating animations of the model representing this actor. If left empty no animations will be played.")]
        protected Animator m_Animator;
        [SerializeField, Tooltip("Should the character use Root Motion baked into the animations?")]
        bool m_UseRootMotion = false;
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the forward speed of the character.")]
        private string m_SpeedParameterName = "Forward";
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the turn angle of the character.")]
        private string m_TurnParameterName = "Turn";
        [SerializeField, Tooltip("The speed of this character when at a run. It will usually be going slower than this, and for short periods, can go faster (at a spring).")]
        private float m_RunningSpeed = 8;
        [Tooltip("If true then this script will control IK configuration of the character.")]
        public bool isFootIKActive = false;
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

            m_Agent = GetComponent<NavMeshAgent>();

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
            }

            if (!head || !animator)
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

        protected override void Update()
        {
            base.Update();
            UpdateMovement();
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
                m_Agent.speed = m_WalkSpeed;
            } else if (m_Agent.remainingDistance >= m_MinSprintDistance)
            {
                m_Agent.speed = m_MaxSpeed;
            } else
            {
                m_Agent.speed = m_MaxSpeed * m_RunSpeedFactor;
            }
            
            SetForwardAndTurnParameters();
        }

        private void SetForwardAndTurnParameters()
        {
            float magVelocity = m_Agent.velocity.magnitude;
            float speedParam = 0;
            if (!Mathf.Approximately(magVelocity, 0))
            {
                speedParam = magVelocity / m_MaxSpeed;
            }

            Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
            float turn = s.x;

            if (m_Animator)
            {
                m_Animator.SetFloat(m_SpeedParameterName, speedParam);
                m_Animator.SetFloat(m_TurnParameterName, turn);
            }

            if (Mathf.Abs(speedParam) > 0.05 || Mathf.Abs(turn) > 0.05)
            {
                state = States.Moving;
            }
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

            Vector3 curDir = m_CurrentLookAtPosition - head.position;
            Vector3 futDir = pos - head.position;

            curDir = Vector3.RotateTowards(curDir, futDir, m_LookAtSpeed * Time.deltaTime, float.PositiveInfinity);
            m_CurrentLookAtPosition = head.position + curDir;

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
        /// <summary>
        /// If the clip is non null then it will be played using a Playable. 
        /// </summary>
        /// <param name="clip"></param>
        // REFACTOR: PlayAnimationClip should be pulled up to the AnimatorActorController
        public void PlayAnimationClip(AnimationClip clip)
        {
            if (Animator == null) return;
            if (clip)
            {
                AnimationPlayableUtilities.PlayClip(Animator, clip, out _playableGraph);
            }
        }

        /// <summary>
        /// Switch back to animating using the animation controller.
        /// </summary>
        public void PlayAnimatorController()
        {           
            if (Animator == null) return;
            AnimationPlayableUtilities.PlayAnimatorController(Animator, m_AnimatorController, out _playableGraph);
        }
#endregion // Animation
    }
}