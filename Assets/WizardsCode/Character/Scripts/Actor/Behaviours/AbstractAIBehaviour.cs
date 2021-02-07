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
        [SerializeField, Tooltip("How frequentlys, in seconds, this behaviour should be tested for activation."), Range(0.01f,5f)]
        float m_RetryFrequency = 2;
        [SerializeField, Tooltip("Time until execution of this behaviour is aborted. " +
            "This is used as a safeguard in case something prevents the actor from completing " +
            "the actions associated with this behaviour, e.g. if they are unable to reach the chosen interactable.")]
        float m_AbortDuration = 30;
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash.")]
        RequiredStat[] m_RequiredStats = default;

        [Header("Interactables")]
        [SerializeField, Tooltip("Does this behaviour require an interactable to be active?")]
        bool m_RequiresInteractable = true;
        [SerializeField, Tooltip("The impacts we need an interactable to have on states for this behaviour to be enabled by it.")]
        DesiredStatImpact[] m_DesiredStateImpacts = new DesiredStatImpact[0];
        [SerializeField, Tooltip("The range within which the Actor can sense interactables that this behaviour can impact. This does not affect interactables that are recalled from memory.")]
        float awarenessRange = 10;
        
        internal Brain brain;
        internal ActorController controller;
        private bool m_IsExecuting = false;
        private List<Interactable> cachedAvailableInteractables = new List<Interactable>();
        private Vector3 positionAtLastInteractableCheck = Vector3.zero;
        private List<Interactable> nearbyInteractablesCache = new List<Interactable>();
        private float m_NextRetryTime;

        internal Interactable CurrentInteractableTarget = default;


        internal MemoryController Memory { get { return brain.Memory; } }

        public DesiredStatImpact[] DesiredStateImpacts {
            get {return m_DesiredStateImpacts;}
        }

        public float EndTime { 
            get; 
            internal set; 
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
                if (Time.timeSinceLevelLoad < m_NextRetryTime) return false;
                m_NextRetryTime = Time.timeSinceLevelLoad + m_RetryFrequency;

                if (CheckCharacteHasRequiredStats())
                {
                    if (m_RequiresInteractable)
                    {
                        UpdateAvailableInteractablesCache();
                    } else
                    {
                        return true;
                    }
                } else
                {
                    return false;
                }

                // Check there is a valid interactable
                if (cachedAvailableInteractables.Count == 0)
                {
                    CurrentInteractableTarget = null;
                }
                else
                {
                    float sqrMagnitude = float.MaxValue;
                    //TODO select the optimal interactible based on distance and amount of influence
                    for (int interactablesIndex = 0; interactablesIndex < cachedAvailableInteractables.Count; interactablesIndex++)
                    {
                        float mag = Vector3.SqrMagnitude(transform.position - cachedAvailableInteractables[interactablesIndex].transform.position);
                        if (mag < sqrMagnitude)
                        {
                            sqrMagnitude = mag;
                            CurrentInteractableTarget = cachedAvailableInteractables[interactablesIndex];
                        }
                    }
                }

                return CurrentInteractableTarget != null;
            }
        }

        private bool CheckCharacteHasRequiredStats()
        {
            if (m_RequiredStats.Length == 0) return true;

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
                        requirementsMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue > m_RequiredStats[i].normalizedValue;
                        break;
                    default:
                        Debug.LogError("Don't know how to handle an Objective of " + m_RequiredStats[i].objective);
                        break;
                }
            }

            return requirementsMet;
        }

        /// <summary>
        /// Is this behaviour the currently executing behaviour?
        /// </summary>
        public bool IsExecuting {
            get { return m_IsExecuting; }
            internal set
            {
                if (value && !m_IsExecuting)
                {
                    EndTime = Time.timeSinceLevelLoad + m_AbortDuration;
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
                    Debug.LogError(gameObject.name + " has desired states defined but has no StatsTracker against which to check these states.");
                }
            }
            controller = GetComponentInParent<ActorController>();
        }

        /// <summary>
        /// Start an interaction with a given object as part of this behaviour. This is
        /// where animations, sounds, FX and similar should be started.
        /// </summary>
        /// <param name="interactable">The interactable we are working on.</param>
        internal virtual void StartInteraction(Interactable interactable)
        {
            EndTime = Time.timeSinceLevelLoad + interactable.Duration;
        }

        /// <summary>
        /// Calculates the current weight for this behaviour between 0 (don't execute)
        /// and 1 (really want to execute). By default this is directly proportional to,
        /// the number of unsatisfied desired states in the brain that this behaviour 
        /// impacts.
        /// 
        /// If there are no unsatisfiedDesiredStates then the weight will be 0.01
        /// 
        /// This should nearly always be overridden in specific behaviour implementations.
        /// </summary>
        public virtual float Weight(Brain brain)
        {
            float weight = 0.01f;
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
            Interactable[] currentInteractables;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractables = hitColliders[i].GetComponents<Interactable>();
                for (int idx = 0; idx < currentInteractables.Length; idx++)
                {
                    if (currentInteractables[idx].HasSpaceFor(brain))
                    {
                        nearbyInteractablesCache.Add(currentInteractables[idx]);
                    }
                }
            }

            return nearbyInteractablesCache;
        }


        public void Update()
        {
            if (!IsExecuting) return;
            OnUpdate();
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. By default this will look
        /// for interactables nearby that will satisfy the needs of this behaviour.
        /// </summary>
        protected virtual void OnUpdate()
        {
            if (EndTime < Time.timeSinceLevelLoad)
            {
                Finish();
            }
        }

        /// <summary>
        /// Updates the cache of interractables in the area and from memory that can be used by this
        /// behaviour. Only interactables that have the desired influences on the actor are returned.
        /// </summary>
        private void UpdateAvailableInteractablesCache()
        {
            cachedAvailableInteractables.Clear();

            //TODO share cached interactables across all behaviours and only update if character has moved more than 1 unity
            List<Interactable> candidateInteractables = GetNearbyInteractables();

            // Iterate over them keeping only the ones that satsify all desiredStateImpacts
            for (int i = 0; i < candidateInteractables.Count; i++)
            {
                if (IsValidInteractable(candidateInteractables[i]))
                {
                    cachedAvailableInteractables.Add(candidateInteractables[i]);
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

                    if (IsValidInteractable(interactable))
                    {
                        cachedAvailableInteractables.Add(interactable);
                    }
                }
            }
        }

        /// <summary>
        /// Does the interactable offer the desired impact and does it have the required stats 
        /// to deliver the objects influence?
        /// That is, if a behaviour requires 10 cash to be delievered does the interactable have
        /// 10 cash to deliver?
        /// </summary>
        /// <param name="interactable">The interactable to be tested</param>
        /// <returns>True if the interactable can deliver on all desired influences</returns>
        private bool IsValidInteractable(Interactable interactable)
        {
            bool isValid = interactable.HasSpaceFor(brain);
            isValid &= !interactable.IsOnCooldownFor(brain);
            isValid &= HasDesiredImpact(interactable);
            return isValid && interactable.HasRequiredObjectStats();
        }

        /// <summary>
        /// Does the interactable have the desired impact to satisfy this behaviour.
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns></returns>
        private bool HasDesiredImpact(Interactable interactable)
        {
            for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
            {
                if (!interactable.HasInfluenceOn(DesiredStateImpacts[idx]))
                {
                    return false;
                }
            }

            return true;
        }

        internal void Finish()
        {
            IsExecuting = false;
            EndTime = 0;
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