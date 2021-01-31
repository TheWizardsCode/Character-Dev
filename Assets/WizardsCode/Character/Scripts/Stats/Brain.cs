using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character;
using WizardsCode.Character.Stats;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Stats {
    /// <summary>
    /// The Brain is responsible for tracking the stats and goal states of the character and
    /// making decisions and plans to reach those goal stats.
    /// </summary>
    public class Brain : MonoBehaviour
#if UNITY_EDITOR
        , IDebug
#endif
    {
        [SerializeField, Tooltip("The desired states for our stats.")]
        StateSO[] m_DesiredStates = new StateSO[0];
        [SerializeField, Tooltip("The default behaviour to execute if no other behaviour is selected.")]
        AbstractAIBehaviour m_DefaultBehaviour;

        [Header("Optimization")]
        [SerializeField, Tooltip("How often stats should be processed for changes.")]
        float m_TimeBetweenUpdates = 0.5f;

        [HideInInspector, SerializeField]
        List<StatSO> m_Stats = new List<StatSO>();
        [HideInInspector, SerializeField]
        List<StatInfluencerSO> m_StatsInfluencers = new List<StatInfluencerSO>();

        float m_TimeOfLastUpdate = 0;
        float m_TimeOfNextUpdate = 0;
        MemoryController m_Memory;
        ActorController m_Controller;
        AbstractAIBehaviour[] m_Behaviours = default;
        private AbstractAIBehaviour m_CurrentBehaviour;
        private Interactable m_TargetInteractable;

        public StateSO[] desiredStates
        {
            get { return m_DesiredStates; }
        }
        public StateSO[] UnsatisfiedDesiredStates { get; internal set; }
        internal Interactable TargetInteractable
        {
            get { return m_TargetInteractable; }
            set
            {
                if (m_Controller != null
                    && value != null)
                {
                    //TODO move to an interaction point not to the transform position
                    m_Controller.TargetPosition = value.transform.position;
                }

                m_TargetInteractable = value;
            }
        }

        private void Awake()
        {
            m_Memory = GetComponent<MemoryController>();
            m_Controller = GetComponent<ActorController>();
            m_Behaviours = GetComponentsInChildren<AbstractAIBehaviour>();
        }

        /// <summary>
        /// Decide whether the actor should interact with an influencer trigger they just entered.
        /// </summary>
        /// <param name="statsInfluencerTrigger">The influencer trigger that was activated and can now be interacted with.</param>
        /// <returns></returns>
        internal bool ShouldInteractWith(StatsInfluencerTrigger statsInfluencerTrigger)
        {
            Interactable interactable = statsInfluencerTrigger.gameObject.GetComponent<Interactable>();
            if (interactable != null && GameObject.ReferenceEquals(interactable, TargetInteractable))
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad >= m_TimeOfNextUpdate)
            {
                for (int i = 0; i < m_Stats.Count; i++)
                {
                    m_Stats[i].OnUpdate();
                }

                for (int i = 0; i < m_StatsInfluencers.Count; i++)
                {
                    if (m_StatsInfluencers[i] != null)
                    {
                        m_StatsInfluencers[i].ChangeStat(this);

                        if (Mathf.Abs(m_StatsInfluencers[i].influenceApplied) >= Mathf.Abs(m_StatsInfluencers[i].maxChange))
                        {
                            m_StatsInfluencers.RemoveAt(i);
                        }
                    } else
                    {
                        m_StatsInfluencers.RemoveAt(i);
                    }
                }

                UpdateUnsatisfiedStates();

                UpdateActiveBehaviour();

                m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
                m_TimeOfNextUpdate = Time.timeSinceLevelLoad + m_TimeBetweenUpdates;
            }
        }

        /// <summary>
        /// Iterates over all the behaviours available to this actor and picks the most important one to be executed next.
        /// </summary>
        private void UpdateActiveBehaviour()
        {
            //TODO Allow tasks to be interuptable
            if (m_CurrentBehaviour != null && m_CurrentBehaviour.IsExecuting) return;
            
            AbstractAIBehaviour candidateBehaviour = m_DefaultBehaviour;
            float highestWeight = 0;
            float currentWeight = 0;
            for (int i = 0; i < m_Behaviours.Length; i++)
            {
                if (m_Behaviours[i].IsAvailable)
                {
                    currentWeight = m_Behaviours[i].Weight(this);
                    if (currentWeight > highestWeight)
                    {
                        candidateBehaviour = m_Behaviours[i];
                        highestWeight = currentWeight;
                    }
                }
            }

            if (candidateBehaviour == null) return;

            m_CurrentBehaviour = candidateBehaviour;
            m_CurrentBehaviour.IsExecuting = true;
        }

        /// <summary>
        /// Iterates over all the desired stated and checks to see if they are currently satsified.
        /// Unsatisfied states are caches in `UnsatisfiedStates`.
        /// </summary>
        private void UpdateUnsatisfiedStates()
        {
            List<StateSO> states = new List<StateSO>();
            for (int i = 0; i < m_DesiredStates.Length; i++)
            {
                if (!m_DesiredStates[i].IsSatisfiedFor(this))
                {
                    states.Add(m_DesiredStates[i]);
                }
            }
            UnsatisfiedDesiredStates = states.ToArray();
        }

        /// <summary>
        /// Get a list of stats that are currently outside the desired state for that stat.
        /// This can be used, for example. by AI deciding what action to take next.
        /// </summary>
        /// <returns>A list of stats that are not in a desired state.</returns>
        [Obsolete("This method needs to be replaced with one that identifies whether the stat is to increase or decrease. Or perhaps it is not needed at all since it is currently only used in WanderWithIntent. Maybe that behaviour should look for places the brain has identified for it.")]
        public StatSO[] GetStatsNotInDesiredState()
        {
            List<StatSO> stats = new List<StatSO>();
            for (int i = 0; i < UnsatisfiedDesiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(UnsatisfiedDesiredStates[i].statTemplate);
                if (GetGoalFor(UnsatisfiedDesiredStates[i].statTemplate) != StateSO.Goal.NoAction)
                {
                    stats.Add(stat);
                }
            }
            return stats.ToArray();
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
                if (m_Stats[i].name == template.name)
                {
                    return m_Stats[i];
                }
            }
            return null;
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
        /// do nothing.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        /// <returns>True if the influencer was added, otherwise false.</returns>
        public bool TryAddInfluencer(StatInfluencerSO influencer)
        {
            if (m_Memory != null && influencer.generator != null)
            {
                MemorySO[] memories = m_Memory.GetAllMemoriesAbout(influencer.generator);
                for (int i = 0; i < memories.Length; i++)
                {
                    if (memories[i].stat == influencer.stat && memories[i].time + memories[i].cooldown > Time.timeSinceLevelLoad)
                    {
                        return false;
                    }
                }
            }

            if (m_Memory != null)
            {
                StatSO stat = GetOrCreateStat(influencer.stat);
                List<StateSO> states = GetStatesFor(stat);
                bool isGood = false;
                for (int i = 0; i < states.Count; i++)
                {
                    switch (states[i].objective)
                    {
                        case StateSO.Objective.LessThan:
                            if (influencer.maxChange > 0)
                            {
                                isGood = false;
                            }
                            break;
                        case StateSO.Objective.Approximately:
                            float currentDelta = states[i].normalizedTargetValue - stat.NormalizedValue;
                            float influencedDelta = states[i].normalizedTargetValue - (stat.NormalizedValue + influencer.maxChange);
                            if (currentDelta < influencedDelta)
                            {
                                isGood = false;
                            }
                            break;
                        case StateSO.Objective.GreaterThan:
                            if (influencer.maxChange < 0)
                            {
                                isGood = false;
                            }
                            break;
                    }

                    m_Memory.AddMemory(influencer, isGood);
                }
            }

            m_StatsInfluencers.Add(influencer);

            return true;
        }

        private List<StateSO> GetStatesFor(StatSO stat)
        {
            List<StateSO> states = new List<StateSO>();

            for (int i = 0; i < m_DesiredStates.Length; i++)
            {
                if (stat.GetType() == m_DesiredStates[i].statTemplate.GetType())
                {
                    states.Add(m_DesiredStates[i]);
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

            List<StateSO> states = GetStatesFor(stat);
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

#if UNITY_EDITOR
        string IDebug.StatusText()
        {
            string msg = "";
            msg += "\n\nStats";
            for (int i = 0; i < m_Stats.Count; i++)
            {
                msg += "\n" + m_Stats[i].statusDescription;
            }

            msg += "\n\nActive Influencers";
            if (m_StatsInfluencers.Count == 0) msg += "\nNone";
            for (int i = 0; i < m_StatsInfluencers.Count; i++)
            {
                msg += "\n" + m_StatsInfluencers[i].stat.name + " changed by " + m_StatsInfluencers[i].maxChange + " at " + m_StatsInfluencers[i].changePerSecond + " per second (" + Mathf.Round((m_StatsInfluencers[i].influenceApplied / m_StatsInfluencers[i].maxChange) * 100) + "% applied)";
            }

            msg += "\n\nUnsatisfied Desired States";
            if (UnsatisfiedDesiredStates.Length == 0) msg += "\nNone";
            for (int i = 0; i < UnsatisfiedDesiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(m_DesiredStates[i].statTemplate);
                msg += GetGoalFor(stat) == Goal.NoAction ? "\nIs " : "\nIs not ";
                msg += m_DesiredStates[i].name + " ";
                msg += " (" + stat.name + " should be " + m_DesiredStates[i].objective + " " + m_DesiredStates[i].normalizedTargetValue + ")";
            }

            msg += "\n\nCurrent Behaviour";
            msg += "\n" + m_CurrentBehaviour;

            return msg;
        }
#endif
    }
}
