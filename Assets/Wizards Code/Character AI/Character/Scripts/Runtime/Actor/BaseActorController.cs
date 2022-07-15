using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Playables;
using WizardsCode.AnimationControl;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// A character actor performs for the camera and takes cues from a director.
    /// Converts NavMesh movement to animation controller parameters.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BaseActorController : MonoBehaviour
    {
        public enum States { Idle, Moving, Arriving, Arrived }

        #region InspectorParameters
        [Header("Character Ground Movement")]
        [SerializeField, Tooltip("The maximum speed of this character, this will usually be a full sprint.")]
        protected float m_MaxSpeed = 8f;
        [SerializeField, Tooltip("If the distance the actor needs to travel to reach their destination is greater than this and the actor can run then they will do so.")]
        protected float m_MinRunDistance = 15;
        [SerializeField, Tooltip("If the distance the actor needs to travel to reach their destination is greater than this and the actor can sprint then they will do so.")]
        protected float m_MinSprintDistance = 30;
        [SerializeField, Tooltip("The factor used to calculate the top walking speed of the character relative to the maximum speed. The higher this value the faster the character needs to be moving before switching to a run animation.")]
        protected float m_WalkSpeedFactor = 0.45f;
        [SerializeField, Tooltip("The factor used to calculate the top running speed of the character relative to the maximum speed. The higher this value the faster the character needs to be moving before switching to a sprint animation.")]
        protected float m_RunSpeedFactor = 0.8f;
        [SerializeField, Tooltip("The distance within which the character is considered to be arriving at their destination. This is used to allow callbacks for when the character has nearly completed their movement. This can be useful when the character needs to, for example, turn to sit on a chair just before reaching the final stopping point.")]
        protected float m_ArrivingDistance = 1f;

        [Header("Look")]
        [SerializeField, Tooltip("A transform at the point in space that the actor should look towards.")]
        Transform m_LookAtTarget;
        [SerializeField, Tooltip("Should the actor use IK to look at a given target.")]
        bool m_IsLookAtIKActive = true;
        [SerializeField, Tooltip("The head bone, used for Look IK. If this is blank there will be an attempt to automatically find the head upon startup.")]
        public Transform head = null;
        [SerializeField, Tooltip("The speed at which a character will turn their head to look at a target.")]
        float m_LookAtSpeed = 6f;
        [SerializeField, Tooltip("The time it takes for the head to start moving when it needs to turn to look at something.")]
        float m_LookAtHeatTime = 0.2f;
        [SerializeField, Tooltip("The time it takes for the look IK rig to cool after reaching the correct look angle.")]
        float m_LookAtCoolTime = 0.2f;

        [Header("Animation")]
        [SerializeField, Tooltip("The animation controller for updating animations of the model representing this actor. If left empty no animations will be played.")]
        protected Animator m_Animator;
        #endregion

        #region Public Variables
        [HideInInspector]
        public Action onStationary;
        bool hasMoved = false;
        [HideInInspector]
        public Action onArriving;
        [HideInInspector]
        public Action onArrived;
        #endregion

        #region Members
        protected NavMeshAgent m_Agent;
        private float lastStateChangeTime = float.NegativeInfinity;
        private States m_state;
        #endregion

        #region Properties
        public string displayName
        {
            get { return brain.DisplayName; }
        }

        public Brain brain
        {
            get;
            internal set;
        }

        public States state
        {
            get { return m_state; } 
            set
            {
                if (m_state != value)
                {
                    lastStateChangeTime = Time.timeSinceLevelLoad;
                    m_state = value;
                }
            }
        }

        private Vector3 m_CurrentLookAtPosition;
        private float lookAtWeight = 0.0f;

        protected float m_WalkSpeed;
        protected float m_RunSpeed;

        Quaternion desiredRotation = default;
        bool isRotating = false;

        AnimationLayerController m_AnimationLayers;
        #endregion

        public float ArrivingDistance
        {
            get { return m_ArrivingDistance; }
            set { m_ArrivingDistance = value; }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            LookAtIK();
        }

        /// <summary>
        /// If the clip is non null then it will be played using a Playable. 
        /// </summary>
        /// <param name="clip"></param>
        public void PlayAnimationClip(AnimationClip clip)
        {
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
            AnimationPlayableUtilities.PlayAnimatorController(Animator, m_AnimatorController, out _playableGraph);
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

        /// <summary>
        /// Set the LookAtTarget to a specific position in world space.
        /// </summary>
        /// <param name="position"></param>
        public void LookAt(Vector3 position)
        {
            m_LookAtTarget.position = position;
        }

        public Transform LookAtTarget
        {
            get {
                if (!m_LookAtTarget)
                {
                    GameObject go = new GameObject($"Look At target for {gameObject.name}");
                    go.transform.SetParent(gameObject.transform);
                    m_LookAtTarget = go.transform;
                    ResetLookAt();
                }
                return m_LookAtTarget;
            }
            set
            {
                m_LookAtTarget.transform.SetParent(value);
                m_LookAtTarget.localPosition = Vector3.zero;
                m_LookAtTarget.localRotation = Quaternion.identity;
            }
        }

        public Animator Animator
        {
            get { return m_Animator; }
        }

        #region Actions
        /// <summary>
        /// Instruct the character to move to a defined positiion and, optionally, 
        /// make callbacks at various points in the process.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="arrivingCallback">Called as the character is arriving at the destination. This is called when the character is entering the ArrivingDistance defined in the NavMeshController.</param>
        /// <param name="arrivedCallback">Called as the character arrives at the destination. Arrival is defined by the NavMeshAgent.</param>
        /// <param name="stationaryCallback">Called once the character has stopped moving.</param>
        public void MoveTo(Vector3 position, Action arrivingCallback, Action arrivedCallback, Action stationaryCallback)
        {
            onArriving = arrivingCallback;
            onArrived = arrivedCallback;
            onStationary = stationaryCallback;
            MoveTargetPosition = position;
        }

        public virtual void MoveTo(Transform destination)
        {
            MoveTo(destination.position, null, null, null);
        }

        public virtual void MoveTo(Vector3 position)
        {
            MoveTo(position, null, null, null);
        }

        public void TurnTo(Quaternion rotation)
        {
            desiredRotation = rotation;
            isRotating = true;
        }

        public void TurnToFace(Vector3 position)
        {
            TurnTo(Quaternion.LookRotation(position - transform.position, Vector3.up));
        }
        #endregion

        public Vector3 MoveTargetPosition
        {
            get { return m_Agent.destination; }
            set
            {
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
        public void StopMoving()
        {
            if (!m_Agent) return;

            m_Agent.ResetPath();
        }

        System.Collections.IEnumerator cueCoroutine;
        /// <summary>
        /// Prompt the actor to enact a cue. A cue describes
        /// a position and actions that an actor should take.
        /// </summary>
        /// <param name="cue">The cue to enact.</param>
        public virtual void Prompt(ActorCue cue)
        {
            if (cue == null) return;

            cueCoroutine = cue.Prompt(this);
            if (cueCoroutine != null)
            {
                StartCoroutine(cueCoroutine);
            }
        }

        protected float m_runSqrMagnitude;
        protected float m_sprintSqrMagnitude;
        private Transform m_InteractionPoint;
        private PlayableGraph _playableGraph;
        private RuntimeAnimatorController m_AnimatorController;

        protected virtual void Awake()
        {
            if (m_LookAtTarget == null)
            {
                m_LookAtTarget = new GameObject("Look At Target").transform;
                m_LookAtTarget.SetParent(transform);
                m_LookAtTarget.localPosition = new Vector3(0, 1.6f, 0.5f);
            }

            m_WalkSpeed = m_MaxSpeed * m_WalkSpeedFactor;
            m_RunSpeed = m_MaxSpeed * m_RunSpeedFactor;
            m_runSqrMagnitude = m_WalkSpeed * m_WalkSpeed;
            m_sprintSqrMagnitude = m_RunSpeed * m_RunSpeed;

            if (m_Animator == null)
            {
                m_Animator = GetComponentInChildren<Animator>();
            }
            m_AnimationLayers = GetComponentInChildren<AnimationLayerController>();
            m_AnimatorController = m_Animator.runtimeAnimatorController;

            m_Agent = GetComponent<NavMeshAgent>();
            if (m_Agent != null)
            {
                m_Agent.stoppingDistance = ArrivingDistance / 2;
            }
            brain = GetComponentInChildren<Brain>();
            if (m_Agent.isOnNavMesh)
            {
                MoveTargetPosition = transform.position;
            } else
            {
                m_Agent.enabled = false;
            }

            // Look IK Setup
            if (!head)
            {
                head = transform.Find("Head");
            }
            if (!head)
            {
                Debug.LogWarning("No head transform set on " + gameObject.name + " and one could not be found automatically - LookAt disabled");
                m_IsLookAtIKActive = false;
            }

            m_InteractionPoint = new GameObject(transform.root.name + " Interaction Point").transform;

            m_LookAtTarget.name = transform.root.name + " Look At Target";
        }

        [Obsolete("Use an ActorCue that plays the chosen Emote.")] // v0.0.9
        public void PlayEmote(string name)
        {
            Animator animator = GetComponent<Animator>();
            animator.Play(name);
        }

        protected virtual void Update()
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

            ManageState();
            
            if (LookAtTarget != null)
            {
                float sqrMagToLookAtTarget = Vector3.SqrMagnitude(LookAtTarget.position - transform.position);
                if (sqrMagToLookAtTarget > 100)
                {
                    ResetLookAt();
                }
            }

            RotateIfNeeded();
        }

        /// <summary>
        /// If the actor is already rotating continue that rotation. If the character is trying
        /// to look at something that is outside of the angle their head should turn then 
        /// start rotating the character.
        /// </summary>
        private void RotateIfNeeded()
        {
            if (!isRotating && m_LookAtTarget != null)
            {
                Vector3 lookAtHeading = LookAtTarget.position - transform.position;
                float dot = Vector3.Dot(lookAtHeading, transform.forward);
                if (dot <= 0.1f) // LookAtTarget is to the side or behind
                {
                    isRotating = true;
                    desiredRotation = Quaternion.LookRotation(lookAtHeading, Vector3.up);
                }
            }

            if (isRotating)
            {
                if (Quaternion.Angle(transform.rotation, desiredRotation) >= 0.5f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * m_LookAtSpeed);
                }
                else
                {
                    isRotating = false;
                }
            }
        }
        
        /// <summary>
        /// if appropriate update the current state of the actor and make any animation callbacks
        /// necessary.
        /// </summary>
        private void ManageState()
        {
            switch (state)
            {
                case States.Idle:
                    if (hasMoved && onStationary != null)
                    {
                        onStationary();
                        onStationary = null;
                        hasMoved = false;
                    }
                    else if (m_Agent.remainingDistance > ArrivingDistance)
                    {
                        state = States.Moving;
                    }
                    break;
                case States.Moving:
                    if (m_Agent != null)
                    {
                        if (!m_Agent.pathPending && m_Agent.remainingDistance <= ArrivingDistance)
                        {
                            state = States.Arriving;
                            if (onArriving != null)
                            {
                                onArriving();
                                onArriving = null;
                            }
                        }
                    }

                    hasMoved = true;
                    break;
                case States.Arriving:
                    if (HasArrived())
                    {
                        if (onArrived != null)
                        {
                            onArrived();
                            onArrived = null;
                        }
                        state = States.Arrived;
                    }
                    break;
                case States.Arrived:
                    // Stay in the arrived state for a short while in case code or Ink narrative requires the character to do something. This small delay ensures that at least one frame passes before we allow the brain to make an independent decision.
                    if (Time.timeSinceLevelLoad + 0.1 > lastStateChangeTime)
                    {
                        state = States.Idle;
                    }
                    break;
                default:
                    break;
            }
        }

        internal bool HasArrived()
        {
            return m_Agent.remainingDistance <= m_Agent.stoppingDistance;
        }

        public bool IsMoving
        {
            get
            {
                return state == States.Moving 
                    || state == States.Arriving
                    || state == States.Arrived;
            }
        }

        /// <summary>
        /// A measure of how noticable this character is from 0 to 1. 
        /// 0 is as good as invisible, 1 is can't miss them.
        /// How noticable an actor is depends on what they are doing
        /// at any given time as well as their emations. For example, 
        /// a fearful character who is resting is less noticeable
        /// than an interested character. Anger will increase noticability,
        /// but sadness will reduce it. Similarly a character who is attacking
        /// is more noticable than one who is idle.
        /// </summary>
        public float Noticability {
            get
            {
                float result = 0;

                EmotionalState emotion = GetComponent<EmotionalState>();
                if (emotion)
                {
                    result = emotion.Noticability;
                }

                //TODO currently active behaviour should impact noticability. Add a noticability factor to behaviours.
                return Mathf.Clamp01(result);
            }
        }

        public virtual bool isIdle { 
            get { return state == States.Idle; }
        }

        /// <summary>
        /// Move the look at target to its default position and parent it to the actor.
        /// </summary>
        public void ResetLookAt()
        {
            LookAtTarget.transform.SetParent(transform);
            Vector3 pos = Vector3.zero;
            if (head)
            {
                LookAtTarget.transform.position = head.position + transform.forward;
            } else
            {
                LookAtTarget.transform.localPosition = new Vector3(0, 1.7f, 1);
            }
            isRotating = false;
        }

        [Obsolete("Use an AnimationCuePrompt instead. Deprecated in 0.4.0.")]
        public void StartTalking()
        {
            if (m_AnimationLayers != null)
            {
                m_AnimationLayers.isTalking = true;
            }
        }

        [Obsolete("Use an AnimationCuePrompt instead. Deprecated in 0.4.0.")]
        public void StopTalking()
        {
            if (m_AnimationLayers != null)
            {
                m_AnimationLayers.isTalking = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Look At
            if (LookAtTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(LookAtTarget.position, 0.15f);
                if (head != null)
                {
                    Gizmos.DrawLine(head.position, LookAtTarget.position);
                }
            }
        }

        /// <summary>
        /// Teleport the actor to a location. The actor will inherit the rotation and position of the location.
        /// </summary>
        /// <param name="location">The location to teleport to.</param>
        /// <param name="aiActive">Should the AI brain, if the actor has one, be active after the teleport?</param>
        public void Teleport(Transform location, bool aiActive)
        {
            transform.position = location.position;
            transform.rotation = location.rotation;
            if (m_Agent)
            {
                m_Agent.Warp(location.position);
            }

            if (brain)
            {
                brain.active = aiActive;
            }
            StopMoving();
            ResetLookAt();
        }
    }
}