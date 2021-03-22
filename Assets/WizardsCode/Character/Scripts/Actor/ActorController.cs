using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// A character actor performs for the camera and takes cues from a director.
    /// Converts NavMesh movement to animation controller parameters.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ActorController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the forward speed of the character.")]
        private string SpeedParameterName = "Forward";
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the turn angle of the character.")]
        private string TurnParameterName = "Turn";
        [SerializeField, Tooltip("The speed of this character when at a run. It will usually be going slower than this, and for short periods, can go faster (at a spring).")]
        private float m_RunningSpeed = 8;
        [SerializeField, Tooltip("Set to true to use a ragdoll for death animation.")]
        bool useRagdoll = false;

        [Header("Events")]
        [SerializeField, Tooltip("An event to be fired whenever the actor dies.")]
        UnityEvent OnDeath;

        private Animator m_Animator;
        private NavMeshAgent m_Agent;
        private Brain m_Brain;

        internal Animator Animator
        {
            get
            {
                if (m_Animator == null)
                {
                    Debug.LogWarning("No animator set during start. If this error repeats then it means the character has no animator and the ActorController will not be able to do its job. If it does not repeat it is likely because the animator is being programatically configured on the character and this happens after the ActorController.Start() method is called.");
                    m_Animator = GetComponentInChildren<Animator>();
                }
                return m_Animator;
            }
        }

        internal Vector3 TargetPosition
        {
            get { return m_Agent.destination; }
            set
            {
                m_Agent.SetDestination(value);
            }
        }

        /// <summary>
        /// Stop the actor from moving. Clearing the current path if there is one.
        /// </summary>
        internal void StopMoving()
        {
            m_Agent.ResetPath();
        }

        /// <summary>
        /// Set the actor to be dead. This will stop all motion and brain activity etc.
        /// It will also either enable the ragdoll or play a death animation.
        /// </summary>
        protected virtual void Die()
        {
            m_Agent.isStopped = true;
            if (useRagdoll)
            {
                EnableRagdoll();
            }
            else
            {
                //TODO make the die parameter configurable in the UI.
                m_Animator.SetTrigger("Die");
            }
            if (m_Brain != null)
            {
                m_Brain.enabled = false;
            }
            if (OnDeath != null)
            {
                OnDeath.Invoke();
            }
        }

        public virtual void EnableRagdoll()
        {
            Debug.LogError("ActorController is set to Use Ragdoll, but it is not aware of any ragdoll to use. You should extend the ActorController to provide this functionality in EnableRagdoll.");
        }

        System.Collections.IEnumerator cueCoroutine;
        /// <summary>
        /// Prompt the actor to enact a cue. A cue describes
        /// a position and actions that an actor should take.
        /// </summary>
        /// <param name="cue">The cue to enact.</param>
        public void Prompt(ActorCue cue)
        {
            cueCoroutine = cue.Prompt(this);
            if (cueCoroutine != null)
            {
                StartCoroutine(cueCoroutine);
            }
        }

        protected virtual void Start()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_Brain = GetComponentInChildren<Brain>();
            TargetPosition = transform.position;
        }

        [Obsolete("Use an ActorCue that plays the chosen Emote.")] // v0.0.9
        public void PlayEmote(string name)
        {
            Animator animator = GetComponent<Animator>();
            animator.Play(name);
        }

        protected virtual void Update()
        {
            if (Animator != null && m_Agent != null)
            {
                float speed = m_Agent.desiredVelocity.magnitude / m_RunningSpeed;
                if (speed < 0.1 || speed > 0.1)
                {
                    Animator.SetFloat(SpeedParameterName, speed);
                }
                else
                {
                    Animator.SetFloat(SpeedParameterName, 0);
                }

                Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
                float turn = s.x;
                Animator.SetFloat(TurnParameterName, turn);
            }
        }
        internal bool HasReachedTarget
        {
            get
            {
                if (m_Agent.hasPath && !m_Agent.pathPending)
                {
                    if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
                    {
                        return true;
                    }
                }

                if (!m_Agent.hasPath && !m_Agent.pathPending)
                {
                    return true;
                }

                return false;
            }
        }

    }
}