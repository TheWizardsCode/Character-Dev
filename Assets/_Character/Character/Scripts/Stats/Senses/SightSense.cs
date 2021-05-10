using UnityEngine;
using System;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Checks to see if the actor can see any objects of the desired type.
    /// </summary>
    public class SightSense : AbstractSense
    {
        Transform m_closestSensedObject;

        internal override void OnUpdate()
        {
            //TODO OPTIMIZATION don't just find the closest, check the actor can actually see the actor (should probably be done in a `bool isSensed()` that checks they are seen when first detected rather than iterate over them a second time here.
            m_closestSensedObject = null;
            float minSqrMag = float.PositiveInfinity;
            float currentSqrMag;
            for (int i = SensedThings.Count - 1; i >= 0; i--)
            {
                currentSqrMag = Vector3.SqrMagnitude(SensedThings[i].position - transform.position);
                if (currentSqrMag < minSqrMag)
                {
                    minSqrMag = currentSqrMag;
                    m_closestSensedObject = SensedThings[i];
                }
            }
        }
    }
}
