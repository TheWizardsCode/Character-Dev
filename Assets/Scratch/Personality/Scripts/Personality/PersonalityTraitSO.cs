using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Personality
{
    /// <summary>
    /// A Personality trait tracks the base value and current value of a personality trait. This can be used by an AI
    /// system to decide on actions to take.
    /// </summary>
    public class PersonalityTraitSO : ScriptableObject
    {
        [Header("Details")]
        [SerializeField, Tooltip("The human readable name for this trait.")]
        string displayName = "No Name Trait";
        [SerializeField, Tooltip("The base value for this trait. This is the value that the character will always trend towards with no external factors influencing the current value."), Range(-100, 100)]
        float m_BaseValue = 0;

        [HideInInspector, SerializeField]
        float m_CurrentValue;

        /// <summary>
        /// Set the current value of this trait. If an attempt is made to set the value 
        /// outside the allowable range (-100 to 100) then the value will
        /// be adjusted to fit this range.
        /// </summary>
        public float value {
            get { return m_CurrentValue; }
            internal set
            {
                m_CurrentValue = Mathf.Clamp(value, -100, 100);
            }
        }

        private void Awake()
        {
            m_CurrentValue = m_BaseValue;
        }

        /// <summary>
        /// Called by the PersonalityController to update the trait based on current conditions.
        /// </summary>
        internal virtual void OnUpdate()
        {
            Debug.Log("Trait \"" + name + "\" has value of " + value);
        }
    }
}
