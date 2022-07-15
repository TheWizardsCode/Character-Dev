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
using UnityEngine.Serialization;
using static WizardsCode.Character.BaseActorController;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI")]
        [SerializeField, Tooltip("The name to use in the User Interface.")]
        string m_DisplayName = "Unnamed AI Behaviour";
        [SerializeField, Tooltip("A player readable description of the behaviour.")]
        [TextArea(3, 10)]
        string m_Description;
        [SerializeField, Tooltip("Icon for this behaviour.")]
        internal Sprite Icon;

        [Header("Controls")]
        [SerializeField, Tooltip("How frequently, in seconds, this behaviour should be tested for activation."), Range(0.01f, 5f)]
        float m_RetryFrequency = 2;
        [SerializeField, Tooltip("Is this behaviour interuptable. That is if the actor decides something else is more important can this behaviour be finished early.")]
        bool m_isInteruptable = false;
        [SerializeField, Tooltip("Maximum time until execution of this behaviour is ended. " +
            "For behaviours that act on the self rather than another interactable object or actor this is the duration of the behaviour." +
            "For behaviours that involve an interaction with another object or actor the duration is defined by that interaction. " +
            "In this situation this valu is used as a safeguard in case something prevents the actor from completing " +
            "the actions associated with this behaviour, e.g. if they are unable to reach the chosen interactable.")]
        float m_MaximumExecutionTime = 30;
        [SerializeField, Tooltip("If a behaviour is blocking it means no other blocking behaviour can be carried out at the same time. Most behaviours are blocking, however, some special behaviours, such as being preganant, do not entirely block other behaviours.")]
        bool m_IsBlocking = true;

        [Header("Events")]
        [SerializeField, Tooltip("Events to fire when this behaviour is started.")]
        protected UnityEvent m_OnStartEvent;
        [SerializeField, Tooltip("Events to fire when this behaviour is finished.")]
        protected UnityEvent m_OnEndEvent;

        [Header("Cues")]
        [SerializeField]
        internal TimelineAsset m_timeline;
        [SerializeField, Tooltip("An actor cue to send to the actor upon the start of this interaction. It should be used to configure the actor ready for the interaction.")]
        [FormerlySerializedAs("m_OnStart")] // v0.12
        protected ActorCue m_OnStartCue;
        [SerializeField, Tooltip("An actor cue to send to the actor as they start the prepare phase of this interaction. This is where you will typically play wind up animations and the like.")]
        [FormerlySerializedAs("m_OnArrivingCue")] // v0.11
        [FormerlySerializedAs("m_OnPrepare")] // v0.12
        protected ActorCue m_OnPrepareCue;
        [SerializeField, Tooltip("A set of actor cues from which to randomly select an appropriate cue when enacting this behaviour. This is where you will usually play animations and sounds reflecting the interaction itself.")]
        [FormerlySerializedAs("m_OnPerformInteraction")] // changed in v0.1.1
        [FormerlySerializedAs("m_OnPerformAction")] // v0.12
        protected ActorCue[] m_OnPerformCue;
        [SerializeField, Tooltip("An actor cue to send to the actor as they finalize this interaction. This is where you will typically play wind up animations and the like.")]
        [FormerlySerializedAs("m_OnFinalize")] // v0.12
        protected ActorCue m_OnFinalizeCue;
        [SerializeField, Tooltip("An actor cue sent when ending this interaction. This should set the character back to their default state.")]
        [FormerlySerializedAs("m_OnEnd")] // v0.12
        protected ActorCue m_OnEndCue;
        [SerializeField, Tooltip("If this behaviour should always be followed by the same behaviour (assuming it is possible) drop the behaviour here. If this is null the brain will be free to select its own behaviour")]
        AbstractAIBehaviour m_NextBehaviour;

        [Header("Conditions")]
        [SerializeField, Range(0.1f, 5), Tooltip("The Weight Multiplier is used to lower or higher the priority of this behaviour relative to others the actor has. The higher this multiplier is the more likely it is the behaviour will be fired. The lower, the less likely.")]
        float m_WeightMultiplier = 1;
        [SerializeField, Range(0f, 2f), Tooltip("An allowable variation in the Weight Multiplier. Each time the behaviour is evaluated the base weight multiplier will be increased or decreased by a random number between +/- this amount.")]
        float m_WeightVariation = 0.1f;
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
        #endregion

        public enum State { Starting, Preparing, Performing, Finalizing, Ending, Inactive, MovingTo }
        public State CurrentState = State.Inactive;

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
        internal BaseActorController m_ActorController;
        private float m_NextRetryTime;

        internal StringBuilder reasoning = new StringBuilder();
        private ActorCue performingCue;

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
                    result.AddRange(m_RequiredSenses[i].sensedThings);
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

        /// <summary>
        /// Get the ActorController this behaviour movements are managed by.
        /// </summary>
        internal BaseActorController ActorController
        {
            get
            {
                return m_ActorController;
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

        public float MaxEndTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Tests to see if this behaviour is available to be executed. That is are the necessary preconditions
        /// met.
        /// </summary>
        public virtual bool IsAvailable
        {
            get
            {
                if (Time.timeSinceLevelLoad < m_NextRetryTime) return false;
                m_NextRetryTime = Time.timeSinceLevelLoad + m_RetryFrequency;

                reasoning.Clear();

                if ((CheckWorldState()
                    && CheckCharacteHasRequiredStats()
                    && CheckSenses()))
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Indicates if this behaviour should be destroyed, and thus removed from the character
        /// when it next enters the Inactive state.
        /// </summary>
        public bool DestoryOnInactive = false;
        private PlayableDirector m_Director;
        private EmotionalState m_emotionalState;

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
                StatSO stat = Brain.GetOrCreateStat(m_RequiredStats[i].statTemplate);
                reasoning.Append($"{stat.DisplayName} ({stat.Value})");

                switch (m_RequiredStats[i].objective)
                {
                    case Objective.LessThan:
                        thisRequirementMet = stat.Value < m_RequiredStats[i].Value;
                        if (!thisRequirementMet) {
                            reasoning.Append(" is in the wrong range since it is not less than ");
                        }
                        break;
                    case Objective.Approximately:
                        thisRequirementMet = Mathf.Approximately(stat.Value, m_RequiredStats[i].Value);
                        if (!thisRequirementMet)
                        {
                            reasoning.Append(" is in the wrong range since it is not approximately equal to ");
                        }
                        break;
                    case Objective.GreaterThan:
                        thisRequirementMet = stat.Value > m_RequiredStats[i].Value;
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
        /// Called when the behaviour is awoken, from the `Awake` method of the underlying
        /// `MonoBehaviour`.
        /// </summary>
        protected virtual void Init()
        {
            m_Brain = transform.root.GetComponentInChildren<Brain>();
            m_ActorController = transform.root.GetComponentInChildren<BaseActorController>();
            m_emotionalState = transform.root.GetComponentInChildren<EmotionalState>();

            m_Director = GetComponent<PlayableDirector>();
            if (m_Director == null)
            {
                m_Director = gameObject.AddComponent<PlayableDirector>();
            }

            if (m_timeline)
            {
                foreach (var track in m_timeline.GetOutputTracks())
                {
                    m_Director.SetGenericBinding(track, m_ActorController.gameObject);
                }
                m_Director.playableAsset = m_timeline;
            }
        }

        /// <summary>
        /// Start this behaviour without an interactable. If this behaviour requires
        /// an interactable and somehow this method gets called it will return with no
        /// actions (after logging a warning).
        /// </summary>
        internal virtual void StartBehaviour()
        {
            MaxEndTime = Time.timeSinceLevelLoad + MaximumExecutionTime;
            CurrentState = State.Starting;

            if (m_Director != null)
            {
                m_Director.Play();
            }
        }

        /// <summary>
        /// If this cue has animations for the start, prepare, perform, finalize and/or end phases then set
        /// them up in the animator.
        /// </summary>
        void SetupAnimations()
        {
            if (m_OnStartCue is ActorCueAnimator)
            {
                Debug.Log("Setup Start Animations");
            }
            
            if (m_OnPrepareCue is ActorCueAnimator)
            {
                //m_ActorController.Animator.SetPrepareClip(((ActorCueAnimator)m_OnPrepareCue).Clip);
            }
            
            for (int i = 0; i < m_OnPerformCue.Length; i++)
            {
                if (m_OnPerformCue[i] is ActorCueAnimator)
                {
                    Debug.Log("Setup Perform Animations");
                }
            }

            if (m_OnFinalizeCue is ActorCueAnimator)
            {
                Debug.Log("Setup Finalize Animations");
            }
            
            if (m_OnEndCue is ActorCueAnimator)
            {
                Debug.Log("Setup End Animations");
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
            float multiplier = m_WeightMultiplier + (Random.Range(-m_WeightVariation, m_WeightVariation));
            return BaseWeight(brain) * multiplier;
        }

        /// <summary>
        /// The base weight is the weight befre the multiplier is applied.
        /// </summary>
        /// <param name="stats">The stats to be applied</param>
        /// <returns>The base weight, before the multiplier is applied.</returns>
        protected virtual float BaseWeight(StatsTracker stats)
        {
            float weight = 1f;
            for (int i = 0; i < stats.UnsatisfiedDesiredStates.Count; i++)
            {
                for (int idx = 0; idx < DesiredStateImpacts.Length; idx++)
                {
                    if (stats.UnsatisfiedDesiredStates[i].statTemplate == DesiredStateImpacts[idx].statTemplate)
                    {
                        float impact = Math.Abs(stats.UnsatisfiedDesiredStates[i].normalizedTargetValue - stats.GetStat(stats.UnsatisfiedDesiredStates[i].statTemplate).NormalizedValue);
                        reasoning.Append("They are not ");
                        reasoning.Append(stats.UnsatisfiedDesiredStates[i].name);
                        reasoning.AppendLine(" and this behaviour will help.");
                        //TODO higher weight should be given to behaviours that will bring the stat into the desired state
                        weight += impact;
                    }
                }
            }

            return weight;
        }

        public virtual void Update()
        {
            if (CurrentState == State.MovingTo || CurrentState == State.Inactive)
            {
                return;
            }

            if (m_timeline)
            {
                if (CurrentState == State.Starting)
                {
                    m_ActorController.Animator.applyRootMotion = true;
                    CurrentState = State.Performing;
                }

                if (m_Director.state == PlayState.Playing)
                {
                    return;
                } else
                {
                    m_ActorController.Animator.applyRootMotion = false;
                    EndBehaviour();
                    return;
                }
            }

            if (CurrentState != State.Inactive
                && (EndTime < Time.timeSinceLevelLoad
                || MaxEndTime < Time.timeSinceLevelLoad))
            {
                OnUpdateState();
            }
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. By default this will look
        /// for interactables nearby that will satisfy the needs of this behaviour.
        /// </summary>
        protected virtual void OnUpdateState()
        {   
            if (CurrentState == State.MovingTo)
            {
                if (m_ActorController.state == States.Arrived)
                {
                    CurrentState = State.Starting;
                }
                return;
            }

            if (CurrentState == State.Starting)
            {
                AddCharacterInfluencers(10); // TODO: why is this hard coded to 10 seconds?

                if (m_OnStartCue != null)
                {
                    Brain.Actor.Prompt(m_OnStartCue);
                    EndTime = Time.timeSinceLevelLoad + m_OnStartCue.m_Duration;
                }

                if (m_OnStartEvent != null)
                {
                    m_OnStartEvent.Invoke();
                }

                CurrentState = CheckEnvironment();
                return;
            }

            if (CurrentState == State.Preparing && !Brain.Actor.IsMoving)
            {
                CurrentState = State.Performing;

                if (m_OnPerformCue.Length > 0)
                {
                    performingCue = m_OnPerformCue[Random.Range(0, m_OnPerformCue.Length)];
                    if (performingCue != null)
                    {
                        Brain.Actor.Prompt(performingCue);
                        EndTime = Time.timeSinceLevelLoad + performingCue.m_Duration;
                    }
                }
                else
                {
                    EndTime = Time.timeSinceLevelLoad + 1;
                }
                return;
            }

            if (CurrentState == State.Performing)
            {
                CurrentState = State.Finalizing;

                if (performingCue != null)
                {
                    StartCoroutine(performingCue.Revert(Brain.Actor));
                }

                if (m_OnFinalizeCue != null)
                {
                    Brain.Actor.Prompt(m_OnFinalizeCue);
                    EndTime = Time.timeSinceLevelLoad + m_OnFinalizeCue.m_Duration;
                }
                else
                {
                    EndTime = Time.timeSinceLevelLoad + 3;
                }
                return;
            }

            if (CurrentState == State.Finalizing)
            {
                CurrentState = State.Ending;

                EndTime = FinishBehaviour();
                return;
            }

            if (CurrentState == State.Ending)
            {
                CurrentState = State.Inactive;
                EndBehaviour();
                return;
            }
        }

        private void EndBehaviour()
        {
            Brain.ActiveBlockingBehaviour = null;
            Brain.ClearTarget();
            CurrentState = State.Inactive;

            if (DestoryOnInactive)
            {
                if (gameObject.GetComponents<AbstractAIBehaviour>().Length > 1)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Check the environment and the character to ensure that
        /// this behaviour can be carried out. 
        /// </summary>
        protected virtual State CheckEnvironment()
        {
            if (m_OnPrepareCue)
            {
                Brain.Actor.Prompt(m_OnPrepareCue);
                EndTime = Time.timeSinceLevelLoad + m_OnPrepareCue.m_Duration;
            }
            else
            {
                EndTime = Time.timeSinceLevelLoad;
            }

            OnPrepareCue();
            return State.Preparing; ;
        }

        /// <summary>
        /// Called whenever the PrepareCue is prompted. Subclasses can use this method to
        /// insert additional actions at this point in the behaviour lifecycle. 
        /// 
        /// By default this method does nothing.
        /// </summary>
        protected virtual void OnPrepareCue() { }

        private void OnEnable()
        {
            Init();
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
                    reasoning.Append($" doesn't have the desired effect on {DesiredStateImpacts[idx].statTemplate.DisplayName}.");
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Finish the behaviour, prompting any cue needed.
        /// </summary>
        /// <returns>The time, since level load, at which this behaviour should end.</returns>
        internal virtual float FinishBehaviour()
        {
            CurrentState = State.Ending;

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

            if (m_NextBehaviour != null)
            {
                Brain.PrioritizeBehaviour(m_NextBehaviour);
            }

            if (m_OnEndCue != null)
            {
                Brain.Actor.Prompt(m_OnEndCue);
                EndTime = Time.timeSinceLevelLoad + m_OnEndCue.m_Duration;
            }

            Brain.Actor.PlayAnimatorController();

            return EndTime;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        private void OnValidate()
        {
                if (transform.localPosition != Vector3.zero) transform.localPosition = Vector3.zero;
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
        [HideInInspector, SerializeField, Tooltip("The name of the interaction that created this influencer.")]
        string m_InteractionName;
        [SerializeField, Tooltip("The Stat this influencer acts upon.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait, to the limit of the stats allowable value.")]
        public float maxChange;
        [SerializeField, Tooltip("Should the influence be applied gradually over the duration of the behaviour or upon completion of the behaviour?")]
        public bool applyOnCompletion;
        [SerializeField, Tooltip("The cooldown period before a character can be influenced by this same influencer again, in seconds.")]
        public float cooldownDuration;
        [SerializeField, Tooltip("Should the influence applied be reset each time this influencer is added to an influenced object? If set to true then influence applied will be set to 0 and up to Max Change will be applied again. If set to false then the influence applied will not be reset and thus the Max Change will be the difference between the Max Change set above and any influence previously applied - that is this influencers influence will run out.")]
        public bool resetInfluenceApplied;

    }
}