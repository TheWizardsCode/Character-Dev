using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using UnityEngine.AI;

namespace WizardsCode.Character.AI
{
    public class CombatCoverObject : Interactable
    {
        /// <summary>
        /// Get an appropriate cover position and rotation that an Actor should take in order to
        /// take cover behind this object.
        /// </summary>
        public override Transform GetInteractionPointFor(BaseActorController actor)
        {
            if (actor == null)
            {
                return base.GetInteractionPointFor(null);
            }
            else
            {
                Bounds bounds = GetComponent<Renderer>().bounds;
                Vector3 position = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
                Vector3 targetPosition = actor.brain.GetTargetActor().transform.position;

                NavMeshHit closestEdge = new NavMeshHit();
                if (TestCoverPosition(targetPosition, position, out closestEdge))
                {
                    actor.LookAt(position);
                    return actor.LookAtTarget;
                }

                position = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
                if (TestCoverPosition(targetPosition, position, out closestEdge))
                {
                    actor.LookAt(position);
                    return actor.LookAtTarget;
                }

                position = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
                if (TestCoverPosition(targetPosition, position, out closestEdge))
                {
                    actor.LookAt(position);
                    return actor.LookAtTarget;
                }

                position = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
                if (TestCoverPosition(targetPosition, position, out closestEdge))
                {
                    actor.LookAt(position);
                    return actor.LookAtTarget;
                }
                return null;
            }   
        }

        /// <summary>
        /// Test if this is a suitable cover position.
        /// </summary>
        /// <param name="targetPosition">The position of the target we are trying to take cover from.</param>
        /// <param name="testPosition">The candidate position we are testing.</param>
        /// <param name="closestEdge">If this is a suitable cover position for the AI this will be the closest NavMeshEdge to that position.</param>
        /// <returns>True if this is a suitable cover position, otherwise false;</returns>
        private bool TestCoverPosition(Vector3 targetPosition, Vector3 testPosition, out NavMeshHit closestEdge)
        {
            if (!NavMesh.FindClosestEdge(testPosition, out closestEdge, NavMesh.AllAreas))
            {
                return false;
            }

            float suitability = Vector3.Dot(closestEdge.normal, (targetPosition - closestEdge.position).normalized);
            return suitability < -0.5f;
        }
    }
}
