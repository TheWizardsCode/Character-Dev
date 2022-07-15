using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using UnityEngine.Serialization;
using System.Linq;
using Random = UnityEngine.Random;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// A sense monitors things in the environment. 
    /// 
    /// Implementations of this abstract class will need to provide the senses code in the `OnUpdate` method. The AbstractBase class captures the objects that match the spec within range, these should be filtered in the OnUpdate method.
    /// </summary>
    public abstract class AbstractSense : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed Sense";
        [SerializeField, TextArea, Tooltip("Description field for use in the editor")]
        string description;

        [Header("Base Senses Config")]
        [SerializeField, Tooltip("How frequently should the area be scanned with these senses.")]
        float m_ScanFrequency = 1;
        [SerializeField, Tooltip("The minimum range over which this sense will work under normal circumstances.")]
        float m_MinRange = 0f;
        [SerializeField, Tooltip("The maximum range over which this sense will work under normal circumstances.")]
        [FormerlySerializedAs("range")]
        float m_MaxRange = 100f;
        [SerializeField, Tooltip("The maximum number of sensed objects.")]
        int maxSensedColliders = 50;

        [Header("Filters")]
        [SerializeField, Tooltip("The layermask to use when detecting colliders. Use this to ensure only the right kind of objects are detected.")]
        [FormerlySerializedAs("m_LayerMask")] // changed 6/21
        LayerMask m_DetectionLayerMask = 1;
        [SerializeField, Tooltip("A set of tags that will be accepted as a sensed objects. If null any object will be sensed. If non-null and no tag matches then the object will not be sensed.")]
        string[] m_Tags;
        [SerializeField, Tooltip("Zero or more states that the spotted item needs to satisfy if it is to be considered of interest.")]
        StateSO[] m_RequiredSatisfiedStates;

        [Header("Stats Impact")]
        [SerializeField, Tooltip("An (optional) influencer to apply to the actor if they sense anything.")]
        StatInfluence m_StatInfluencer;

        internal string logName;
        private float timeOfNextScan;
        private List<Transform> m_SensedObjects = new List<Transform>();
        bool isValid = true;
        float minRangeSqr;

        internal List<Transform> sensedThings
        {
            get { return m_SensedObjects; }
        }

        /// <summary>
        /// Test to see if the actor has sensed anything during the last cycle.
        /// </summary>
        internal virtual bool HasSensed
        {
            get { return sensedThings.Count > 0; }
        }

        internal virtual void Awake()
        {
            logName = transform.root.name;
            minRangeSqr = m_MinRange * m_MinRange;
        }

        internal void Update()
        {
            if (Time.timeSinceLevelLoad >= timeOfNextScan)
            {
                timeOfNextScan = Time.timeSinceLevelLoad + m_ScanFrequency;

                List<Transform> previouslySensed = new List<Transform>(m_SensedObjects);
                m_SensedObjects.Clear();

                //OPTIMIZATION move the overall sense code into the ActorController where it can cache the sensed object list. Implementations of this class can then filter for items they care about.
                //OPTIMIZATION have the collider array as a global so we aren't creating it every tick
                Collider[] hitColliders = new Collider[maxSensedColliders];
                int numColliders = Physics.OverlapSphereNonAlloc(transform.position, m_MaxRange, hitColliders, m_DetectionLayerMask);
                for (int i = numColliders - 1; i >= 0; i--)
                {
                    Transform root = hitColliders[i].transform.root;
                    if (IsValidObject(root, hitColliders[i]))
                    {
                        m_SensedObjects.Add(root);
                    }
                }

                OnUpdate();
            }
        }

        internal virtual bool IsValidObject(Transform root, Collider collider)
        {
            if (m_SensedObjects.Contains(root))
            {
                return false;
            }

            if (root == this.transform.root)
            {
                return false;
            }

            float sqrMagnitude = (this.transform.root.transform.position - root.position).sqrMagnitude;
            if (sqrMagnitude < minRangeSqr)
            {
                return false;
            }

            if (m_Tags.Length > 0 && !m_Tags.Any(root.tag.Contains))
            {
                return false;
            }

            if (m_RequiredSatisfiedStates.Length > 0)
            {
                StatsTracker stats = root.GetComponentInChildren<StatsTracker>();
                if (stats != null)
                {
                    bool satisfied = true;
                    for (int y = 0; y < m_RequiredSatisfiedStates.Length; y++)
                    {
                        if (!stats.SatisfiesState(m_RequiredSatisfiedStates[y]))
                        {
                            satisfied = false;
                            break;
                        }
                    }
                    if (!satisfied)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        internal virtual void OnUpdate() { }

        private void OnValidate()
        {
            if (transform.localPosition != Vector3.zero) transform.localPosition = Vector3.zero;
        }

    }
}
