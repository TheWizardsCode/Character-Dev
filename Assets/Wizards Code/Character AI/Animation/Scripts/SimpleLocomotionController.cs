using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace threeDTBD.Animation
{
    /// <summary>
    /// Converts NavMesh movement to animation controller parameters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SimpleLocomotionController : MonoBehaviour
    {
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the forward speed of the character.")]
        private string SpeedParameterName = "InputMagnitude";
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the turn angle of the character.")]
        private string TurnParameterName = "InputAngle";
        [SerializeField, Tooltip("The speed of this character when at a run. It will usually be going slower than this, and for short periods, can go faster (at a spring).")]
        private float m_RunningSpeed = 8;

        private Animator m_Animator;
        private NavMeshAgent m_Agent;

        protected virtual void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            if (m_Animator != null && m_Agent != null)
            {
                float speed = m_Agent.desiredVelocity.magnitude / m_RunningSpeed;
                if (speed < 0.1 || speed > 0.1) {
                    m_Animator.SetFloat(SpeedParameterName, speed);
                } else
                {
                    m_Animator.SetFloat(SpeedParameterName, 0);
                }

                Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
                float turn = s.x;
                m_Animator.SetFloat(TurnParameterName, turn);
            }
        }
    }
}
