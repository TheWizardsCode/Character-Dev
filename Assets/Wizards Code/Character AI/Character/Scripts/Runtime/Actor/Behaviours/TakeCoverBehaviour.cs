using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

namespace WizardsCode.Character.AI
{
    public class TakeCoverBehaviour : GenericInteractionBehaviour
    {
        [Header("Cover Config")]
        [SerializeField, Tooltip("The senses that will detect enemies that we must take cover from.")]
        AbstractSense enemySenses;

        Transform coverPosition;
        private DistanceComparer comparer;

        public override bool IsAvailable {
            get
            {
                return enemySenses.HasSensed && base.IsAvailable;
            }
        }

        internal override void StartBehaviour()
        {
            Brain.SetTarget(enemySenses.sensedThings[0]);
            Vector3 targetPosition = Brain.GetTarget().position;

            Bounds bounds = CurrentInteractableTarget.GetComponentInChildren<Renderer>().bounds;

            Vector3 position = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);

            NavMeshHit closestEdge = new NavMeshHit();
            if (TestCoverPosition(targetPosition, position, out closestEdge))
            {
                Brain.Actor.MoveTo(closestEdge.position);
                return;
            }

            position = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            if (TestCoverPosition(targetPosition, position, out closestEdge))
            {
                Brain.Actor.MoveTo(closestEdge.position);
                return;
            }

            position = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            if (TestCoverPosition(targetPosition, position, out closestEdge))
            {
                Brain.Actor.MoveTo(closestEdge.position);
                return;
            }

            position = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            if (TestCoverPosition(targetPosition, position, out closestEdge))
            {
                Brain.Actor.MoveTo(closestEdge.position);
                return;
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

        public class DistanceComparer : IComparer<Transform>
        {
            private Transform target;

            public DistanceComparer(Transform distanceToTarget)
            {
                target = distanceToTarget;
            }

            public int Compare(Transform a, Transform b)
            {
                return Vector3.SqrMagnitude(a.position - target.position).CompareTo(Vector3.SqrMagnitude(b.position - target.position));
            }
        }
    }
}
