using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.AnimationControl
{
    /// <summary>
    /// The NavMeshConnector converts a `NavMeshAgents` movement to 
    /// parameters for the Animation Controller.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LookAt))]
    public class NavMeshConnector : MonoBehaviour
    {   public enum States { Stationary, Moving, Arriving, Arrived }

        [Header("Character Setup")]
        [SerializeField, Tooltip("The maximum speed of this character, this will usually be a full sprint.")]
        private float m_MaxSpeed = 8f;
        [SerializeField, Tooltip("The factor used to calculate the normal (usually walking) speed of the character relative to the maximum speed. Normally you won't want to change this, but if your character is sliding when walking this can help.")]
        float m_NormalSpeedFactor = 0.4375f;
        [SerializeField, Tooltip("The distance within which the character is considered to be arriving at their destination. This is used to allow callbacks for when the character has nearly completed their movement. This can be useful when the character needs to, for example, turn to sit on a chair just before reaching the final stopping point.")]
        float m_ArrivingDistance = 1f;

        [Header("Animation Parameters")]
        [SerializeField, Tooltip("The normalized speed of the character. This does not take into account the direction of travel.")]
        private string m_SpeedParameterName = "Forward";
        [SerializeField, Tooltip("The normalized turning speed (-1 to 1) of the character (-1 to 1).")]
        private string m_TurnParameterName = "Turn";
        
        [Header("Debug")]
        [SerializeField, Tooltip("Enable click to move when running in the editor (ignored in the application).")]
        private bool m_EnableClickToMove = true;

        float m_NormalSpeed; // typically this will be the walking speed
        States state;


        [HideInInspector]
        public Action onStationary;
        bool hasMoved = false;
        [HideInInspector]
        public Action onArriving;
        [HideInInspector]
        public Action onArrived;

        RaycastHit hitInfo = new RaycastHit();
        Animator m_Animator;
        NavMeshAgent m_Agent;
        private LookAt lookAt;

        void Start()
        {
            m_NormalSpeed = m_MaxSpeed * m_NormalSpeedFactor;
            m_NormalSpeed += 0.01f; // increase the walking speed a little to avoid rounding errors affecting comparisons

            m_Animator = GetComponent<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
            lookAt = GetComponent<LookAt>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (m_EnableClickToMove && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
                    m_Agent.destination = hitInfo.point;
            }
#endif

            SetForwardAndTurnParameters();

            switch (state)
            {
                case States.Stationary:
                    if (hasMoved && onStationary != null)
                    {
                        onStationary();
                        onStationary = null;
                        hasMoved = false;
                    }
                    break;
                case States.Moving:
                    if (m_Agent.remainingDistance > m_Agent.stoppingDistance && m_Agent.remainingDistance <= m_ArrivingDistance)
                    {
                        state = States.Arriving;
                    }
                    hasMoved = true;
                    break;
                case States.Arriving:
                    if (onArriving != null)
                    {
                        onArriving();
                        onArriving = null;
                    }
                    if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
                    {
                        state = States.Arrived;
                    }
                    break;
                case States.Arrived:
                    if (onArrived != null)
                    {
                        onArrived();
                        onArrived = null;
                    }
                    state = States.Stationary;
                    break;
                default:
                    break;
            }

            lookAt.LookAtPosition(m_Agent.destination);
        }

        private void SetForwardAndTurnParameters()
        {
            float magVelocity = m_Agent.velocity.magnitude;
            float animSpeed = 1;
            float speedParam = 0;
            if (!Mathf.Approximately(magVelocity, 0))
            {
                if (magVelocity <= m_NormalSpeed)
                {
                    speedParam = magVelocity / (m_NormalSpeed + m_MaxSpeed);
                    animSpeed = magVelocity / m_NormalSpeed;
                }
                else
                {
                    speedParam = magVelocity / m_MaxSpeed;
                    animSpeed = speedParam;
                }
            }

            Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
            float turn = s.x;

            m_Animator.SetFloat(m_SpeedParameterName, speedParam);
            m_Animator.speed = Math.Abs(animSpeed);
            m_Animator.SetFloat(m_TurnParameterName, turn);

            if (speedParam > 0.01 || turn > 0.01)
            {
                state = States.Moving;
            }
        }
    }
}
