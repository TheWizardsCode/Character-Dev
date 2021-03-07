using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.Utility
{
    /// <summary>
    /// Click hte left mouse button on the NavMesh to move the NavMeshAgent this scrpt is attached to.
    /// </summary>
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField, Tooltip("The parameter name to make the animator enter crouch mode.")]
        string m_CrouchParamName = "Crouch";

        NavMeshAgent m_Agent;
        Animator m_Animator;
        int m_CrouchHash;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_CrouchHash = Animator.StringToHash(m_CrouchParamName);
        }

        private void Update()
        {
            // LMB
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    m_Agent.SetDestination(hitInfo.point);
                }
            }

            // Crouch
            if (Input.GetKeyDown(KeyCode.C))
            {
                m_Animator.SetBool(m_CrouchHash, !m_Animator.GetBool(m_CrouchHash));
            }
        }
    }
}
