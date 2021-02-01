using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Stats;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// Place the StatsInfluencerTrigger on any game object with a trigger collider. When 
    /// another object with a StatsController attached triggers the collider the defined
    /// StatsInfluencer is attached to the StatsController. That StatsController will then
    /// apply the influence as defined within the StatsInfluencerSO.
    /// </summary>
    public class StatsInfluencerTrigger : MonoBehaviour
    {
        // TODO allow multiple influencers in each trigger
        [SerializeField, Tooltip("The Stat this influencer acts upon.")]
        StatSO m_Stat;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait, to the limit of the stats allowable value.")]
        float m_MaxChange;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The total change will occure over this time period. If duration is 0 then the total change is applied instantly")]
        float m_Duration = 0;
        [SerializeField, Tooltip("The cooldown time before a character can be influenced by this influencer again.")]
        float m_Cooldown = 30;
        [SerializeField, Tooltip("If the actor stays within the trigger area can they get a new influencer after the duration + cooldown has expired?")]
        bool m_IsRepeating = false;

        /// <summary>
        /// Test to see if this influencer trigger is on cooldown for a given actor.
        /// </summary>
        /// <param name="brain">The brain of the actor we are testing against</param>
        /// <returns>True if this influencer is on cooldown, meaning the actor cannot use it yet.</returns>
        internal bool IsOnCooldownFor(Brain brain)
        {
            float lastTime;
            if (m_TimeOfLastInfluence.TryGetValue(brain, out lastTime))
            {
                return lastTime + m_Cooldown > Time.timeSinceLevelLoad;
            } else
            {
                return false;
            }
        }

        private Dictionary<Brain, float> m_TimeOfLastInfluence = new Dictionary<Brain, float>();

        public StatSO Stat
        {
            get { return m_Stat; }
        }

        /// <summary>
        /// The time this influencer will operate.
        /// </summary>
        public float Duration { 
            get
            {
                return m_Duration;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == this.gameObject) return;

            Brain brain = other.GetComponentInParent<Brain>();

            if (brain == null || !brain.ShouldInteractWith(this)) return;
            AddInfluencer(brain);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!m_IsRepeating) return;

            if (other.gameObject == this.gameObject) return;

            Brain brain = other.GetComponentInParent<Brain>();

            if (brain == null || !brain.ShouldInteractWith(this)) return;

            if (IsOnCooldownFor(brain))
            {
                AddInfluencer(brain);
            }
        }

        private void AddInfluencer(Brain brain)
        {
            StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
            influencer.name = m_Stat.name + " influencer from " + name + " : " + GetInstanceID(); ;
            influencer.generator = gameObject;
            influencer.stat = m_Stat;
            influencer.maxChange = m_MaxChange;
            influencer.duration = m_Duration;
            influencer.cooldown = m_Cooldown;

            if (brain.TryAddInfluencer(influencer))
            {
                m_TimeOfLastInfluence.Remove(brain);
                m_TimeOfLastInfluence.Add(brain, Time.timeSinceLevelLoad);
            }
        }
    }
}