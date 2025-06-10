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
using NaughtyAttributes;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        #region Inspector Fields
        // UI
        [SerializeField, Tooltip("The name to use in the User Interface."), BoxGroup("UI")]
        [Required]
        string m_DisplayName = string.Empty;
        [SerializeField, Tooltip("A player readable description of the behaviour."), BoxGroup("UI")]
        [TextArea(3, 10), Required]
        string m_Description;
        [SerializeField, Tooltip("Icon for this behaviour."), BoxGroup("UI")]
        internal Sprite Icon;

        [Space]
        // Controls
        [SerializeField, Tooltip("How frequently, in seconds, this behaviour should be tested for activation."), Range(0.01f, 5f), BoxGroup("Controls")]
        float m_RetryFrequency = 2;
        [SerializeField, Tooltip("Is this behaviour interuptable. That is if the actor decides something else is more important can this behaviour be finished early."), BoxGroup("Controls")]
        bool m_isInteruptable = false;
        [SerializeField, Tooltip("Maximum time until execution of this behaviour is ended. " +
            "For behaviours that act on the self rather than another interactable object or actor this is the duration of the behaviour." +
            "For behaviours that involve an interaction with another object or actor the duration is defined by that interaction. " +
            "In this situation this value is used as a safeguard in case something prevents the actor from completing " +
            "the actions associated with this behaviour, e.g. if they are unable to reach the chosen interactable."), BoxGroup("Controls")]
        float m_MaximumExecutionTime = 30;
        [SerializeField, Tooltip("If a behaviour is blocking it means no other blocking behaviour can be carried out at the same time. Most behaviours are blocking, however, some special behaviours, such as being pregnant, do not entirely block other behaviours."), BoxGroup("Controls")]
        bool m_IsBlocking = true;

        [Space]
        // Events
        [SerializeField, Tooltip("Events to fire when this behaviour is started."), BoxGroup("Events")]
        protected UnityEvent m_OnStartEvent;
        [SerializeField, Tooltip("Events to fire when this behaviour is finished."), BoxGroup("Events")]
        protected UnityEvent m_OnEndEvent;

        [Space]
        // Lifecycle
        [SerializeField, Tooltip("A timeline asset to play when this behaviour is started."), BoxGroup("Lifecycle")]
        internal TimelineAsset m_timeline;
        [SerializeField, Tooltip("An actor cue to send to the actor upon the start of this interaction. It should be used to configure the actor ready for the interaction."), BoxGroup("Lifecycle")]
        // [FormerlySerializedAs("m_OnStart")] // v0.12
        protected ActorCue m_OnStartCue;
        [SerializeField, Tooltip("An actor cue to send to the actor as they start the prepare phase of this interaction. This is where you will typically play wind up animations and the like."), BoxGroup("Lifecycle")]
        // [FormerlySerializedAs("m_OnArrivingCue")] // v0.11
        // [FormerlySerializedAs("m_OnPrepare")] // v0.12
        protected ActorCue m_OnPrepareCue;
        [SerializeField, Tooltip("A set of actor cues from which to randomly select an appropriate cue when enacting this behaviour. This is where you will usually play animations and sounds reflecting the interaction itself."), BoxGroup("Lifecycle")]
        // [FormerlySerializedAs("m_OnPerformInteraction")] // changed in v0.1.1
        // [FormerlySerializedAs("m_OnPerformAction")] // v0.12
        protected ActorCue[] m_OnPerformCue = new ActorCue[0];
        [SerializeField, Tooltip("The minimum duration of the performance phase of this interaction. Note that if there are explicit perform cues this value will be overridden by the settings in those cues."), HideIf("m_OnPerformCue"), BoxGroup("Lifecycle")]
        protected float m_MinimumPerformanceDuration = 1;
        [SerializeField, Tooltip("An actor cue to send to the actor as they finalize this interaction. This is where you will typically play wind up animations and the like."), BoxGroup("Lifecycle")]
        // [FormerlySerializedAs("m_OnFinalize")] // v0.12
        protected ActorCue m_OnFinalizeCue;
        [SerializeField, Tooltip("An actor cue sent when ending this interaction. This should set the character back to their default state."), BoxGroup("Lifecycle")]
        // [FormerlySerializedAs("m_OnEnd")] // v0.12
        protected ActorCue m_OnEndCue;
        [SerializeField, Tooltip("If this behaviour should always be followed by the same behaviour (assuming it is possible) drop the behaviour here. If this is null the brain will be free to select its own behaviour"), BoxGroup("Lifecycle")]
        AbstractAIBehaviour m_NextBehaviour;
        [SerializeField, Tooltip("Indicates if this behaviour should be destroyed, and thus removed from the character when it next enters the Inactive state."), BoxGroup("Lifecycle")]
        public bool DestroyOnInactive = false;

        [Space]
        // Execution Conditions
        [SerializeField, Range(0.1f, 5), Tooltip("The Weight Multiplier is used to lower or higher the priority of this behaviour relative to others the actor has. The higher this multiplier is the more likely it is the behaviour will be fired. The lower, the less likely."), BoxGroup("Execution Conditions")]
        internal float m_WeightMultiplier = 1;
        [SerializeField, Range(0f, 2f), Tooltip("An allowable variation in the Weight Multiplier. Each time the behaviour is evaluated the base weight multiplier will be increased or decreased by a random number between +/- this amount."), BoxGroup("Execution Conditions")]
        float m_WeightVariation = 0.1f;
        [SerializeField, Tooltip("The required senses about the current world state around the actor. For example, we may have a sense for whether there is a willing mate nearby which will permit a make babies  behaviour to fire. Another example is that a" +
            "character will only sleep in the open if they sense there are no threats nearby."), BoxGroup("Execution Conditions")]
        AbstractSense[] m_RequiredSenses = new AbstractSense[0];
        [SerializeField, Tooltip("The required stats to enable this behaviour. Here you should set minimum, maximum or approximate values for stats that are needed for this behaviour to fire. For example, buying items is only possible if the actor has cash."), BoxGroup("Execution Conditions")]
        RequiredStat[] m_RequiredStats = new RequiredStat[0];
        [SerializeField, Tooltip("The set of character stats and the influence to apply to them when a character chooses this behaviour AND the behaviour does not require an interactable (influences come from the interactable if one is required)."), BoxGroup("Execution Conditions")]
        internal StatInfluence[] m_CharacterInfluences = new StatInfluence[0];
        [SerializeField, Tooltip("The impacts we need an interactable to have on states for this behaviour to be enabled by it."), BoxGroup("Execution Conditions")]
        DesiredStatImpact[] m_DesiredStateImpacts = new DesiredStatImpact[0];
        [SerializeField, Tooltip("The conditions required in the world state for this behaviour to be valid."), BoxGroup("Execution Conditions")]
        WorldStateSO[] m_RequiredWorldState = new WorldStateSO[0];
        #endregion

        private PlayableDirector m_Director;

        public enum State { Starting, Preparing, Performing, Finalizing, Ending, Inactive, MovingTo }
        State m_State = State.Inactive;
        public State CurrentState {
            get { return m_State; }
            internal set
            {
                if (m_State != value)
                {
                    m_State = value;
                    if (m_State == State.Inactive)
                    {
                        EndTime = 0;
                    }
                }
            }
        }

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        AnimatorActorController m_AnimatorController;
        bool m_AttemptedCastToAnimatorController = false;
        
        /// <summary>
        /// Get the ActorController this behaviour is influencing.
        /// 
        /// <seealso cref="AnimatorActorController"/> 
        /// </summary>
        internal BaseActorController ActorController
        {
            get
            {
                return m_ActorController;
            }
        }

        /// <summary>
        /// If the actor controller is an animator controller then this will return it, otherwise
        /// it will return a null.
        /// 
        /// This parameter is optimized to avoid multiple casts to AnimatorActorController. 
        /// It is strongly recommended that this parameter be used instead of casting `ActorController`.
        /// <seealso cref="ActorController"/>
        /// </summary>
        internal AnimatorActorController AnimatorActorController {
            get
            {
                if (m_AttemptedCastToAnimatorController && m_AnimatorController == null)
                {
                    m_AnimatorController = m_ActorController as AnimatorActorController;
                    m_AttemptedCastToAnimatorController = true;
                }

                return m_AnimatorController;
            }
        }

        public float MaximumExecutionTime
        {
            get { return m_MaximumExecutionTime; }
        }

        /// <summary>
        /// Is this a blocking behaviour? There can only be one blocking behaviour active at any one time.
        /// However, there can be multiple non-blocking behaviours active at once.
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
                    && CheckCharacterHasRequiredStats()
                    && CheckSenses()))
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }
        private EmotionalState m_emotionalState;

        /// <summary>
        /// Check that all the required senses of the world around the character are true.
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
        /// If found to be invalid the reasoning log will have details.
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
        private bool CheckCharacterHasRequiredStats()
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
                reasoning.Append($"{stat.DisplayName} has a current value of {stat.Value}");
                switch (m_RequiredStats[i].objective)
                {
                    case Objective.LessThan:
                        thisRequirementMet = stat.Value < m_RequiredStats[i].Value;
                        reasoning.Append($" and a target value of < {m_RequiredStats[i].Value}.");
                        break;
                    case Objective.Approximately:
                        thisRequirementMet = Mathf.Approximately(stat.Value, m_RequiredStats[i].Value);
                        reasoning.Append($" and a target value of ~= {m_RequiredStats[i].Value}.");
                        break;
                    case Objective.GreaterThan:
                        thisRequirementMet = stat.Value > m_RequiredStats[i].Value;
                        reasoning.Append($" and a target value of > {m_RequiredStats[i].Value}.");
                        break;
                    default:
                        Debug.LogError("Don't know how to handle an Objective of " + m_RequiredStats[i].objective);
                        thisRequirementMet = false;
                        reasoning.Append("Error in processing " + m_RequiredStats[i] + " unrecognized objective: " + m_RequiredStats[i].objective);
                        break;
                }
                if (thisRequirementMet)
                {
                    reasoning.AppendLine($" This requirement /is/ met.");
                }
                else
                {
                    reasoning.AppendLine($" This requirement is /not/ met.");
                }
                allRequirementsMet &= thisRequirementMet;
            }

            return allRequirementsMet;
        }

        /// <summary>
        /// Called whenever the behaviour is enabled. Any configuration for all runs of the behaviour 
        /// should be added here. It is important to recognize that this means it 
        /// may be called multiple times since behaviours are enabled and disabled by the brain depending 
        /// on their relevance to the current Actor state. Be sure to consider the impact of this on 
        /// performance.
        /// </summary>
        protected virtual void Init()
        {
            m_ActorController = GetComponentInParent<BaseActorController>();
            m_Brain = m_ActorController.GetComponentInChildren<Brain>();
            m_emotionalState = m_ActorController.GetComponentInChildren<EmotionalState>();

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
        /// `StartBehaviour` called whenever the Brain wants this behaviour to be enacted. 
        /// Any configuration for a specific execution of the behaviour should occur here.
        /// 
        /// If this behaviour requires
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
        /// Add all the character influencers that operate over time from this behaviour to the stats tracker.
        /// </summary>
        /// <param name="duration">The time over which the influencers should be applied</param>
        internal void AddCharacterInfluencers(float duration)
        {
            for (int i = 0; i < m_CharacterInfluences.Length; i++)
            {
                if (!m_CharacterInfluences[i].applyOnCompletion)
                {
                    Brain.TryAddInfluence(m_CharacterInfluences[i], duration);
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
        /// The base weight is the weight before the multiplier is applied.
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
                        //TODO higher weight should be given to behaviours that will bring the stat into the desired state
                        weight += impact;
                    }
                }
            }

            return weight;
        }

        /// <summary>
        /// This is the standard Unity `Update` method. Generally this should not be overridden.
        /// Use `OnUpdateState` instead.
        /// 
        /// <seealso cref="OnUpdateState"/>
        /// </summary>
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
                    if (AnimatorActorController != null) {
                        AnimatorActorController.Animator.applyRootMotion = true;
                    }
                    CurrentState = State.Performing;
                }

                if (m_Director.state == PlayState.Playing)
                {
                    return;
                } else
                {
                    if (AnimatorActorController != null) {
                        AnimatorActorController.Animator.applyRootMotion = false;
                    }
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
        /// `OnUpdateState` this is used to update the behaviour while it is being executed. 
        /// Whenever the state of the behaviour needs to be changes this method should be called.
        /// The method will change the state based on the current state and the state of the actor controller and/or behaviour being executed.
        /// Generally this method will set the state of the behaviour to the next state in the lifecycle of the behaviour, e.g. MovingTo -> Starting -> Preparing -> etc.
        /// </summary>
        /// <seealso cref="State"/>
        /// <seealso cref="CurrentState"/>
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
                AddCharacterInfluencers(10); // TODO: this shouldn't be hard coded to 10 seconds?

                if (m_OnStartCue != null)
                {
                    Brain.Actor.Prompt(m_OnStartCue);
                    EndTime = Time.timeSinceLevelLoad + m_OnStartCue.Duration;
                }

                if (m_OnStartEvent != null)
                {
                    m_OnStartEvent.Invoke();
                }

                CurrentState = CheckEnvironment();
                return;
            }

            if (CurrentState == State.Preparing) {
                if(Brain.Actor.IsMoving) {
                    // still moving to the target location for this behaviour
                    return;
                }
                else
                {
                    CurrentState = State.Performing;

                    if (m_OnPerformCue.Length > 0)
                    {
                        performingCue = m_OnPerformCue[Random.Range(0, m_OnPerformCue.Length)];
                        if (performingCue != null)
                        {
                            Brain.Actor.Prompt(performingCue);
                            EndTime = Time.timeSinceLevelLoad + performingCue.Duration;
                        }
                    }
                    else
                    {
                        EndTime = Time.timeSinceLevelLoad + m_MinimumPerformanceDuration;
                    }
                    return;
                }
            }

            if (CurrentState == State.Performing)
            {
                CurrentState = State.Finalizing;

                if (performingCue != null)
                {
                    // TODO: Is this now being called twice as I added it into the Prompt method?
                    StartCoroutine(performingCue.Revert(Brain.Actor));
                }

                if (m_OnFinalizeCue != null)
                {
                    Brain.Actor.Prompt(m_OnFinalizeCue);
                    EndTime = Time.timeSinceLevelLoad + m_OnFinalizeCue.Duration;
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

            if (DestroyOnInactive)
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
                EndTime = Time.timeSinceLevelLoad + m_OnPrepareCue.Duration;
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
                    reasoning.AppendLine($"{interactable.DisplayName} at {interactable.transform.position} doesn't have the desired effect on {DesiredStateImpacts[idx].statTemplate.DisplayName}.");
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
                EndTime = Time.timeSinceLevelLoad + m_OnEndCue.Duration;
            }

            AnimatorActorController?.PlayAnimatorController();
            
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
        // These values are hidden in the inspector because there is a custom editor
        // But at the time of writing it is incomplete.
        [SerializeField, Tooltip("The stat we require a value for.")]
        public StatSO statTemplate;
        [SerializeField, Tooltip("The object for this stats value, for example, greater than, less than or approximately equal to.")]
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