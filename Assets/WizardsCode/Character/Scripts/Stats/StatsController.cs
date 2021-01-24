using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character;

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
        DesiredState[] desiredStates = new DesiredState[0];

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
            for (int i = 0; i < desiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(desiredStates[i].stat.name);
                if (stat.goal != DesiredState.Goal.NoAction)
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

            stat.value += change;
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
            stat.value = value;
            for (int i = 0; i < desiredStates.Length; i++)
            {
                if (stat.GetType() == desiredStates[i].stat.GetType())
                {
                    stat.desiredState = desiredStates[i];
                    break;
                }
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

            StatSO stat = GetOrCreateStat(influencer.stat.name);
            bool isGood = true;
            switch (stat.desiredState.objective)
            {
                case DesiredState.Objective.LessThan:
                    if (influencer.maxChange > 0)
                    {
                        isGood = false;
                    }
                    break;
                case DesiredState.Objective.Approximately:
                    float currentDelta = stat.desiredState.targetValue - stat.value;
                    float influencedDelta = stat.desiredState.targetValue - (stat.value + influencer.maxChange);
                    if (currentDelta < influencedDelta)
                    {
                        isGood = false;
                    }
                    break;
                case DesiredState.Objective.GreaterThan:
                    if (influencer.maxChange < 0)
                    {
                        isGood = false;
                    }
                    break;
            }

            if (m_Memory != null) {
                m_Memory.AddMemory(influencer, isGood);
            }

            m_StatsInfluencers.Add(influencer);

            return true;
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
                msg += "\n" + m_StatsInfluencers[i].stat + " changed by " + m_StatsInfluencers[i].maxChange + " at " + m_StatsInfluencers[i].changePerSecond + " per second (" + Mathf.Round((m_StatsInfluencers[i].influenceApplied / m_StatsInfluencers[i].maxChange) * 100) + "% applied)";
            }

            return msg;
        }
#endif
    }
}