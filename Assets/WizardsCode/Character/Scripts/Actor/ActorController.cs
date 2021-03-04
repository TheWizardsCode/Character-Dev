using UnityEngine;
using UnityEngine.AI;
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

        private Animator m_Animator;
        private NavMeshAgent m_Agent;
        private Brain m_Brain;

        internal Animator Animator
        {
            get { return m_Animator; }
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
        /// Prompt the actor to enact a cue. A cue describes
        /// a position and actions that an actor should take.
        /// </summary>
        /// <param name="cue">The cue to enact.</param>
        internal void Prompt(ActorCue cue)
        {
            System.Collections.IEnumerator coroutine = cue.Prompt(this);
            if (coroutine != null)
            {
                StartCoroutine(coroutine);
            }
        }

        protected virtual void Start()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_Brain = GetComponent<Brain>();
            TargetPosition = transform.position;
        }

        public void PlayEmote(string name)
        {
            Animator animator = GetComponent<Animator>();

            animator.Play(name);
        }

        protected virtual void Update()
        {
            if (m_Animator != null && m_Agent != null)
            {
                float speed = m_Agent.desiredVelocity.magnitude / m_RunningSpeed;
                if (speed < 0.1 || speed > 0.1)
                {
                    m_Animator.SetFloat(SpeedParameterName, speed);
                }
                else
                {
                    m_Animator.SetFloat(SpeedParameterName, 0);
                }

                Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
                float turn = s.x;
                m_Animator.SetFloat(TurnParameterName, turn);
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
