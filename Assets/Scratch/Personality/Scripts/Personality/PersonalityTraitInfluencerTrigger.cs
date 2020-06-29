using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Personality
{
    /// <summary>
    /// Place the PersonalityTraitInfluencerTrigger on any game object with a trigger collider. When 
    /// another object with a PersonalityController attached triggers the collider the defined
    /// PersonalityTraitInfluencer is enacted (if instant effect) or attached to the PersonalityController
    /// (if the influence is imparted over time).
    /// </summary>
    public class PersonalityTraitInfluencerTrigger : MonoBehaviour
    {
        // TODO all multiple influencers in each trigger
        [SerializeField, Tooltip("The PersonalityTrait name this influencer acts upon.")]
        string m_TraitName;
        [SerializeField, Tooltip("The maximum amount of change this influencer will impart upon the trait, to the limit of the traits allowable value.")]
        float m_MaxChange;
        [SerializeField, Tooltip("The time, in seconds, over which the influencer will be effective. The total change will occure over this time period. If duration is 0 then the total change is applied instantly")]
        float m_Duration = 0;

        private void OnTriggerEnter(Collider other)
        {
            PersonalityController personality = other.GetComponent<PersonalityController>();
            if (personality != null)
            {
                if (m_Duration == 0)
                {
                    personality.ChangeTrait(m_TraitName, m_MaxChange);
                } else
                {
                    PersonalityTraitInfluencerSO influencer = ScriptableObject.CreateInstance<PersonalityTraitInfluencerSO>();
                    influencer.name = m_TraitName + " influencer from " + name + " : " + GetInstanceID(); ;
                    influencer.traitName = m_TraitName;
                    influencer.maxChange = m_MaxChange;
                    influencer.duration = m_Duration;

                    personality.TryAddInfluencer(influencer);
                }
            }
        }
    }
}