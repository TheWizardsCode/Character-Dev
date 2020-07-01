using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// A stats influencer will alter a Stat by a given amount over a given duration.
    /// </summary>
    public class StatInfluencerSO : ScriptableObject
    {
        [SerializeField, Tooltip("The object that generates this influence.")]
        GameObject m_Generator;
        [SerializeField, Tooltip("The name of the Stat this influencer acts upon.")]
        string m_StatName;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the stat. If the stat will never be taken beyond its maximum and minimum allowable values.")]
        float m_MaxChange;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The change will occur over this time period, up to the limit of the stat or the maxChange of this influencer. If duration is 0 then the total change is applied instantly.")]
        float m_Duration = 0;
        [SerializeField, Tooltip("The cooldown period before a character can be influenced by this object again, in seconds.")]
        float m_Cooldown = 5;

        [HideInInspector, SerializeField]
        float m_InfluenceApplied = 0;

        float m_ChangePerSecond = float.NegativeInfinity;

        public GameObject generator
        {
            get { return m_Generator; }
            set { m_Generator = value; }
        }

        /// <summary>
        /// The name of the stat that this influencer will act upon.
        /// </summary>
        public string statName
        {
            get { return m_StatName; }
            set { m_StatName = value; }
        }

        /// <summary>
        /// The maximum amount of change this influencer will impart upon the stat. If the stat will never be taken beyond its maximum and minimum allowable values.
        /// </summary>
        public float maxChange
        {
            get { return m_MaxChange; }
            set { 
                m_MaxChange = value;
                if (m_Duration > 0)
                {
                    m_ChangePerSecond = m_MaxChange / m_Duration;
                }
            }
        }

        /// <summary>
        /// The minimum time that must pass before a character can be influenced by this same influencer again.
        /// </summary>
        public float cooldown
        {
            get { return m_Cooldown; }
            set { m_Cooldown = value; }
        }

        /// <summary>
        /// The change this influencer is making per second of game time.
        /// </summary>
        public float changePerSecond
        {
            get {
                return m_ChangePerSecond; 
            }
        }

        /// <summary>
        /// The influence applied by this infliencer to date. When this reaches
        /// the maxChange value the object will be destroyed.
        /// </summary>
        public float influenceApplied
        {
            get { return m_InfluenceApplied; }
            set
            {
                if (Mathf.Abs(value) < Mathf.Abs(maxChange))
                {
                    m_InfluenceApplied = value;
                } else
                {
                    Destroy(this);
                }
            }
        }

        /// <summary>
        /// The time, in seconds, over which the influencer will be effective. 
        /// The change will occur over this time period, up to the limit of the stat or the maxChange of this influencer. 
        /// If duration is 0 then the total change is applied instantly.
        /// </summary>
        public float duration
        {
            get { return m_Duration; }
            set { 
                m_Duration = value;
                m_ChangePerSecond = m_MaxChange / m_Duration;
            }
        }

        internal StatsController controller { get; set; }
    }
}