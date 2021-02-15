using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;
using static WizardsCode.Character.StateSO;
using System.Text;
using UnityEngine.Serialization;
using WizardsCode.Character.WorldState;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("A player readable description of the behaviour.")]
        [TextArea(3, 10)]
        string m_Description;
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed AI Behaviour";
        [SerializeField, Tooltip("An actor cue to send to the actor upon the start of this interaction.")]
        ActorCue m_OnStartCue;
        [SerializeField, Tooltip("An actor cue to send to the actor upon the ending of this interaction.")]
        ActorCue m_OnEndCue;
        [SerializeField, Tooltip("How frequently, in seconds, this behaviour should be tested for activation."), Range(0.01f,5f)]
        float m_RetryFrequency = 2;
        [SerializeField, Tooltip("Is this behaviour interuptable. That is if the actor decides something else is more important can this behaviour be finished early.")]
        bool m_isInteruptable = false;
        [SerializeField, Tooltip("Time until execution of this behaviour is aborted. " +
            "This is used as a safeguard in case something prevents the actor from completing " +
            "the actions associated with this behaviour, e.g. if they are unable to reach the chosen interactable.")]
        float m_AbortDuration = 30;
        
        [Header("Conditions")]
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash.")]
        RequiredStat[] m_RequiredStats = default;
        [SerializeField, Tooltip("The set of character stats and the influence to apply to them when a character chooses this behaviour AND the behaviour does not require an interactable (influences come from the interactable if one is requried).")]
        internal StatInfluence[] m_CharacterInfluences;
        [SerializeField, Tooltip("The impacts we need an interactable to have on states for this behaviour to be enabled by it.")]
        DesiredStatImpact[] m_DesiredStateImpacts = new DesiredStatImpact[0];
        [SerializeField, Tooltip("The conitions required in the worldstate for this behaviour to be valid.")]
        WorldStateSO[] m_RequiredWorldState;

        public float AbortDuration
        {
            get { return m_AbortDuration; }
        }

        public bool IsInteruptable
        {
            get { return m_isInteruptable; }
        }

        public DesiredStatImpact[] DesiredStateImpacts
        {
            get { return m_DesiredStateImpacts; }
        }

        internal Brain brain;
        internal ActorController controller;
        private bool m_IsExecuting = false;
        private float m_NextRetryTime;

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

        public float EndTime { 
            get; 
            internal set; 
        }

        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Tests to see if this behaviour is availble to be executed. That is are the necessary preconditions
        /// met.
        /// </summary>
        public virtual bool IsAvailable
        {
            get
            {
                if (Time.timeSinceLevelLoad < m_NextRetryTime) return false;
                m_NextRetryTime = Time.timeSinceLevelLoad + m_RetryFrequency;

                reasoning.Clear();

                if (CheckWorldState() && CheckCharacteHasRequiredStats())
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// If any required world states are defined check they are valid.
        /// If found to be invalid the reasining log will have details.
        /// </summary>
        /// <returns>True if all world states are valid.</returns>
        public bool CheckWorldState()
        {
            for (int i = 0; i < m_RequiredWorldState.Length; i++)
            {
                if (!m_RequiredWorldState[i].IsValid)
                {
                    reasoning.Append(m_RequiredWorldState[i].DisplayName);
                    reasoning.AppendLine(" is not a valid world state.");
                    return false;
                }
            }
            return true;
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
                        thisRequirementMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value < m_RequiredStats[i].Value;
                        if (thisRequirementMet) {
                            reasoning.Append(" is in the right range since it is less than ");
                        } 
                        else
                        {
                            reasoning.Append(" is in the wrong range since it is not less than ");
                        }
                        break;
                    case Objective.Approximately:
                        thisRequirementMet = Mathf.Approximately(brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value, m_RequiredStats[i].Value);
                        if (thisRequirementMet)
                        {
                            reasoning.Append(" is in the right range since it is approximately equal to ");
                        }
                        else
                        {
                            reasoning.Append(" is in the wrong range since it is not approximately equal to ");
                        }
                        break;
                    case Objective.GreaterThan:
                        thisRequirementMet = brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value > m_RequiredStats[i].Value;
                        if (thisRequirementMet)
                        {
                            reasoning.Append(" is in the right range since it is greater than ");
                        }
                        else
                        {
                            reasoning.Append(" is in the wrong range since it is not greater than ");
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
                if (value && !IsExecuting)
                {
                    EndTime = Time.timeSinceLevelLoad + m_AbortDuration;
                }

                m_IsExecuting = value;
            }
        }

        /// <summary>
        /// Called when the behaviour is awoken, from the `Awake` method of the underlying
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
        /// Start this behaviour without an interactable. If this behaviour requires
        /// an interactable and somehow this method gets called it will return with no
        /// actions (after logging a warning).
        /// </summary>
        internal virtual void StartBehaviour(float duration)
        {
            EndTime = Time.timeSinceLevelLoad + duration;

            for (int i = 0; i < m_CharacterInfluences.Length; i++)
            {
                StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                influencer.InteractionName = m_CharacterInfluences[i].statTemplate.name;
                influencer.Trigger = null;
                influencer.stat = m_CharacterInfluences[i].statTemplate;
                influencer.maxChange = m_CharacterInfluences[i].maxChange;
                influencer.duration = duration;
                influencer.CooldownDuration = 0;

                brain.TryAddInfluencer(influencer);
            }

            if (m_OnStartCue != null) {
                m_OnStartCue.Prompt(brain.Actor);
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
            for (int i = 0; i < brain.UnsatisfiedDesiredStates.Count; i++)
            {
                for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].name == DesiredStateImpacts[idx].statTemplate.name)
                    {
                        reasoning.Append("They are not ");
                        reasoning.Append(brain.UnsatisfiedDesiredStates[i].name);
                        reasoning.AppendLine(" and this behaviour will help.");
                        weight++;
                    }
                }
            }
            weight /= brain.UnsatisfiedDesiredStates.Count;

            reasoning.Append("Total weight for this behaviour is ");
            reasoning.AppendLine(weight.ToString("0.000"));

            return weight;
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
                FinishBehaviour();
            }
        }

        

        

        /// <summary>
        /// Does the interactable have the desired impact to satisfy this behaviour.
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns></returns>
        internal bool HasDesiredImpact(Interactable interactable)
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

        internal virtual void FinishBehaviour()
        {
            IsExecuting = false;
            EndTime = 0;

            if (m_OnEndCue != null)
            {
                m_OnEndCue.Prompt(brain.Actor);
            }
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
        [SerializeField, Tooltip("The value required for this stat (used in conjunction with the objective).")]
        float m_Value;

        public float Value
        {
            get { return m_Value; }
            set { 
                m_Value = value;
            }
        }

        public float NormalizedValue
        {
            get {
                if (statTemplate != null)
                {
                    return (Value - statTemplate.MinValue) / (statTemplate.MaxValue - statTemplate.MinValue);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
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