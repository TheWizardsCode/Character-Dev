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
        [SerializeField, Tooltip("The range over which this sense will work under normal circumstances.")]
        float range = 100f;
        [SerializeField, Tooltip("The maximum number of sensed objects.")]
        int maxSensedColliders = 50;

        internal string logName;
        private List<Transform> m_SensedObjects = new List<Transform>();

        internal List<Transform> SensedObjects
        {
            get { return m_SensedObjects; }
        }

        /// <summary>
        /// Test to see if the actor has sensed anything during the last cycle.
        /// </summary>
        internal virtual bool HasSensed
        {
            get { return SensedObjects.Count > 0; }
        }

        internal virtual void Awake()
        {
            logName = transform.root.name;
        }

        internal void Update()
        {
            //TODO move the overall sense code into the ActorController where it can cache the sensed object list. Implementations of this class can then filter for items they care about.
            Collider[] hitColliders = new Collider[maxSensedColliders];
            //TODO provide a layermask for the senses
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, range, hitColliders);
            m_SensedObjects = new List<Transform>();
            for (int i = numColliders - 1; i >= 0; i--)
            {
                Transform root = hitColliders[i].transform.root;
                if (root == this.transform.root)
                {
                    continue;
                }
                m_SensedObjects.Add(root);
            }

            OnUpdate();
        }

        internal virtual void OnUpdate() { }

    }
}
