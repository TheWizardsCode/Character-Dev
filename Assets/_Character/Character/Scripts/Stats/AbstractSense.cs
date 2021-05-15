using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using UnityEngine.Serialization;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// A sense monitors things in the environment. 
    /// 
    /// Implementations of this abstract class will need to provide the senses code in the `OnUpdate` method. The AbstractBase class captures the objects that match the spec within range, these should be filtered in the OnUpdate method.
    /// </summary>
    public abstract class AbstractSense : MonoBehaviour
    {
        [SerializeField, TextArea, Tooltip("Description field for use in the editor")]
        string description;
        [SerializeField, Tooltip("The minimum range over which this sense will work under normal circumstances.")]
        float m_MinRange = 1f;
        [SerializeField, Tooltip("The maximum range over which this sense will work under normal circumstances.")]
        [FormerlySerializedAs("range")]
        float m_MaxRange = 100f;
        [SerializeField, Tooltip("The layermask to use when detecting colliders. Use this to ensure only the right kind of objects are detected.")]
        LayerMask m_LayerMask = 1;
        [SerializeField, Tooltip("A tag that is required on the sensed objects. If null any object will be sensed.")]
        string m_Tag;
        [SerializeField, Tooltip("The maximum number of sensed objects.")]
        int maxSensedColliders = 50;
        [SerializeField, Tooltip("The name of the Component type we require the sensed object to have.")]
        //TODO don't require fully qualified name here
        string m_ComponentTypeNameToSense = "WizardsCode.Stats.StatsTracker";
        [SerializeField, Tooltip("An (optional) influencer to apply to the actor if they sense anything.")]
        StatInfluencerSO m_StatInfluencer;

        internal string logName;
        private Type m_ComponentType;
        private List<Transform> m_SensedObjects = new List<Transform>();
        bool isValid = true;

        internal Type ComponentType
        {
            get { return m_ComponentType; }
        }

        internal List<Transform> SensedThings
        {
            get { return m_SensedObjects; }
        }

        /// <summary>
        /// Test to see if the actor has sensed anything during the last cycle.
        /// </summary>
        internal virtual bool HasSensed
        {
            get { return SensedThings.Count > 0; }
        }

        internal virtual void Awake()
        {
            logName = transform.root.name;

            m_ComponentType = Type.GetType(m_ComponentTypeNameToSense);
            if (m_ComponentType == null)
            {
                Debug.LogWarning(logName + " is a sense that has an invalid target component type of " + m_ComponentType + " Disabling the sense component.");
                this.enabled = false;
            }
        }

        internal void Update()
        {
            //TODO move the overall sense code into the ActorController where it can cache the sensed object list. Implementations of this class can then filter for items they care about.
            Collider[] hitColliders = new Collider[maxSensedColliders];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, m_MaxRange, hitColliders, m_LayerMask);
            m_SensedObjects = new List<Transform>();
            for (int i = numColliders - 1; i >= 0; i--)
            {
                Transform root = hitColliders[i].transform.root;
                if (root == this.transform.root)
                {
                    continue;
                }

                //TODO OPTIMIZATION use sqrmagnitude not distance
                if (string.IsNullOrEmpty(m_Tag) || root.CompareTag(m_Tag))
                {
                    if (Vector3.Distance(this.transform.root.transform.position, root.position) >= m_MinRange && root.GetComponentInChildren(ComponentType))
                    {
                        m_SensedObjects.Add(root);
                    }
                }
            }

            OnUpdate();
        }

        internal virtual void OnUpdate() { }

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
