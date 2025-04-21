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
    public class PlayerInputBehaviour : AbstractAIBehaviour
    {
        protected override void OnUpdateState()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.name.StartsWith("Ground"))
                    {
                        Brain.Actor.MoveTo(hit.point);
                        Brain.Actor.TurnToFace(hit.point);
                    }
                }
            }
        }
    }
}
