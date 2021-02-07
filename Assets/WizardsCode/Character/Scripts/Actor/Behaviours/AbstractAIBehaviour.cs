using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed AI Behaviour";
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash.")]
        RequiredStat[] m_RequiredStats = default;

        [Header("Interactables")]
        [SerializeField, Tooltip("Does this behaviour require an interactable to be active?")]
        bool m_RequiresInteractable = true;//TODO These are not required states, they are affected states and I think they will always be inverted
        [SerializeField, Tooltip("The impacts we need an interactable to have on states for this behaviour to be enabled by it.")]
        DesiredStatImpact[] m_DesiredStateImpacts = new DesiredStatImpact[0];
        [SerializeField, Tooltip("The range within which the Actor can sense interactables that this behaviour can impact. This does not affect interactables that are recalled from memory.")]
        float awarenessRange = 10;
        
        internal Brain brain;
        internal ActorController controller;
        
        internal MemoryController Memory { get { return brain.Memory; } }

        public DesiredStatImpact[] DesiredStateImpacts {
            get {return m_DesiredStateImpacts;}
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
                            requirementsMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue < m_RequiredStats[i].normalizedValue;
                            break;
                        case Objective.Approximately:
                            requirementsMet = Mathf.Approximately(brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue, m_RequiredStats[i].normalizedValue);
                            break;
                        case Objective.GreaterThan:
                            requirementsMet =  brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue > m_RequiredStats[i].normalizedValue;
                            break;
                        default:
                            Debug.LogError("Don't know how to handle an Objective of " + m_RequiredStats[i].objective);
                            break;
                    }
                }

                UpdateAvailableInteractablesCache();

                return (m_RequiredStats.Length == 0 || requirementsMet) 
                    && (!m_RequiresInteractable || cachedAvailableInteractables.Count > 0);
            }
        }

        private bool m_IsExecuting = false;
        private float m_EndTime;
        private List<Interactable> cachedAvailableInteractables = new List<Interactable>();
        private Vector3 positionAtLastInteractableCheck = Vector3.zero;
        private List<Interactable> nearbyInteractablesCache = new List<Interactable>();

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
                if (DesiredStateImpacts.Length > 0)
                {
                    Debug.LogError(gameObject.name + " has desired states defined but has no StatsController against which to check these states.");
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
                for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].name == DesiredStateImpacts[idx].statTemplate.name) weight++;
                }
            }
            return weight / brain.UnsatisfiedDesiredStates.Length;
        }

        /// <summary>
        /// Scan for nearby interactables that have capacity for an actor.
        /// </summary>
        /// <returns>A list of interactables within range that have space for an actor.</returns>
        internal List<Interactable> GetNearbyInteractables()
        {
            if (positionAtLastInteractableCheck != Vector3.zero
                && Vector3.SqrMagnitude(positionAtLastInteractableCheck - transform.position) <= 1)
            {
                positionAtLastInteractableCheck = transform.position;
                return nearbyInteractablesCache;
            }

            nearbyInteractablesCache.Clear();

            //TODO Put interactables on a layer to make the physics operation faster
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRange);
            Interactable currentInteractable;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractable = hitColliders[i].GetComponent<Interactable>();
                if (currentInteractable != null && currentInteractable.HasSpaceFor(brain))
                {
                    nearbyInteractablesCache.Add(currentInteractable);
                }
            }

            return nearbyInteractablesCache;
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
                    Interactable interactable = null;
                    StatInfluence influence;
                    bool selectInteractable = true;
                    //TODO select the optimal interactible based on distance and amount of influence
                    for (int i = 0; i < cachedAvailableInteractables.Count; i++)
                    {
                        if (!cachedAvailableInteractables[i].HasSpaceFor(brain)) continue;

                        for (int idx = 0; idx < cachedAvailableInteractables[i].CharacterInfluences.Length; idx++)
                        {
                            influence = cachedAvailableInteractables[i].CharacterInfluences[idx];
                            for (int y = 0; y < m_DesiredStateImpacts.Length; y++)
                            {
                                if (m_DesiredStateImpacts[y].statTemplate.name == influence.statTemplate.name)
                                {
                                    switch (m_DesiredStateImpacts[y].objective)
                                    {
                                        case Objective.GreaterThan:
                                            if (influence.maxChange > 0)
                                            {
                                                selectInteractable &= true;
                                            } else
                                            {
                                                selectInteractable = false;
                                            }
                                            break;
                                        case Objective.Approximately:
                                            if (Mathf.Approximately(influence.maxChange, 0))
                                            {
                                                selectInteractable &= true;
                                            }
                                            else
                                            {
                                                selectInteractable = false;
                                            }
                                            break;
                                        case Objective.LessThan:
                                            if (influence.maxChange < 0)
                                            {
                                                selectInteractable &= true;
                                            }
                                            else
                                            {
                                                selectInteractable = false;
                                            }
                                            break;
                                    }
                                    if (!selectInteractable) break;
                                }
                                if (!selectInteractable) break;
                            }
                        }
                        if (selectInteractable)
                        {
                            interactable = cachedAvailableInteractables[i];
                            break;
                        } else
                        {
                            interactable = null;
                        }
                    }

                    brain.TargetInteractable = interactable;
                    if (interactable != null)
                    {
                        m_EndTime = Time.timeSinceLevelLoad + brain.TargetInteractable.Duration;
                    }
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

        /// <summary>
        /// Updates the cache of interractables in the area and from memory that can be used by this
        /// behaviour.
        /// </summary>
        private void UpdateAvailableInteractablesCache()
        {
            cachedAvailableInteractables.Clear();

            //TODO share cached interactables across all behaviours and only update if character has moved more than 1 unity
            List<Interactable> candidateInteractables = GetNearbyInteractables();
            
            // Iterate over them keeping only the ones that satsify all desiredStateImpacts
            for (int i = 0; i < candidateInteractables.Count; i++)
            {
                for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                {
                    // does the interactable have the desired influence on the character?
                    if (!candidateInteractables[i].IsOnCooldownFor(brain) 
                        && candidateInteractables[i].HasInfluenceOn(DesiredStateImpacts[idx]))
                    {
                        // does the interactable have the required stats drive the object influence?
                        if (candidateInteractables[i].HasRequiredObjectStats())
                        {
                            cachedAvailableInteractables.Add(candidateInteractables[i]);
                            break;
                        }
                    }
                }
                
            }

            if (Memory != null)
            {
                //TODO rather than get all memories and then test for DesiredStateImpact add a method to do it in one pass
                MemorySO[] memories = Memory.GetAllMemoriesAboutInteractables(awarenessRange * 5);
                Interactable interactable;
                for (int i = 0; i < memories.Length; i++)
                {
                    interactable = memories[i].about.GetComponentInChildren<Interactable>();

                    //TODO if memory is of an already cached interactable we can skip
                    
                    for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                    {
                        if (!interactable.IsOnCooldownFor(brain)
                            && interactable.HasInfluenceOn(DesiredStateImpacts[idx]))
                        {
                            cachedAvailableInteractables.Add(interactable);
                            break;
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
    public struct DesiredStatImpact
    {
        [SerializeField, Tooltip("The stat we want this behaviour to impact.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The type of change we desire after the behaviour has completed.")]
        public Objective objective;
    }

    [Serializable]
    public struct RequiredStat
    {
        [SerializeField, Tooltip("The stat we require a value for.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The object for this stats value, for example, greater than, less than or approximatly equal to.")]
        public Objective objective;
        [SerializeField, Tooltip("The normalized value required for this stat. "), Range(0f,1f)]
        public float normalizedValue;
    }

    [Serializable]
    public struct StatInfluence
    {
        [SerializeField, Tooltip("The Stat this influencer acts upon.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait, to the limit of the stats allowable value.")]
        public float maxChange;
    }
}