using UnityEngine;
using System;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Checks to see if the actor can see any objects of the desired type.
    /// </summary>
    public class SightSense : AbstractSense
    {
        [Header("Sight Configuration")]
        [SerializeField, Tooltip("The layermask to use for objects that will block vision. If set to anything other than `Nothing` RayCast(s) will be used to detect if the object is visible.")]
        LayerMask m_ObstructionLayerMask;
        [SerializeField, Tooltip("The place to originate raycasts testing for line of sight.")]
        Transform eyes;

        Transform m_closestSensedObject;

        internal override void OnUpdate()
        {
            //OPTIMIZATION don't just find the closest, check the actor can actually see the actor (should probably be done in a `bool isSensed()` that checks they are seen when first detected rather than iterate over them a second time here.
            m_closestSensedObject = null;
            float minSqrMag = float.PositiveInfinity;
            float currentSqrMag;
            for (int i = sensedThings.Count - 1; i >= 0; i--)
            {
                currentSqrMag = Vector3.SqrMagnitude(sensedThings[i].position - transform.position);
                if (currentSqrMag < minSqrMag)
                {
                    minSqrMag = currentSqrMag;
                    m_closestSensedObject = sensedThings[i];
                }
            }
        }

        internal override bool IsValidObject(Transform root, Collider collider)
        {
            bool isValid = base.IsValidObject(root, collider);
            if (m_ObstructionLayerMask == 0)
            {
                return isValid;
            }

            RaycastHit hitInfo;
            Vector3 direction = (collider.transform.position - eyes.position);
            bool isObstructed = Physics.Raycast(eyes.position, direction.normalized, out hitInfo, direction.magnitude, m_ObstructionLayerMask);
            isValid &= !isObstructed || (hitInfo.collider == collider);

            return isValid;
        }
    }
}
