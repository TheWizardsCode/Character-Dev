using UnityEngine;
using System;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Checks to see if the actor can see any objects of the desired type.
    /// </summary>
    public class SightSense : AbstractSense
    {
        [SerializeField, Tooltip("The name of the Component type we require the sensed object to have.")]        
        //TODO don't require fully qualified name here
        string m_ComponentTypeNameToSense = "WizardsCode.Character.ActorController";

        bool isValid = true;
        private Type m_ComponentType;
        float m_DistanceToClosestSensedObject;
        Transform m_closestSensedObject;

        internal Type ComponentType
        {
            get { return m_ComponentType; }
        }

        internal override bool HasSensed
        {
            get { return m_closestSensedObject != null; }
        }

        internal override void Awake()
        {
            base.Awake();
            m_ComponentType = Type.GetType(m_ComponentTypeNameToSense);
            if (m_ComponentType == null)
            {
                Debug.LogWarning(logName + " is a sense that has an invalid target component type of " + m_ComponentType + " Disabling the sense component.");
                this.enabled = false;
            }
        }

        internal override void OnUpdate()
        {
            m_closestSensedObject = null;
            m_DistanceToClosestSensedObject = float.PositiveInfinity;
            float minSqrMag = float.PositiveInfinity;
            float currentSqrMag;
            for (int i = SensedObjects.Count - 1; i >= 0; i--)
            {
                if (SensedObjects[i].GetComponent(ComponentType))
                {
                    currentSqrMag = Vector3.SqrMagnitude(SensedObjects[i].position - transform.position);
                    if (currentSqrMag < minSqrMag)
                    {
                        minSqrMag = currentSqrMag;
                        m_closestSensedObject = SensedObjects[i];
                    }
                }
            }
        }
        private void OnValidate()
        {
            Type componentType = Type.GetType(m_ComponentTypeNameToSense);
            if (componentType == null && isValid)
            {
                isValid = false;
                //TODO handle this error in the editor, e.g. show an error box
                Debug.LogWarning(name + " has a sense SO attempting to sense an unkown object type of " + componentType);
            }
            else if (!isValid)
            {
                isValid = true;
                //TODO when the error is reported in the editor no need for this log
                Debug.Log(name + " is now valid.");
            }
        }
    }
}
