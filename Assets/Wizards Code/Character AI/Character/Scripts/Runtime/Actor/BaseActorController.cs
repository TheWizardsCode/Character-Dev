using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using WizardsCode.AnimationControl;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// This is responsible for controlling the Actor. It does not make decisions (see `Brain` below) but it does enact those decisions in terms of movement etc.
    /// 
    /// While the Base Actor Controller is fully functional it is very limited in what it can do. In most cases you will use a controller that extends this one, 
    /// such as the `Animator Actor Controller` which will convert movement on the navmesh to animation parameters. 
    /// </summary>
    public class BaseActorController : MonoBehaviour
    {
        public enum States { Idle, Moving, Arriving, Arrived }

        #region InspectorParameters
        // Ground Movement
        [SerializeField, Tooltip("The maximum speed of this character, this will usually be a full sprint."), BoxGroup("Ground Movement")]
        protected float m_MaxSpeed = 8f;
        [SerializeField, Tooltip("If the distance the actor needs to travel to reach their destination is greater than this and the actor can run then they will do so."), BoxGroup("Ground Movement")]
        protected float m_MinRunDistance = 15;
        [SerializeField, Tooltip("If the distance the actor needs to travel to reach their destination is greater than this and the actor can sprint then they will do so."), BoxGroup("Ground Movement")]
        protected float m_MinSprintDistance = 30;
        [SerializeField, Tooltip("The factor used to calculate the top walking speed of the character relative to the maximum speed. The higher this value the faster the character needs to be moving before switching to a run animation."), BoxGroup("Ground Movement")]
        protected float m_WalkSpeedFactor = 0.45f;
        [SerializeField, Tooltip("The factor used to calculate the top running speed of the character relative to the maximum speed. The higher this value the faster the character needs to be moving before switching to a sprint animation."), BoxGroup("Ground Movement")]
        protected float m_RunSpeedFactor = 0.8f;
        [SerializeField, Tooltip("The distance within which the character is considered to be arriving at their destination. This is used to allow callbacks for when the character has nearly completed their movement. This can be useful when the character needs to, for example, turn to sit on a chair just before reaching the final stopping point."), BoxGroup("Ground Movement")]
        protected float m_ArrivingDistance = 1f;

        [Space]
        // Look
        [SerializeField, Tooltip("A transform at the point in space that the actor should look towards. This should be moved at runtime to cause the character to adjust their body position to enable them to look at the target. If this is not set in the inspector a target will be created at runtime. This may not be optimally placed."), BoxGroup("Look")]
        Transform m_LookAtTarget;
        [SerializeField, Tooltip("The \"head\" bone or object, used to look. If this is blank there will be an attempt to automatically find the head upon startup."), BoxGroup("Look")]
        [FormerlySerializedAs("head")] // 5/3/25
        Transform m_HeadBone = null;
        [SerializeField, Tooltip("The speed at which a character will turn their head to look at a target."), BoxGroup("Look")]
        protected float m_LookAtSpeed = 6f;
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
        private float lastStateChangeTime = float.NegativeInfinity;
        private States m_state;
        #endregion

#region Properties
        public string displayName
        {
            get { return brain.DisplayName; }
        }

        public Transform HeadBone
        {
            get { return m_HeadBone; }
        }

        public Brain brain
        {
            get;
            internal set;
        }

        public virtual States state
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

        public float MaxSpeed
        {
            get { return m_MaxSpeed; }
            set { m_MaxSpeed = value; }
        }

        public float ArrivingDistance
        {
            get { return m_ArrivingDistance; }
            set { m_ArrivingDistance = value; }
        }
#endregion

        protected Vector3 m_CurrentLookAtPosition;

        protected float m_WalkSpeed;
        protected float m_RunSpeed;

        Quaternion desiredRotation = default;
        bool isRotating = false;

        AnimationLayerController m_AnimationLayers;

        /// <summary>
        /// Set the LookAtTarget to a specific position in world space.
        /// </summary>
        /// <param name="position"></param>
        [Obsolete("Use LookAtTarget instead. Deprecated in 0.4.0.")]
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

        #region Actions
        /// <summary>
        /// Instruct the character to move to a defined position and, optionally, 
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

        Vector3 m_MoveTarget = Vector3.zero;
        public virtual Vector3 MoveTargetPosition
        {
            get { return m_MoveTarget; }
            set
            {
                m_MoveTarget = value;
            }
        }


        /// <summary>
        /// Stop the actor from moving. Clearing the current path if there is one.
        /// </summary>
        public virtual void StopMoving()
        {
        }

        IEnumerator cueCoroutine;
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

        public virtual void Prompt(ActorCue cue, float duration) {
            if (cue == null) return;

            cue.Duration = duration;

            Prompt(cue);
        }

        protected float m_runSqrMagnitude;
        protected float m_sprintSqrMagnitude;
        private Transform m_InteractionPoint;

        protected virtual bool IsArriving
        {
            get
            {
                // TODO: add a stopping distance parameter and have it sync to namemesh agent in AnimatorActorController
                return Vector3.SqrMagnitude(transform.position - MoveTargetPosition) <= (m_ArrivingDistance * m_ArrivingDistance) / 2;
            }
        }

        protected virtual bool HasArrived
        {
            get
            {
                return Vector3.SqrMagnitude(transform.position - MoveTargetPosition) <= m_ArrivingDistance * m_ArrivingDistance;
            }
        }

        protected virtual void Awake()
        {
            if (m_LookAtTarget == null)
            {
                m_LookAtTarget = new GameObject("Look At Target").transform;
                m_LookAtTarget.SetParent(transform);
                m_LookAtTarget.localPosition = new Vector3(0, 1.6f, 0.5f);
            }
            
            brain = GetComponentInChildren<Brain>();

            // Look IK Setup
            if (!HeadBone)
            {
                Debug.LogWarning("No head transform set on " + gameObject.name + " and one could not be found automatically - LookAt disabled");
            }

            m_InteractionPoint = new GameObject(transform.root.name + " Interaction Point").transform;

            m_LookAtTarget.name = transform.root.name + " Look At Target";
        }

        protected virtual void Update()
        {
            UpdateMovement();

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

        protected virtual void UpdateMovement()
        {
            if (MoveTargetPosition != Vector3.zero)
            {
                Vector3 direction = (MoveTargetPosition - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, MoveTargetPosition);

                float speed = m_MaxSpeed;
                if (distance < m_MinRunDistance)
                {
                    speed *= m_WalkSpeedFactor;
                }
                else if (distance < m_MinSprintDistance)
                {
                    speed *= m_RunSpeedFactor;
                }

                transform.position = Vector3.MoveTowards(transform.position, MoveTargetPosition, speed * Time.deltaTime);

                if (distance <= ArrivingDistance)
                {
                    state = States.Arriving;
                }
                else
                {
                    state = States.Moving;
                }
            }
        }

        /// <summary>
        /// If the actor is already rotating continue that rotation. If the character is trying
        /// to look at something that is outside of the look angle range their head should turn then 
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
                    else if (!HasArrived)
                    {
                        state = States.Moving;
                    }
                    break;
                case States.Moving:
                    if (IsArriving)
                    {
                        state = States.Arriving;
                        if (onArriving != null)
                        {
                            onArriving();
                            onArriving = null;
                        }
                    }
                    
                    hasMoved = true;
                    break;
                case States.Arriving:
                    if (HasArrived)
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
        /// A measure of how noticeable this character is from 0 to 1. 
        /// 0 is as good as invisible, 1 is can't miss them.
        /// How noticeable an actor is depends on what they are doing
        /// at any given time as well as their emotions. For example, 
        /// a fearful character who is resting is less noticeable
        /// than an interested character. Anger will increase noticeability,
        /// but sadness will reduce it. Similarly a character who is attacking
        /// is more noticeable than one who is idle.
        /// </summary>
        public float Noticeability {
            get
            {
                float result = 0;

                EmotionalState emotion = GetComponent<EmotionalState>();
                if (emotion)
                {
                    result = emotion.Noticeability;
                }

                //TODO currently active behaviour should impact noticeability. Add a noticeability factor to behaviours.
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
            if (HeadBone)
            {
                LookAtTarget.transform.position = HeadBone.position + transform.forward;
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
                if (HeadBone != null)
                {
                    Gizmos.DrawLine(HeadBone.position, LookAtTarget.position);
                }
            }
        }

        /// <summary>
        /// Teleport the actor to a location. The actor will inherit the rotation and position of the location.
        /// </summary>
        /// <param name="location">The location to teleport to.</param>
        /// <param name="aiActive">Should the AI brain, if the actor has one, be active after the teleport?</param>
        public virtual void Teleport(Transform location, bool aiActive)
        {
            transform.position = location.position;
            transform.rotation = location.rotation;
            
            if (brain)
            {
                brain.active = aiActive;
            }
            StopMoving();
            ResetLookAt();
        }

        protected virtual void OnValidate() {
            if (m_HeadBone == null)
            {
                foreach (var t in transform.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Head")
                    {
                        m_HeadBone = t;
                        break;
                    }
                }
            }
        }
    }
}