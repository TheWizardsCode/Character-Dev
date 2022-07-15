using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WizardsCode.Character;
using WizardsCode.Character.Stats;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Stats {
    /// <summary>
    /// The Brain is responsible for tracking the stats and goal states of the character and
    /// making decisions and plans to reach those goal stats.
    /// </summary>
    public class StatsTracker : MonoBehaviour
#if UNITY_EDITOR
        , IDebug
#endif
    {
        [SerializeField, Tooltip("Desired States are the states that the actor would like to satisfy. These are, essentially, the things that drive the actor.")]
        StateSO[] m_DesiredStates = default;

        [Header("Optimization")]
        [SerializeField, Tooltip("How often stats should be processed for changes.")]
        protected float m_TimeBetweenUpdates = 0.5f;

        [HideInInspector, SerializeField]
        internal List<StatSO> m_Stats = new List<StatSO>();
        [HideInInspector, SerializeField]
        internal List<StatInfluencerSO> m_StatsInfluencers = new List<StatInfluencerSO>();

        protected float interactionOffset = 0.5f;
        internal float m_TimeOfNextUpdate = 0;
        private List<StateSO> m_UnsatisfiedDesiredStatesCache = new List<StateSO>();
        private BaseActorController m_TargetActor;
        private Interactable m_TargetInteractable;

        internal List<StatInfluencerSO> StatsInfluencers
        {
            get { return m_StatsInfluencers; }
        }

        public string DisplayName
        {
            get
            {
                return transform.root.gameObject.name;
            }
        }

        /// <summary>
        /// Desired States are the states that the actor would like to satisfy.
        /// These are, essentially, the things that drive the actor.
        /// </summary>
        public StateSO[] DesiredStates { get { return m_DesiredStates; } }

        public List<StateSO> UnsatisfiedDesiredStates
        {
            get { return m_UnsatisfiedDesiredStatesCache; }
            internal set { m_UnsatisfiedDesiredStatesCache = value; }
        }

        /// <summary>
        /// Tests to see if this stats tracker satisfies the requirements of a state.
        /// </summary>
        /// <param name="stateTemplate">The state to test against.</param>
        /// <returns>True if the requirements are satisfied, otherwise false.</returns>
        internal bool SatisfiesState(StateSO stateTemplate)
        {
            return !UnsatisfiedDesiredStates.Contains(stateTemplate);
        }

        /// <summary>
        /// Clear the current target. This will also clear the Target Actor and Target Interactable values.
        /// </summary>
        public virtual void ClearTarget()
        {
            m_TargetActor = null;
            m_TargetInteractable = null;
        }

        private Transform m_Target;
        /// <summary>
        /// Set the current target trasnform. The target is the thing that the AI is currently
        /// focusing their attention on. It may be an interactable, and actor or similar.
        /// When retrieving the target you can use GetTarget() to get the transform, but
        /// you can also use GetTargetInteractable() or GetTargetActor().
        /// </summary>
        public virtual void SetTarget(Transform value)
        {
            if (m_Target == value) return;

            m_Target = value;
            m_TargetActor = m_Target.GetComponentInChildren<BaseActorController>();
            m_TargetInteractable = m_Target.GetComponentInChildren<Interactable>();
        }
        /// <summary>
        /// Set the current interactable target. The target is the thing that the AI is currently
        /// focusing their attention on. if the interactable is also an actor then the appropriate
        /// actor value will also be set.
        /// When retrieving the target you can use GetTarget() to get the transform, but
        /// you can also use GetTargetInteractable() or GetTargetActor().
        /// </summary>
        public virtual void SetTarget(Interactable value)
        {
            if (value == m_TargetInteractable) return;

            m_TargetInteractable = value;

            if (m_TargetInteractable != null)
            {
                m_Target = m_TargetInteractable.transform.root;
                m_TargetActor = m_Target.GetComponentInChildren<BaseActorController>();
            }
            else
            {
                m_Target = null;
                m_TargetActor = null;
            }
        }
        /// <summary>
        /// Set the current actor target. The target is the thing that the AI is currently
        /// focusing their attention on. If the actor is also an interactable then the appropriate
        /// actor value will also be set.
        /// When retrieving the target you can use GetTarget() to get the transform, but
        /// you can also use GetTargetInteractable() or GetTargetActor().
        /// </summary>
        public virtual void SetTarget(BaseActorController value)
        {
            if (value == m_TargetActor) return;

            m_TargetActor = value;

            if (m_TargetActor != null)
            {
                m_Target = m_TargetActor.transform.root;
                m_TargetInteractable = m_Target.GetComponentInChildren<Interactable>();
            }
            else
            {
                m_Target = null;
                m_TargetInteractable = null;
            }
        }

        /// <summary>
        /// Return the current target transform.
        /// </summary>
        /// <returns>The transform the current target, if one exists.</returns>
        public virtual Transform GetTarget()
        {
            return m_Target;
        }

        /// <summary>
        /// If the current Target is an actor return the actor component.
        /// </summary>
        /// <returns>The actor component of the current target, if one exists.</returns>
        public virtual BaseActorController GetTargetActor()
        {
            return m_TargetActor;
        }

        /// <summary>
        /// If the current Target is an interactable return the actor component.
        /// </summary>
        /// <returns>The interactable component of the current target, if one exists.</returns>
        public virtual Interactable GetTargetInteractable()
        {
            return m_TargetInteractable;
        }

        /// <summary>
        /// Return an available interaction position for this brain.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetInteractionPosition()
        {
            return transform.position + (transform.forward * interactionOffset);
        }

        /// <summary>
        /// Decide whether the actor should interact with an influencer trigger they just entered.
        /// </summary>
        /// <param name="interactable">The influencer trigger that was activated and can now be interacted with.</param>
        /// <returns></returns>
        internal bool ShouldInteractWith(Interactable interactable)
        {
            if (interactable != null && GameObject.ReferenceEquals(interactable, GetTargetInteractable()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal virtual void Update()
        {
            if (Time.timeSinceLevelLoad < m_TimeOfNextUpdate) return;

            UpdateAllStats();
            ApplyStatInfluencerEffects();
            UpdateDesiredStates();

            m_TimeOfNextUpdate = Time.timeSinceLevelLoad + m_TimeBetweenUpdates;
        }

        internal void ApplyStatInfluencerEffects()
        {
            for (int i = StatsInfluencers.Count - 1; i >= 0; i--)
            {
                if (StatsInfluencers[i] != null)
                {
                    StatsInfluencers[i].ChangeStat(this);

                    if (Mathf.Abs(StatsInfluencers[i].influenceApplied) >= Mathf.Abs(StatsInfluencers[i].maxChange))
                    {
                        if (StatsInfluencers[i].Trigger)
                        {
                            StatsInfluencers[i].Trigger.StopCharacterInteraction(this);
                        }
                        StatsInfluencers.RemoveAt(i);
                    }
                }
                else
                {
                    StatsInfluencers.RemoveAt(i);
                }
            }
        }

        internal void UpdateAllStats()
        {
            for (int i = 0; i < m_Stats.Count; i++)
            {
                m_Stats[i].OnUpdate();
            }
        }


        /// <summary>
        /// Iterates over all the desired stated and checks to see if they are currently satisified.
        /// Apply any influencers that are needed and cache unsatisfied states are caches in `UnsatisfiedStates`.
        /// </summary>
        protected void UpdateDesiredStates()
        {
            List<StatInfluencerSO> influencers = new List<StatInfluencerSO>();

            UnsatisfiedDesiredStates.Clear();

            if (DesiredStates == null) return;

            for (int i = 0; i < DesiredStates.Length; i++)
            {
                if (DesiredStates[i].IsSatisfiedFor(this))
                {
                    influencers = DesiredStates[i].InfluencersToApplyWhenInDesiredState;
                    UpdatePossibleBehaviours(DesiredStates[i], true);
                }
                else
                {
                    influencers = DesiredStates[i].InfluencersToApplyWhenNotInDesiredState;
                    UnsatisfiedDesiredStates.Add(DesiredStates[i]);
                    UpdatePossibleBehaviours(DesiredStates[i], false);
                }

                for (int idx = 0; idx < influencers.Count; idx++)
                {
                    TryAddInfluencer(ScriptableObject.Instantiate(influencers[idx]));
                }
            }
        }

        /// <summary>
        /// Update the available behaviours based on the current satisfied or
        /// otherwise status of a DesiredState. Note that for the most part this will
        /// only be used in AI's and thus the default behaviour is to do nothing here.
        /// </summary>
        /// <param name="state">The state we are updating against</param>
        /// <param name="isSatisfied">whether or ot the state is satisfied.</param>
        protected virtual void UpdatePossibleBehaviours(StateSO state, bool isSatisfied) { }

        /// <summary>
        /// Get a list of stats that are currently outside the desired state for that stat.
        /// This can be used, for example. by AI deciding what action to take next.
        /// </summary>
        /// <returns>A list of stats that are not in a desired state.</returns>
        [Obsolete("This method needs to be replaced with one that identifies whether the stat is satisfied or needs to increase or decrease. Or perhaps it is not needed at all since it is currently only used in WanderWithIntent. Maybe that behaviour should look for places the brain has identified for it.")]
        public StatSO[] GetStatsNotInDesiredState()
        {
            List<StatSO> stats = new List<StatSO>();
            for (int i = 0; i < UnsatisfiedDesiredStates.Count; i++)
            {
                stats.AddRange(GetStatsDesiredForState(UnsatisfiedDesiredStates[i]));
            }
            return stats.ToArray();
        }

        private List<StatSO> GetStatsDesiredForState(StateSO state)
        {
            List<StatSO> stats = new List<StatSO>();
            if (state.statTemplate != null)
            {
                StatSO stat = GetOrCreateStat(state.statTemplate);
                if (GetGoalFor(state.statTemplate) != StateSO.Goal.NoAction)
                {
                    stats.Add(stat);
                }
            }

            for (int idx = 1; idx < state.SubStates.Length; idx++)
            {
                stats.AddRange(GetStatsDesiredForState(state.SubStates[idx]));
            }
            return stats;
        }

        /// <summary>
        /// Get the Stat of a given type.
        /// </summary>
        /// <param name="name">The name of the stat we want to retrieve</param>
        /// <returns>The stat, if it exists, or null.</returns>
        public StatSO GetStat(StatSO template)
        {
            for (int i = 0; i < m_Stats.Count; i++)
            {
                if (template != null)
                {
                    if (m_Stats[i].name == template.name)
                    {
                        return m_Stats[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Test if the stat tracker is currently tracking the stat provided in the template.
        /// </summary>
        /// <param name="statTemplate">The stat to test for</param>
        /// <returns>True if the stat is currently being tracked.</returns>
        internal bool HasStat(StatSO statTemplate)
        {
            return GetStat(statTemplate) != null;
        }

        /// <summary>
        /// Get the stat object representing a named stat. If it does not already
        /// exist it will be created with a base value.
        /// </summary>
        /// <param name="name">Tha name of the stat to Get or Create for this controller</param>
        /// <returns>A StatSO representing the named stat</returns>
        public StatSO GetOrCreateStat(StatSO template, float? value = null)
        {
            StatSO stat = GetStat(template);
            if (stat != null) return stat;

            stat = Instantiate(template);
            stat.name = template.name;
            if (value != null)
            {
                stat.NormalizedValue = (float)value;
            }

            m_Stats.Add(stat);
            return stat;
        }

        /// <summary>
        /// Add an influencer to this controller. If this controller is not managing the required stat then 
        /// do nothing. If we already added an influencer of this type within the cooldown time then
        /// do nothing.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        /// <returns>True if the influencer was added, otherwise false.</returns>
        public virtual bool TryAddInfluence(StatInfluence influence, float duration)
        {
            StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
            influencer.InteractionName = influence.statTemplate.name;
            influencer.Trigger = null;
            influencer.stat = influence.statTemplate;
            influencer.maxChange = influence.maxChange;
            influencer.duration = duration;
            influencer.CooldownDuration = influence.cooldownDuration;
            influencer.ResetInfluenceApplied = influence.resetInfluenceApplied;

            return TryAddInfluencer(influencer);
        }

        [Obsolete("User TryAddInfluence(StatInfluence influence, float duration) instead.")]
        public virtual bool TryAddInfluencer(StatInfluencerSO influencer)
        {
            if (influencer.ResetInfluenceApplied)
            {
                influencer.influenceApplied = 0;
            }

            // check that if an influencer already exists we are not in a cooldown period for this influencer
            for (int i = StatsInfluencers.Count -1; i >= 0; i--)
            {
                if (StatsInfluencers[i].InteractionName == influencer.InteractionName)
                {
                    if (Time.timeSinceLevelLoad > StatsInfluencers[i].CooldownCompleteTime)
                    {
                        StatsInfluencers.Add(influencer);
                        return true;
                    } else
                    {
                        // it already exists and we are in cooldown, don't add again.
                        return false;
                    }
                }
            }

            // If the influencer doesn't already exist add it
            StatsInfluencers.Add(influencer);
            return true;
        }

        internal List<StateSO> GetDesiredStatesFor(StatSO stat)
        {
            List<StateSO> states = new List<StateSO>();

            for (int i = 0; i < DesiredStates.Length; i++)
            {
                if (DesiredStates[i].statTemplate != null && stat.name == DesiredStates[i].statTemplate.name)
                {
                    states.Add(DesiredStates[i]);
                    break;
                }
            }

            return states;
        }

        /// <summary>
        /// Get the current goal for a given stat. That is do we currently want to 
        /// increase, decrease or maintaint his stat.
        /// If there are multiple desired states then an attempt is made to create 
        /// a meaningful goal. For example, if there are multiple greater than goals
        /// then the target will be the highest goal. 
        /// 
        /// If there are conflicting goals,
        /// such as a greater than and a less than then lessThan will take preference
        /// over greaterThan, but approximately will always be given prefernce.
        /// </summary>
        /// <returns>The current goal for the stat.</returns>
        public Goal GetGoalFor(StatSO stat)
        {
            float lessThan = float.MaxValue;
            float greaterThan = float.MinValue;

            List<StateSO> states = GetDesiredStatesFor(stat);
            for (int i = 0; i < states.Count; i++)
            {
                switch (states[i].objective)
                {
                    case Objective.LessThan:
                        if (stat.NormalizedValue >= states[i].normalizedTargetValue && states[i].normalizedTargetValue < lessThan)
                        {
                            lessThan = states[i].normalizedTargetValue;
                        }
                        break;

                    case Objective.Approximately:
                        if (Mathf.Approximately(stat.NormalizedValue, states[i].normalizedTargetValue)) {
                            if (stat.NormalizedValue > states[i].normalizedTargetValue)
                            {
                                return StateSO.Goal.Decrease;
                            } else
                            {
                                return StateSO.Goal.Increase;
                            }
                        }
                        break;

                    case Objective.GreaterThan:
                        if (stat.NormalizedValue <= states[i].normalizedTargetValue && states[i].normalizedTargetValue > greaterThan)
                        {
                            greaterThan = states[i].normalizedTargetValue;
                        }
                        break;
                }
            }

            if (lessThan != float.MaxValue) return StateSO.Goal.Decrease;
            if (greaterThan != float.MinValue) return StateSO.Goal.Increase;

            return StateSO.Goal.NoAction;
        }

        /// <summary>
        /// Generate a name for the owner of this stats tracker. This
        /// can be overridden for different kinds of stats trackers.
        /// By default it returns the name of the game object this
        /// component is attached to.
        /// </summary>
        /// <returns>A name for this stats tracker object to be used in the UI.</returns>
        internal virtual string GenerateName()
        {
            return transform.root.gameObject.name;
        }

#if UNITY_EDITOR
        string IDebug.StatusText()
        {
            string msg = DisplayName;
            msg += "\n\nStats";
            for (int i = 0; i < m_Stats.Count; i++)
            {
                msg += "\n" + m_Stats[i].statusDescription;
            }

            msg += GetActiveInfluencersDescription();

            msg += "\n\nUnsatisfied Desired States";
            if (UnsatisfiedDesiredStates.Count == 0) msg += "\nNone";
            for (int i = 0; i < UnsatisfiedDesiredStates.Count; i++)
            {
                StatSO stat = GetOrCreateStat(UnsatisfiedDesiredStates[i].statTemplate);
                msg += "\nIs not ";
                msg += UnsatisfiedDesiredStates[i].name + " ";
                msg += " (" + stat.name + " should be " + UnsatisfiedDesiredStates[i].objective + " " + UnsatisfiedDesiredStates[i].normalizedTargetValue + ")";
            }

            return msg;
        }

        internal string GetActiveInfluencersDescription()
        {
            StringBuilder msg = new StringBuilder("\n\nActive Influencers\n");
            if (StatsInfluencers.Count == 0) msg.AppendLine("None");
            for (int i = 0; i < StatsInfluencers.Count; i++)
            {
                msg.Append(StatsInfluencers[i].InteractionName);
                msg.Append(" at ");
                msg.Append(StatsInfluencers[i].GeneratorName);
                msg.AppendLine();
                msg.Append("\t - ");
                msg.Append(StatsInfluencers[i].stat.name);
                msg.Append(" changed by ");
                msg.Append(StatsInfluencers[i].maxChange);
                msg.Append(" at ");
                msg.Append(StatsInfluencers[i].changePerSecond);
                msg.Append(" per second for ");
                msg.Append(StatsInfluencers[i].duration);
                msg.Append(" seconds (");
                msg.Append(Mathf.Round((StatsInfluencers[i].influenceApplied / StatsInfluencers[i].maxChange) * 100));
                msg.AppendLine("% applied)");
            }

            return msg.ToString();
        }
#endif
    }
}
