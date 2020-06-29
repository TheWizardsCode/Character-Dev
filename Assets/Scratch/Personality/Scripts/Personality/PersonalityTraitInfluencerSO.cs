using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Personality
{
    /// <summary>
    /// A personality trait influencer will alter a Personality Trait by a given amount over a given duration.
    /// </summary>
    public class PersonalityTraitInfluencerSO : ScriptableObject
    {
        [SerializeField, Tooltip("The name of the PersonalityTrait this influencer acts upon.")]
        string m_TraitName;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait. If the trait will never be taken beyond its maximum and minimum allowable values.")]
        float m_MaxChange;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The change will occur over this time period, up to the limit of the trait or the maxChange of this influencer. If duration is 0 then the total change is applied instantly.")]
        float m_Duration = 0;

        [HideInInspector, SerializeField]
        float m_InfluenceApplied = 0;

        float m_ChangePerSecond = float.NegativeInfinity;

        /// <summary>
        /// The name of the trait that this influencer will act upon.
        /// </summary>
        public string traitName
        {
            get { return m_TraitName; }
            internal set { m_TraitName = value; }
        }

        /// <summary>
        /// The maximum amount of change this influencer will impart upon the trait. If the trait will never be taken beyond its maximum and minimum allowable values.
        /// </summary>
        public float maxChange
        {
            get { return m_MaxChange; }
            internal set { 
                m_MaxChange = value;
                m_ChangePerSecond = m_MaxChange / m_Duration;
            }
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
        /// The change will occur over this time period, up to the limit of the trait or the maxChange of this influencer. 
        /// If duration is 0 then the total change is applied instantly.
        /// </summary>
        public float duration
        {
            get { return m_Duration; }
            internal set { 
                m_Duration = value;
                m_ChangePerSecond = m_MaxChange / m_Duration;
            }
        }

        internal PersonalityController controller { get; set; }
    }
}