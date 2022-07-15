using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using WizardsCode.Character;

namespace WizardsCode.BackgroundAI
{
    /// <summary>
    /// Click hte left mouse button on the NavMesh to move the NavMeshAgent this scrpt is attached to.
    /// </summary>
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField, Tooltip("The parameter name to make the animator enter crouch mode.")]
        string m_CrouchParamName = "Crouch";
        [SerializeField, Tooltip("The layer mask for the click to move script, only objects in this layer will be valid targets for the move.")]
        LayerMask m_LayerMask;

        BaseActorController m_Actor;
        Animator m_Animator;
        int m_CrouchHash;

        void Awake()
        {
            m_Actor = GetComponent<BaseActorController>();
            m_Animator = GetComponent<Animator>();
            m_CrouchHash = Animator.StringToHash(m_CrouchParamName);
        }

        private void Update()
        {
            // LMB
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 500, m_LayerMask))
                    {
                        m_Actor.MoveTargetPosition = hitInfo.point;
                    }
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
