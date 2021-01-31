using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// A character actor performs for the camera and takes cues from a director.
    /// Converts NavMesh movement to animation controller parameters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Brain))]
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

        internal Vector3 TargetPosition
        {
            get { return m_Agent.destination; }
            set
            {
                m_Agent.SetDestination(value);
            }
        }

        protected virtual void Start()
        {
            m_Animator = GetComponent<Animator>();
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
                return false;
            }
        }

    }
}
