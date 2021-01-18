using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WizardsCode.Character.Stats {
    /// <summary>
    /// The StatsController is responsible for tracking and reporting on the stats of the character.
    /// Stats are made up of a number of `StatsSO` objects.
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
                        if (m_StatsInfluencers[i].duration > 0)
                        {
                            float influence = m_StatsInfluencers[i].changePerSecond * (Time.timeSinceLevelLoad - m_TimeOfLastUpdate);
                            ChangeStat(m_StatsInfluencers[i], influence);
                        } else
                        {
                            ChangeStat(m_StatsInfluencers[i], m_StatsInfluencers[i].maxChange);
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
                StatSO stat = GetOrCreateStat(desiredStates[i].statName);
                if (stat.goal != DesiredState.Goal.NoAction)
                {
                    stats.Add(stat);
                }

            }
            return stats.ToArray();
        }

        /// <summary>
        /// Get a named stat. If it does not exist then return null.
        /// </summary>
        /// <param name="statName">The name of the stat to retrieve.</param>
        /// <returns>The stat, if it exists, or null.</returns>
        public StatSO GetStat(string statName)
        {
            // TODO cache results in a dictionary
            for (int i = 0; i < m_Stats.Count; i++)
            {
                if (m_Stats[i].name == statName)
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
        /// <param name="change">The change to make. The result is kept within the -100 to 100 range.</param>
        internal void ChangeStat(StatInfluencerSO influencer, float change)
        {
            StatSO stat = GetOrCreateStat(influencer.statName);
            stat.value += change;
            influencer.influenceApplied += change;
            //Debug.Log(gameObject.name + " changed stat " + influencer.statName + " by " + change);
        }

        /// <summary>
        /// Get the stat object representing a named stat. If it does not already
        /// exist it will be created with a base value.
        /// </summary>
        /// <param name="statName">Tha name of the stat to Get or Create</param>
        /// <param name="baseValue">The base value to assign if the stat needs to be created.</param>
        /// <returns>A StatSO representing the named stat</returns>
        public StatSO GetOrCreateStat(string statName, float baseValue = 0)
        {
            StatSO stat = GetStat(statName);
            if (stat != null) return stat;

            stat = ScriptableObject.CreateInstance<StatSO>();
            stat.name = statName;
            stat.value = baseValue;
            for (int i = 0; i < desiredStates.Length; i++)
            {
                if (statName == desiredStates[i].statName)
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
        /// do nothing. If this character has any memory of being influenced by the object within short term 
        /// memory this new influence will be rejected.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        /// <returns>True if the influencer was added, otherwise false.</returns>
        public bool TryAddInfluencer(StatInfluencerSO influencer)
        {
            StatSO stat = GetOrCreateStat(influencer.statName);
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
            StatSO[] stats = GetStatsNotInDesiredState();
            
            string msg = "";
            msg += "\n\nStats";
            for (int i = 0; i < stats.Length; i++)
            {
                msg += "\n" + stats[i].statusDescription;
            }

            return msg;
        }
#endif
    }
}