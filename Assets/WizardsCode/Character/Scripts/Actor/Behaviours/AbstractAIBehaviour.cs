using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        RequiredState[] m_RequiredStates = new RequiredState[0];
        [SerializeField, Tooltip("The duration within which the actor will be prevented from starting another behaviour.")]
        float m_Duration = 5;
        [SerializeField, Tooltip("The range within which the Actor can sense interactables that this behaviour can impact. This does not affect interactables that are recalled from memory.")]
        float awarenessRange = 10;

        internal Brain brain;
        internal ActorController controller;
        internal MemoryController memory;

        public RequiredState[] requiredStates {
            get {return m_RequiredStates;}
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Tests to see if this behaviour is availble to be executed. That is are the necessary preconditions
        /// met.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                // Are requirements met?
                for (int i = 0; i < m_RequiredStates.Length; i++)
                {
                    if (m_RequiredStates[i].invert)
                    {
                        if (m_RequiredStates[i].state.IsSatisfiedFor(brain)) return false;
                    }
                    else
                    {
                        if (!m_RequiredStates[i].state.IsSatisfiedFor(brain)) return false;
                    }
                }

                UpdateAvailbleInteractablesCache();

                return cachedAvailableInteractables.Count != 0;
            }
        }

        private bool m_IsExecuting = false;
        private float m_StartTime;
        private List<Interactable> cachedAvailableInteractables = new List<Interactable>();

        /// <summary>
        /// Is this behaviour the currently executing behaviour?
        /// </summary>
        public bool IsExecuting {
            get { return m_IsExecuting; }
            internal set
            {
                if (value && !m_IsExecuting)
                {
                    m_StartTime = 0;
                }

                m_IsExecuting = value;
            }
        }

        /// <summary>
        /// Called when the behaviour is started, from the `Start` method of the underlying
        /// `MonoBehaviour`.
        /// </summary>
        protected virtual void Init()
        {
            brain = GetComponentInParent<Brain>();
            if (brain == null)
            {
                if (m_RequiredStates.Length > 0)
                {
                    Debug.LogError(gameObject.name + " has required states defined but has no StatsController against which to check these states.");
                }
            }
            controller = GetComponentInParent<ActorController>();
            memory = GetComponentInParent<MemoryController>();
        }

        public void Update()
        {
            if (!IsExecuting) return;
            OnUpdate();
        }

        /// <summary>
        /// Calculates the current weight for this behaviour between 0 (don't execute)
        /// and 1 (really want to execute). By default this is directly proportional to,
        /// the number of unsatisfied desired states in the brain that this behaviour 
        /// impacts.
        /// 
        /// If there are no unsatisfiedDesiredStates then the weight will be 0.
        /// 
        /// This should nearly always be overridden in specific behaviour implementations.
        /// </summary>
        public virtual float Weight(Brain brain)
        {
            if (brain.UnsatisfiedDesiredStates.Length == 0) return 0f;

            float weight = 0;
            for (int i = 0; i < brain.UnsatisfiedDesiredStates.Length; i++)
            {
                for (int idx = 0; idx < requiredStates.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].name == requiredStates[idx].state.name) weight++;
                }
            }
            return weight / brain.UnsatisfiedDesiredStates.Length;
        }

        internal void UpdateCacheWithNearbyInteractables(StatSO statTemplate)
        {
            //TODO Cache the interactables near the current location and only update if moved

            cachedAvailableInteractables = new List<Interactable>();

            //TODO Put interactables on a layer to make the physics operation fsater
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRange);
            Interactable currentInteractable;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractable = hitColliders[i].GetComponent<Interactable>();
                //TODO need to only get interactables that affect the state in the way desired (e.g. increase or decrease)
                if (currentInteractable != null && currentInteractable.Influences(statTemplate))
                {
                    cachedAvailableInteractables.Add(currentInteractable);
                }
            }
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. By default this will look
        /// for interactables nearby that will satisfy the needs of this behaviour.
        /// </summary>
        protected virtual void OnUpdate()
        {
            if (m_StartTime == 0)
            {
                m_StartTime = Time.timeSinceLevelLoad;
                
                //TODO select the optimal place to eat (distance and amount of influence)
                int idx = Random.Range(0, cachedAvailableInteractables.Count);
                brain.TargetInteractable = cachedAvailableInteractables[idx];
            }
            else
            {
                if (m_StartTime + m_Duration <= Time.timeSinceLevelLoad)
                {
                    Finish();
                }
            }
        }

        private void UpdateAvailbleInteractablesCache()
        {
            //TODO need to get interactables for all states (currently on getting for the first state)
            if (requiredStates.Length > 0)
            {
                UpdateCacheWithNearbyInteractables(requiredStates[0].state.statTemplate);

                if (memory != null)
                {
                    MemorySO[] memories = memory.GetMemoriesInfluencingStat(requiredStates[0].state.statTemplate);
                    Interactable interactable;
                    for (int i = 0; i < memories.Length; i++)
                    {
                        if (memories[i].isGood)
                        {
                            interactable = memories[i].about.GetComponentInChildren<Interactable>();
                            if (interactable != null) cachedAvailableInteractables.Add(interactable);
                        }
                    }
                }
            }
        }

        private void Finish()
        {
            IsExecuting = false;
            m_StartTime = 0;
        }
    }
    
    [Serializable]
    public struct RequiredState
    {
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        public StateSO state;
        [SerializeField, Tooltip("If set to true the state will be required to be inactive.")]
        public bool invert;
    } 
}