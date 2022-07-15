using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Character;
using WizardsCode.Stats;

namespace WizardsCode.BackgroundAI
{
    /// <summary>
    /// Click the left mouse button on the NavMesh to move the NavMeshAgent this scrpt is attached to.
    /// </summary>
    public class ClickToMoveBehaviour : AbstractAIBehaviour
    {

        NavMeshAgent m_Agent;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        protected override void OnUpdateState()
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
