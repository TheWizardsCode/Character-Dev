using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// Place the StatsInfluencerTrigger on any game object with a trigger collider. When 
    /// another object with a StatsController attached triggers the collider the defined
    /// StatsInfluencer is attached to the StatsController.
    /// </summary>
    public class StatsInfluencerTrigger : MonoBehaviour
    {
        // TODO all multiple influencers in each trigger
        [SerializeField, Tooltip("The Stat name this influencer acts upon.")]
        string m_StatName;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait, to the limit of the stats allowable value.")]
        float m_MaxChange;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The total change will occure over this time period. If duration is 0 then the total change is applied instantly")]
        float m_Duration = 0;
        [SerializeField, Tooltip("The cooldown time before a character can be influenced by this influencer again.")]
        float m_Cooldown = 30;

        private void OnTriggerEnter(Collider other)
        {
            StatsController controller = other.GetComponent<StatsController>();
            if (controller != null)
            {
                StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                influencer.name = m_StatName + " influencer from " + name + " : " + GetInstanceID(); ;
                influencer.generator = gameObject;
                influencer.statName = m_StatName;
                influencer.maxChange = m_MaxChange;
                influencer.duration = m_Duration;
                influencer.cooldown = m_Cooldown;

                controller.TryAddInfluencer(influencer);
            }
        }
    }
}