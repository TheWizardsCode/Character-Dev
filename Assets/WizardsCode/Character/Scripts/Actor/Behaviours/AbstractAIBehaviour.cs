using UnityEngine;

using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;
using static WizardsCode.Character.StateSO;
using System.Text;
using WizardsCode.Character.WorldState;
using WizardsCode.Character.AI;
using UnityEngine.Events;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField, Tooltip("A player readable description of the behaviour.")]
        [TextArea(3, 10)]
        string m_Description;
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed AI Behaviour";
        [SerializeField, Tooltip("Icon for this behaviour.")]
        internal Sprite Icon;

        [Header("Controls")]
        [SerializeField, Tooltip("How frequently, in seconds, this behaviour should be tested for activation."), Range(0.01f, 5f)]
        float m_RetryFrequency = 2;
        [SerializeField, Tooltip("Is this behaviour interuptable. That is if the actor decides something else is more important can this behaviour be finished early.")]
        bool m_isInteruptable = false;
        [SerializeField, Tooltip("Time until execution of this behaviour is ended. " +
            "For behaviours that act on the self rather than another interaction this is the duration of the behaviour." +
            "For behaviours that involve an interaction this is also used as a safeguard in case something prevents the actor from completing " +
            "the actions associated with this behaviour, e.g. if they are unable to reach the chosen interactable.")]
        float m_MaximumExecutionTime = 30;
        [SerializeField, Tooltip("If a behaviour is blocking it means no other blocking behaviour can be carried out at the same time. Most behaviours are blocking, however, some special behaviours, such as being preganant, do not entirely block other behaviours.")]
        bool m_IsBlocking = true;

        [Header("Actions")]
        [SerializeField, Tooltip("Events to fire when this behaviour is started.")]
        UnityEvent m_OnStartEvent;
        [SerializeField, Tooltip("Events to fire when this behaviour is finished.")]
        UnityEvent m_OnEndEvent;
        [SerializeField, Tooltip("An actor cue to send to the actor upon the start of this interaction.")]
        protected ActorCue m_OnStartCue;
        [SerializeField, Tooltip("An actor cue to send to the actor upon the ending of this interaction. This should set the character back to their default state.")]
        protected ActorCue m_OnEndCue;

        [Header("Conditions")]
        [SerializeField, Range(0.1f, 5), Tooltip("The Weight Multiplier is used to lower or higher the priority of this behaviour relative to others the actor has. The higher this multiplier is the more likely it is the behaviour will be fired. The lower, the less likely.")]
        float m_WeightMultiplier = 1;
        [SerializeField, Tooltip("The required senses about the current world state around the actor. For example, we may have a sense for whether there is a willing mate nearby which will permit a make babies  behaviour to fire. Another example is that a" +
            "character will only sleep in the open if they sense there are no threats nearby.")]
        AbstractSense[] m_RequiredSenses;
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash.")]
        RequiredStat[] m_RequiredStats = default;
        [SerializeField, Tooltip("The set of character stats and the influence to apply to them when a character chooses this behaviour AND the behaviour does not require an interactable (influences come from the interactable if one is requried).")]
        internal StatInfluence[] m_CharacterInfluences;
        [SerializeField, Tooltip("The impacts we need an interactable to have on states for this behaviour to be enabled by it.")]
        DesiredStatImpact[] m_DesiredStateImpacts = new DesiredStatImpact[0];
        [SerializeField, Tooltip("The conditions required in the worldstate for this behaviour to be valid.")]
        WorldStateSO[] m_RequiredWorldState;

        public float MaximumExecutionTime
        {
            get { return m_MaximumExecutionTime; }
        }

        /// <summary>
        /// Is this a blocking behaviour? There can only be one blocking behaviour active at any one time.
        /// However, there can be multipl non-blocking behaviours active at once.
        /// </summary>
        public bool IsBlocking
        {
            get { return m_IsBlocking; }
        }

        public bool IsInteruptable
        {
            get { return m_isInteruptable; }
        }

        public DesiredStatImpact[] DesiredStateImpacts
        {
            get { return m_DesiredStateImpacts; }
        }

        Brain m_Brain;
        internal ActorController controller;
        private bool m_IsExecuting = false;
        private float m_NextRetryTime;

        internal StringBuilder reasoning = new StringBuilder();

        /// <summary>
        /// Get an array of all the things that have been recently sensed
        /// using the RequireSenses of this actor.
        /// </summary>
        internal List<Transform> SensedThings
        {
            get {
                List<Transform> result = new List<Transform>();
                for (int i = 0; i < m_RequiredSenses.Length; i++)
                {
                    result.AddRange(m_RequiredSenses[i].SensedThings);
                }
                return result;
            }
        }

        internal MemoryController Memory { get { return Brain.Memory; } }

        /// <summary>
        /// Get the brain this behaviour is being managed by.
        /// </summary>
        internal Brain Brain
        {
            get
            {
                return m_Brain;
            }
        }

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

                if (CheckWorldState() && CheckCharacteHasRequiredStats() && CheckSenses())
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check that all the required senses of the world around the cahracter are true.
        /// </summary>
        /// <returns>True if all senses are true</returns>
        public bool CheckSenses()
        {
            for (int i = 0; i < m_RequiredSenses.Length; i++)
            {
                if (!m_RequiredSenses[i].HasSensed)
                {
                    reasoning.Append(m_RequiredSenses[i].logName);
                    reasoning.AppendLine(" has not sensed what it needs recently.");
                    return false;
                }
            }
            return true;
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
                return true;
            }

            bool allRequirementsMet = true;
            bool thisRequirementMet = false;
            for (int i = 0; i < m_RequiredStats.Length; i++)
            {
                reasoning.Append(m_RequiredStats[i].statTemplate.DisplayName);
                reasoning.Append(" is ");

                switch (m_RequiredStats[i].objective)
                {
                    case Objective.LessThan:
                        thisRequirementMet = Brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value < m_RequiredStats[i].Value;
                        if (!thisRequirementMet) {
                            reasoning.Append("in the wrong range since it is not less than ");
                        }
                        break;
                    case Objective.Approximately:
                        thisRequirementMet = Mathf.Approximately(Brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value, m_RequiredStats[i].Value);
                        if (!thisRequirementMet)
                        {
                            reasoning.Append("in the wrong range since it is not approximately equal to ");
                        }
                        break;
                    case Objective.GreaterThan:
                        thisRequirementMet = Brain.GetOrCreateStat(m_RequiredStats[i].statTemplate).Value > m_RequiredStats[i].Value;
                        if (!thisRequirementMet)
                        {
                            reasoning.Append("is in the wrong range since it is not greater than ");
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
        public virtual bool IsExecuting {
            get { return m_IsExecuting; }
            internal set
            {
                if (value && !IsExecuting)
                {
                    EndTime = Time.timeSinceLevelLoad + m_MaximumExecutionTime;
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

            if (m_Brain == null)
            {
                m_Brain = transform.root.GetComponentInChildren<Brain>();
                if (m_Brain != null)
                {
                    Debug.LogWarning("Brain was not configured in " + DisplayName + ". " + m_Brain.DisplayName + " was automatically discovered. It is safer to set the brain in the inspector.");
                } else
                {
                    Debug.LogWarning("Brain was not configured in " + DisplayName + ". Set the brain in the inspector.");
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
            IsExecuting = true;
            EndTime = Time.timeSinceLevelLoad + duration;
            AddCharacterInfluencers(duration);

            if (m_OnStartEvent != null)
            {
                m_OnStartEvent.Invoke();
            }

            if (m_OnStartCue != null)
            {
                Brain.Actor.Prompt(m_OnStartCue);
            }
        }

        /// <summary>
        /// Add all the character influencers that operate over time from this behaviour to the stats tracker.
        /// </summary>
        /// <param name="duration">The time over which the influencers should be applied</param>
        internal void AddCharacterInfluencers(float duration)
        {
            for (int i = 0; i < m_CharacterInfluences.Length; i++)
            {
                if (!m_CharacterInfluences[i].applyOnCompletion)
                {
                    StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                    influencer.InteractionName = m_CharacterInfluences[i].statTemplate.name;
                    influencer.Trigger = null;
                    influencer.stat = m_CharacterInfluences[i].statTemplate;
                    influencer.maxChange = m_CharacterInfluences[i].maxChange;
                    influencer.duration = duration;
                    influencer.CooldownDuration = 0;

                    Brain.TryAddInfluencer(influencer);
                }
            }
        }

        /// <summary>
        /// Calculates the current weight for this behaviour between 0 (don't execute)
        /// and infinity (really want to execute). By default this is directly proportional to,
        /// the number of unsatisfied stats within desired states in the brain that this behaviour 
        /// impacts.
        /// 
        /// This is the base weight multiplied by the weight multiplier.
        /// 
        /// If there are no unsatisfiedDesiredStates then the weight will be 1 * the multiplier
        /// </summary>
        internal virtual float Weight(Brain brain)
        {
            float weight = BaseWeight(brain) * m_WeightMultiplier;

            return weight;
        }
        
        /// <summary>
        /// The base weight is the weight befre the multiplier is applied.
        /// </summary>
        /// <param name="brain">The brain containing the stats to be applied</param>
        /// <returns>The base weight, before the multiplier is applied.</returns>
        protected virtual float BaseWeight(Brain brain)
        {
            float weight = 1f;
            for (int i = 0; i < brain.UnsatisfiedDesiredStates.Count; i++)
            {
                for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].statTemplate == DesiredStateImpacts[idx].statTemplate)
                    {
                        float impact = Math.Abs(brain.UnsatisfiedDesiredStates[i].normalizedTargetValue - brain.GetStat(brain.UnsatisfiedDesiredStates[i].statTemplate).NormalizedValue);
                        reasoning.Append("They are not ");
                        reasoning.Append(brain.UnsatisfiedDesiredStates[i].name);
                        reasoning.AppendLine(" and this behaviour will help.");
                        //TODO higher weight should be given to behaviours that will bring the stat into the desired state
                        weight += impact;
                    }
                }
            }

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

        private void OnEnable()
        {
            Brain.RegisterBehaviour(this);
        }

        private void OnDisable()
        {
            Brain.DeregisterBehaviour(this);
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

            for (int i = 0; i < m_CharacterInfluences.Length; i++)
            {
                if (m_CharacterInfluences[i].applyOnCompletion)
                {
                    StatSO stat = Brain.GetOrCreateStat(m_CharacterInfluences[i].statTemplate);
                    stat.Value += m_CharacterInfluences[i].maxChange;
                }
            }

            if (m_OnEndEvent != null)
            {
                m_OnEndEvent.Invoke();
            }

            if (m_OnEndCue != null)
            {
                 Brain.Actor.Prompt(m_OnEndCue);
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
        [SerializeField, Tooltip("Should the influence be applied gradually over the duration of the behaviour or upon completion of the behaviour?")]
        public bool applyOnCompletion;
    }
}