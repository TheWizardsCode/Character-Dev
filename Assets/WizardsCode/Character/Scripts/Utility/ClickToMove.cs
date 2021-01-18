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
        NavMeshAgent m_Agent;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    m_Agent.SetDestination(hitInfo.point);
                }
            }
        }
    }
}
