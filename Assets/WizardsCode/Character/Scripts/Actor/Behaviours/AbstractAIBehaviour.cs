using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;
using static WizardsCode.Character.StateSO;
using System.Text;

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
        [SerializeField, Tooltip("The set of character stats and the influence to apply to them when a character chooses this behaviour AND the behaviour does not require an interactable (influences come from the interactable if one is requried).")]
        internal StatInfluence[] m_CharacterInfluences;

        internal Brain brain;
        internal ActorController controller;
        private bool m_IsExecuting = false;
        private List<Interactable> cachedAvailableInteractables = new List<Interactable>();
        private Vector3 positionAtLastInteractableCheck = Vector3.zero;
        private List<Interactable> nearbyInteractablesCache = new List<Interactable>();
        private float m_NextRetryTime;

        internal Interactable CurrentInteractableTarget = default;

        internal StringBuilder reasoning = new StringBuilder();


        internal MemoryController Memory { get { return brain.Memory; } }

        public string DisplayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        public RequiredStat[] RequiredStats
        {
            get { return m_RequiredStats; }
            set { m_RequiredStats = value; }
        }

        public DesiredStatImpact[] DesiredStateImpacts {
            get {return m_DesiredStateImpacts;}
        }

        public bool RequiresInteractable
        {
            get { return m_RequiresInteractable; }
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

                reasoning.Clear();

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
                    reasoning.AppendLine("They decide not to because they don't have the necessary stats.");

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

        /// <summary>
        /// Check if the character has all the necessary stats to execute this behaviour.
        /// </summary>
        /// <param name="log">A string that will contain a textual description, in Ink format, describing why the character believes they can or cannot enable this behaviour.</param>
        /// <returns>True if the behaviour can be enabled, otherwise false.</returns>
        private bool CheckCharacteHasRequiredStats()
        {
            if (m_RequiredStats.Length == 0)
            {
                reasoning.Append(brain.DisplayName);
                reasoning.Append(" has no required stats for ");
                reasoning.Append(DisplayName);
                reasoning.AppendLine(".");
                return true;
            }

            bool allRequirementsMet = true;
            bool thisRequirementMet = false;
            for (int i = 0; i < m_RequiredStats.Length; i++)
            {
                reasoning.Append(m_RequiredStats[i].statTemplate.DisplayName);

                switch (m_RequiredStats[i].objective)
                {
                    case Objective.LessThan:
                        thisRequirementMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue < m_RequiredStats[i].NormalizedValue;
                        if (thisRequirementMet) {
                            reasoning.Append(" is good since it is less than ");
                        } 
                        else
                        {
                            reasoning.Append(" is no good since it is not less than ");
                        }
                        break;
                    case Objective.Approximately:
                        thisRequirementMet = Mathf.Approximately(brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue, m_RequiredStats[i].NormalizedValue);
                        if (thisRequirementMet)
                        {
                            reasoning.Append(" is good since it is approximately equal to ");
                        }
                        else
                        {
                            reasoning.Append(" is no good since it is not approximately equal to ");
                        }
                        break;
                    case Objective.GreaterThan:
                        thisRequirementMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).NormalizedValue > m_RequiredStats[i].NormalizedValue;
                        if (thisRequirementMet)
                        {
                            reasoning.Append(" is good since it is greater than ");
                        }
                        else
                        {
                            reasoning.Append(" is no good since it is not greater than ");
                        }
                        break;
                    default:
                        Debug.LogError("Don't know how to handle an Objective of " + m_RequiredStats[i].objective);
                        thisRequirementMet = false;
                        reasoning.Append("Error in processing " + m_RequiredStats[i] + " unrecognized objective: " + m_RequiredStats[i].objective);
                        break;
                }
                reasoning.AppendLine(m_RequiredStats[i].Value.ToString());
                allRequirementsMet &= thisRequirementMet;
            }

            return allRequirementsMet;
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
        internal virtual void StartBehaviour(Interactable interactable)
        {
            EndTime = Time.timeSinceLevelLoad + interactable.Duration;
        }

        /// <summary>
        /// Start this behaviour without an interactable. If this behaviour requires
        /// an interactable and somehow this method gets called it will return with no
        /// actions (after logging a warning).
        /// </summary>
        internal virtual void StartBehaviour()
        {
            if (m_RequiresInteractable)
            {
                Debug.LogWarning(DisplayName + " was started by " + brain.DisplayName + " without an interactable, yet one is required.");
                return;
            }

            EndTime = Time.timeSinceLevelLoad + m_AbortDuration;

            for (int i = 0; i < m_CharacterInfluences.Length; i++)
            {
                StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                influencer.InteractionName = m_CharacterInfluences[i].statTemplate.name + " influencer from " + DisplayName;
                influencer.Trigger = null;
                influencer.stat = m_CharacterInfluences[i].statTemplate;
                influencer.maxChange = m_CharacterInfluences[i].maxChange;
                influencer.duration = m_AbortDuration;
                influencer.cooldown = 0;

                brain.TryAddInfluencer(influencer);
            }
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
                currentInteractables = hitColliders[i].GetComponentsInParent<Interactable>();
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
            if (!HasDesiredImpact(interactable))
            {
                return false;
            }

            reasoning.Append(interactable.name);
            reasoning.Append(" is close by, maybe it's a good place to ");
            reasoning.AppendLine(interactable.InteractionName);

            if (!interactable.HasSpaceFor(brain))
            {
                reasoning.AppendLine("Looks like it is full.");
                return false;
            }

            if (interactable.IsOnCooldownFor(brain))
            {
                reasoning.AppendLine("I Went there recently, let's try somewhere different.");
                return false;
            }

            if (!interactable.HasRequiredObjectStats())
            {
                reasoning.AppendLine("Looks like they don't have what I need.");
                return false;
            }

            reasoning.AppendLine("Looks like they have space as well as what I need.");
            return true;
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

        internal virtual void Finish()
        {
            IsExecuting = false;
            EndTime = 0;
        }

        public override string ToString()
        {
            return DisplayName;
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
        // These values are hidden in the insepctor because there is a custom editor
        // But at the time of writing it is incomplete.
        [SerializeField, Tooltip("The stat we require a value for.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The object for this stats value, for example, greater than, less than or approximatly equal to.")]
        public Objective objective;
        [SerializeField, Tooltip("The value required for this stat (used in conjunction with the objective). Note that only normalized value and value are paired, so changing one will change the other as well.")]
        float m_Value;
        [SerializeField, Tooltip("The normalized value required for this stat  (used in conjunction with the objective). Note that only normalized value and value are paired, so changing one will change the other as well."), Range(0f,1f)]
        float m_NormalizedValue;

        public float Value
        {
            get { return m_Value; }
            set { 
                m_Value = value;
                if (statTemplate != null)
                {
                    m_NormalizedValue = (value - statTemplate.MinValue) / (statTemplate.MaxValue - statTemplate.MinValue);
                } else
                {
                    m_NormalizedValue = 0;
                }
            }
        }

        public float NormalizedValue
        {
            get { return m_NormalizedValue; }
            set
            {
                m_NormalizedValue = value;
                if (statTemplate != null)
                {
                    m_Value = value * (statTemplate.MaxValue - statTemplate.MinValue);
                } else
                {
                    m_Value = 0;
                }
            }
        }
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