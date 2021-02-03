using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed AI Behaviour";
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash.")]
        RequiredStat[] m_RequiredStats = default;
        //TODO These are not required states, they are affected states and I think they will always be inverted
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        DesiredStateImpact[] m_AffectedStates = new DesiredStateImpact[0];
        [SerializeField, Tooltip("Does this behaviour require an interactable to be active?")]
        bool m_RequiresInteractable = true;
        [SerializeField, Tooltip("The range within which the Actor can sense interactables that this behaviour can impact. This does not affect interactables that are recalled from memory.")]
        float awarenessRange = 10;
        
        internal Brain brain;
        internal ActorController controller;
        
        internal MemoryController Memory { get { return brain.Memory; } }

        [Obsolete("We should probably pull this data from the interactable.")]
        public DesiredStateImpact[] AffectedStates {
            get {return m_AffectedStates;}
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
                bool requirementsMet = false;
                for (int i = 0; i < m_RequiredStats.Length; i++)
                {
                    switch (m_RequiredStats[i].objective)
                    {
                        case Objective.LessThan:
                            requirementsMet = brain.GetOrCreateStat(m_RequiredStats[i].stat).NormalizedValue < m_RequiredStats[i].normalizedValue;
                            break;
                        case Objective.Approximately:
                            requirementsMet = Mathf.Approximately(brain.GetOrCreateStat(m_RequiredStats[i].stat).NormalizedValue, m_RequiredStats[i].normalizedValue);
                            break;
                        case Objective.GreaterThan:
                            requirementsMet =  brain.GetOrCreateStat(m_RequiredStats[i].stat).NormalizedValue > m_RequiredStats[i].normalizedValue;
                            break;
                        default:
                            Debug.LogError("Don't know how to handle an Objective of " + m_RequiredStats[i].objective);
                            break;
                    }
                }

                UpdateAvailbleInteractablesCache();

                return (m_RequiredStats.Length == 0 || requirementsMet) && (!m_RequiresInteractable || cachedAvailableInteractables.Count > 0);
            }
        }

        private bool m_IsExecuting = false;
        private float m_EndTime;
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
                    m_EndTime = 0;
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
                if (m_AffectedStates.Length > 0)
                {
                    Debug.LogError(gameObject.name + " has required states defined but has no StatsController against which to check these states.");
                }
            }
            controller = GetComponentInParent<ActorController>();
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
                for (int idx = 0; idx < AffectedStates.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].name == AffectedStates[idx].state.name) weight++;
                }
            }
            return weight / brain.UnsatisfiedDesiredStates.Length;
        }

        internal void UpdateCacheWithNearbyInteractables(StatSO statTemplate)
        {
            //TODO Cache the interactables near the current location and only update if moved
            cachedAvailableInteractables = new List<Interactable>();

            //TODO Put interactables on a layer to make the physics operation faster
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRange);
            Interactable currentInteractable;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractable = hitColliders[i].GetComponent<Interactable>();
                //TODO need to only get interactables that affect the state in the way desired (e.g. increase or decrease)
                if (currentInteractable != null 
                    && currentInteractable.Influences(statTemplate)
                    && !currentInteractable.IsOnCooldownFor(brain))
                {
                    cachedAvailableInteractables.Add(currentInteractable);
                }
            }
        }


        public void Update()
        {
            if (!IsExecuting) return;
            
            if (m_EndTime == 0)
            {
                if (cachedAvailableInteractables.Count == 0)
                {
                    brain.TargetInteractable = null;
                }
                else
                {
                    //TODO select the optimal interactible based on distance and amount of influence
                    int idx = Random.Range(0, cachedAvailableInteractables.Count);
                    brain.TargetInteractable = cachedAvailableInteractables[idx];
                    m_EndTime = Time.timeSinceLevelLoad + brain.TargetInteractable.Duration;
                }
            }
            OnUpdate();
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. By default this will look
        /// for interactables nearby that will satisfy the needs of this behaviour.
        /// </summary>
        protected virtual void OnUpdate()
        {
            if (m_EndTime < Time.timeSinceLevelLoad)
            {
                Finish();
            }
        }

        private void UpdateAvailbleInteractablesCache()
        {
            for (int i = 0; i < m_AffectedStates.Length; i++)
            {
                /*
                switch (m_AffectedStates[i].objective)
                {
                    case Objective.LessThan:
                        return m_AffectedStates[i].state

                        case Objective.Approximately:
                        break;

                    case Objective.GreaterThan:
                        break;

                    default:
                        Debug.LogError("Don't know how to handle an Objective of " + m_AffectedStates[i].objective);
                        break;
                }
                */
            }
            //TODO need to get interactables for all states (currently on getting for the first state)
            if (AffectedStates.Length > 0)
            {
                UpdateCacheWithNearbyInteractables(AffectedStates[0].state.statTemplate);

                if (Memory != null)
                {
                    MemorySO[] memories = Memory.GetMemoriesInfluencingStat(AffectedStates[0].state.statTemplate);
                    Interactable interactable;
                    for (int i = 0; i < memories.Length; i++)
                    {
                        //TODO if memory is of an already cached interactable we can skip
                        if (memories[i].isGood)
                        {
                            interactable = memories[i].about.GetComponentInChildren<Interactable>();
                            if (interactable != null
                                && !interactable.IsOnCooldownFor(brain))
                            {
                                cachedAvailableInteractables.Add(interactable);
                            }
                        }
                    }
                }
            }
        }

        internal void Finish()
        {
            IsExecuting = false;
            m_EndTime = 0;
        }

        public override string ToString()
        {
            return m_DisplayName;
        }
    }
    
    [Serializable]
    public struct DesiredStateImpact
    {
        [SerializeField, Tooltip("The state we want this behaviour to impact.")]
        public StateSO state;
        [SerializeField, Tooltip("The type of change we desire, usually increase or decrease.")]
        public Objective objective;
    }

    [Serializable]
    public struct RequiredStat
    {
        [SerializeField, Tooltip("The stat we require a value for.")]
        public StatSO stat;
        [SerializeField, Tooltip("The object for this stats value, for example, greater than, less than or approximatly equal to.")]
        public Objective objective;
        [SerializeField, Tooltip("The normalized value required for this stat. "), Range(0f,1f)]
        public float normalizedValue;
    }
}