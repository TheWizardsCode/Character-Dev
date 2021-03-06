using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WizardsCode.Animation
{
    /// <summary>
    /// Look At a point of interest using the Animator LookAt API.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class LookAt : MonoBehaviour
    {
        [SerializeField, Tooltip("The head of the charcter.")]
        Transform m_Head = null;
        [SerializeField, Tooltip("How quickly should the charactor look towards a new target?")]
        float m_LookAtHeatTime = 0.2f;
        [SerializeField, Tooltip("How quickly should the charactor look away from the old target?")]
        float m_LookAtCoolTime = 0.2f;
        [SerializeField, Tooltip("Should the character look to the designated position?")]
        bool m_IsLooking = true;

        private Vector3 m_LookAtTargetPosition;
        private Vector3 m_CurrentLookAtPosition;
        private Animator m_Animator;
        private float m_LookAtWeight = 0.0f;

        public void LookAtTarget(Transform target)
        {
            m_LookAtTargetPosition = target.position;
        }

        public void LookAtPosition(Vector3 position)
        {
            m_LookAtTargetPosition = position;
        }

        void Start()
        {
            if (!m_Head)
            {
                Debug.LogWarning("No head transform - LookAt disabled");
                enabled = false;
                return;
            }
            m_Animator = GetComponent<Animator>();
            m_LookAtTargetPosition = m_Head.position + transform.forward;
            m_CurrentLookAtPosition = m_LookAtTargetPosition;
        }

        void OnAnimatorIK()
        {
            m_LookAtTargetPosition.y = m_Head.position.y;
            float lookAtTargetWeight = m_IsLooking ? 1.0f : 0.0f;

            Vector3 curDir = m_CurrentLookAtPosition - m_Head.position;
            Vector3 futDir = m_LookAtTargetPosition - m_Head.position;

            curDir = Vector3.RotateTowards(curDir, futDir, 6.28f * Time.deltaTime, float.PositiveInfinity);
            m_CurrentLookAtPosition = m_Head.position + curDir;

            float blendTime = lookAtTargetWeight > m_LookAtWeight ? m_LookAtHeatTime : m_LookAtCoolTime;
            m_LookAtWeight = Mathf.MoveTowards(m_LookAtWeight, lookAtTargetWeight, Time.deltaTime / blendTime);
            m_Animator.SetLookAtWeight(m_LookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
            m_Animator.SetLookAtPosition(m_CurrentLookAtPosition);
        }
    }
}