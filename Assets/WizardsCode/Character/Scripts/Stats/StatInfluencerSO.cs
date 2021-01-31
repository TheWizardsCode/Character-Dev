using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Stats
{
    /// <summary>
    /// A stats influencer is used to alter a Stat by a given amount over a given duration.
    /// 
    /// </summary>
    [CreateAssetMenu(fileName ="New Stats Influencer", menuName = "Wizards Code/Stats/New Influencer")]
    public class StatInfluencerSO : ScriptableObject
    {
        [SerializeField, Tooltip("The Stat this influencer acts upon.")]
        StatSO m_stat;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the stat. If the stat will never be taken beyond its maximum and minimum allowable values.")]
        float m_MaxChange = 10;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The change will occur over this time period, up to the limit of the stat or the maxChange of this influencer. If duration is 0 then the total change is applied instantly.")]
        float m_Duration = 0;
        [SerializeField, Tooltip("The cooldown period before a character can be influenced by this object again, in seconds.")]
        float m_Cooldown = 5;

        [HideInInspector, SerializeField]
        float m_InfluenceApplied = 0;
        GameObject m_Generator;

        float m_ChangePerSecond = float.NegativeInfinity;
        private float m_TimeOfLastUpdate;

        /// <summary>
        /// Get the game object that imparted this influencer on the actor.
        /// This is used in the memory system to remember good/bad results of interations with objects.
        /// </summary>
        public GameObject generator
        {
            get { return m_Generator; }
            set { m_Generator = value; }
        }

        /// <summary>
        /// The stat that this influencer will act upon.
        /// </summary>
        public StatSO stat
        {
            get { return m_stat; }
            set { m_stat = value; }
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
                } else
                {
                    m_ChangePerSecond = 0;
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

        private void Awake()
        {
            m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
        }

        /// <summary>
        /// The influence applied by this influencer to date.
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
                    m_InfluenceApplied = maxChange;
                }
            }
        }

        /// <summary>
        /// Apply a change from a stat influencer.
        /// </summary>
        /// 
        /// <param name="brain">The brain managing the stats to be changed.</param>
        internal void ChangeStat(Brain brain)
        {
            StatSO statToUpdate = brain.GetOrCreateStat(stat);
            float change;

            if (duration > 0)
            {
                change = Mathf.Clamp(changePerSecond * (Time.timeSinceLevelLoad - m_TimeOfLastUpdate), float.MinValue, Mathf.Abs(maxChange) - Mathf.Abs(influenceApplied));
            }
            else
            {
                change = Mathf.Clamp(maxChange, maxChange - influenceApplied, Mathf.Abs(maxChange));
            }

            statToUpdate.Value += change;
            influenceApplied += change;

            m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
            //Debug.Log(gameObject.name + " changed stat " + influencer.statName + " by " + change);
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
                if (duration > 0)
                {
                    m_ChangePerSecond = m_MaxChange / m_Duration;
                } else
                {
                    m_ChangePerSecond = 0;
                }
            }
        }

        internal Brain controller { get; set; }
    }
}