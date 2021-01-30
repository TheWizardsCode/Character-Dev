using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Stats {
    /// <summary>
    /// The StatsController is responsible for tracking and reporting on the stats of the character.
    /// Stats are made up of a number of `StatsSO` objects and can be influenced by a collection of
    /// StatsInfluencerSO's.
    /// </summary>
    public class StatsController : MonoBehaviour
#if UNITY_EDITOR
        , IDebug
#endif
    {
        [SerializeField, Tooltip("The desired states for our stats.")]
        StateSO[] m_DesiredStates = new StateSO[0];

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

        public StateSO[] desiredStates
        {
            get { return m_DesiredStates; }
        }

        private void Awake()
        {
            m_Memory = GetComponent<MemoryController>();
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
                        ChangeStat(m_StatsInfluencers[i]);

                        if (m_StatsInfluencers[i].influenceApplied >= m_StatsInfluencers[i].maxChange)
                        {
                            m_StatsInfluencers.RemoveAt(i);
                        }
                    } else
                    {
                        m_StatsInfluencers.RemoveAt(i);
                    }
                }

                m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
                m_TimeOfNextUpdate = Time.timeSinceLevelLoad + m_TimeBetweenUpdates;
            }
        }

        /// <summary>
        /// Get a list of stats that are currently outside the desired state for that stat.
        /// This can be used, for example. by AI deciding what action to take next.
        /// </summary>
        /// <returns>A list of stats that are not in a desired state.</returns>
        public StatSO[] GetStatsNotInDesiredState()
        {
            List<StatSO> stats = new List<StatSO>();
            for (int i = 0; i < m_DesiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(m_DesiredStates[i].statTemplate.name);
                if (GetGoalFor(m_DesiredStates[i].statTemplate) != StateSO.Goal.NoAction)
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
        public StatSO GetStat(string name)
        {
            for (int i = 0; i < m_Stats.Count; i++)
            {
                if (m_Stats[i].name == name)
                {
                    return m_Stats[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Apply an immediate change to a given Stats, if this controller is tracking that stat.
        /// </summary>
        /// 
        /// <param name="influencer">The influencer imparting the change.</param>
        internal void ChangeStat(StatInfluencerSO influencer)
        {
            StatSO stat = GetOrCreateStat(influencer.stat.name);
            float change; 

            if (influencer.duration > 0)
            {
                change = Mathf.Clamp(influencer.changePerSecond * (Time.timeSinceLevelLoad - m_TimeOfLastUpdate), float.MinValue, influencer.maxChange - influencer.influenceApplied);
            }
            else
            {
                change = Mathf.Clamp(influencer.maxChange, influencer.maxChange - influencer.influenceApplied, influencer.maxChange);
            }

            stat.normalizedValue += change;
            influencer.influenceApplied += change;
            //Debug.Log(gameObject.name + " changed stat " + influencer.statName + " by " + change);
        }

        /// <summary>
        /// Get the stat object representing a named stat. If it does not already
        /// exist it will be created with a base value.
        /// </summary>
        /// <param name="name">Tha name of the stat to Get or Create for this controller</param>
        /// <returns>A StatSO representing the named stat</returns>
        public StatSO GetOrCreateStat(string name, float value = 0)
        {
            StatSO stat = GetStat(name);
            if (stat != null) return stat;

            stat = ScriptableObject.CreateInstance<StatSO>();
            stat.name = name;
            stat.normalizedValue = value;

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

            StatSO stat = GetOrCreateStat(influencer.stat.name);
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
                        float currentDelta = states[i].targetValue - stat.normalizedValue;
                        float influencedDelta = states[i].targetValue - (stat.normalizedValue + influencer.maxChange);
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

                if (m_Memory != null)
                {
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
                        if (stat.normalizedValue >= states[i].targetValue && states[i].targetValue < lessThan)
                        {
                            lessThan = states[i].targetValue;
                        }
                        break;

                    case Objective.Approximately:
                        if (stat.normalizedValue > states[i].targetValue * 1.1)
                        {
                            return StateSO.Goal.Decrease;
                        }
                        else
                        {
                            if (stat.normalizedValue < states[i].targetValue * 0.9)
                            {
                                return StateSO.Goal.Increase;
                            }
                        }
                        break;

                    case Objective.GreaterThan:
                        if (stat.normalizedValue <= states[i].targetValue && states[i].targetValue > greaterThan)
                        {
                            greaterThan = states[i].targetValue;
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
            for (int i = 0; i < m_StatsInfluencers.Count; i++)
            {
                msg += "\n" + m_StatsInfluencers[i].stat.name + " changed by " + m_StatsInfluencers[i].maxChange + " at " + m_StatsInfluencers[i].changePerSecond + " per second (" + Mathf.Round((m_StatsInfluencers[i].influenceApplied / m_StatsInfluencers[i].maxChange) * 100) + "% applied)";
            }

            msg += "\n\nDesired States";
            for (int i = 0; i < m_DesiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(m_DesiredStates[i].statTemplate.name);
                msg += GetGoalFor(stat) == Goal.NoAction ? "\nIs " : "\nIs not ";
                msg += m_DesiredStates[i].name + " ";
                msg += " (" + stat.name + " should be " + m_DesiredStates[i].objective + " " + m_DesiredStates[i].targetValue + ")";
            }

            return msg;
        }
#endif
    }
}
