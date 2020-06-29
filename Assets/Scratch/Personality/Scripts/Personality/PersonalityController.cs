using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Personality {
    /// <summary>
    /// The PersonalityController is responsible for tracking and reporting on the personality of the character.
    /// Personality is made up of a number of `PersonalityTraits`.
    /// </summary>
    public class PersonalityController : MonoBehaviour
    {
        [Header("Traits")]
        [SerializeField, Tooltip("The names of personality traits this controller is tracking.")]
        string[] m_TraitNames;

        [Header("Optiomiation")]
        [SerializeField, Tooltip("How often traits should be processed for changes.")]
        float m_TimeBetweenUpdates = 0.5f;

        [HideInInspector, SerializeField]
        List<PersonalityTraitSO> m_Traits = new List<PersonalityTraitSO>();
        [HideInInspector, SerializeField]
        List<PersonalityTraitInfluencerSO> m_TraitInfluencers = new List<PersonalityTraitInfluencerSO>();

        float m_TimeOfLastUpdate = 0;
        float m_TimeOfNextUpdate = 0;

        private void Awake()
        {
            PersonalityTraitSO trait;
            for (int i = 0; i < m_TraitNames.Length; i++)
            {
                if (!TryGetTrait(m_TraitNames[i], out trait))
                {
                    trait = ScriptableObject.CreateInstance<PersonalityTraitSO>();
                    trait.name = m_TraitNames[i];
                    m_Traits.Add(trait);
                }
            }
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad >= m_TimeOfNextUpdate)
            {
                for (int i = 0; i < m_Traits.Count; i++)
                {
                    m_Traits[i].OnUpdate();
                }

                for (int i = 0; i < m_TraitInfluencers.Count; i++)
                {
                    if (m_TraitInfluencers[i] != null)
                    {
                        float influence = m_TraitInfluencers[i].changePerSecond * (Time.timeSinceLevelLoad - m_TimeOfLastUpdate);
                        ChangeTrait(m_TraitInfluencers[i].traitName, influence);
                        m_TraitInfluencers[i].influenceApplied += influence;
                    } else
                    {
                        m_TraitInfluencers.RemoveAt(i);
                    }
                }

                m_TimeOfLastUpdate = Time.timeSinceLevelLoad;
                m_TimeOfNextUpdate = Time.timeSinceLevelLoad + m_TimeBetweenUpdates;
            }
        }

        /// <summary>
        /// Apply an immediate change to a given PersonalityTrait, if this controller is tracking that trait.
        /// </summary>
        /// 
        /// <param name="traitname">The name of the trait to change.</param>
        /// <param name="change">The change to make. The result is kept within the -100 to 100 range.</param>
        internal void ChangeTrait(string traitName, float change)
        {
            PersonalityTraitSO trait;
            if (TryGetTrait(traitName, out trait))
            {
                trait.value += change;
                Debug.Log(gameObject.name + " changed trait " + traitName + " by " + change);
            }
        }

        private bool TryGetTrait(string traitName, out PersonalityTraitSO trait)
        {
            // TODO cache results in a dictionary
            for (int i = 0; i < m_Traits.Count; i++)
            {
                if (m_Traits[i].name == traitName)
                {
                    trait = m_Traits[i];
                    return true;
                }
            }

            trait = null;
            return false;
        }

        /// <summary>
        /// Add an influencer to this controller. If this controller is not managing the require trait then 
        /// do nothing.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        internal void TryAddInfluencer(PersonalityTraitInfluencerSO influencer)
        {
            PersonalityTraitSO trait;
            if (TryGetTrait(influencer.traitName, out trait))
            {
                m_TraitInfluencers.Add(influencer);
            }
        }
    }
}