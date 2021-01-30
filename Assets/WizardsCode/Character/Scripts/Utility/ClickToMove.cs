using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Character;
using WizardsCode.Stats;

namespace WizardsCode.Utility
{
    /// <summary>
    /// Click the left mouse button on the NavMesh to move the NavMeshAgent this scrpt is attached to.
    /// </summary>
    public class ClickToMove : AbstractAIBehaviour
    {

        NavMeshAgent m_Agent;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        protected override void OnUpdate()
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
