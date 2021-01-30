using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WizardsCode.Character;

namespace WizardsCode.Stats
{
    /// <summary>
    /// A StatSO tracks the base value and current value of a Stat. This can be used by an AI
    /// system to decide on actions to take.
    /// </summary>
    [CreateAssetMenu(fileName = "New Stat", menuName = "Wizards Code/Stats/New Stat")]
    public class StatSO : ScriptableObject
    {
        [Header("Details")]
        [SerializeField, Tooltip("The human readable name for this stat.")]
        string m_displayName = "No Name Stat";
        [SerializeField, Tooltip("The minimum value this stat can have.")]
        float minValue = 0;
        [SerializeField, Tooltip("The maximum value this stat can have.")]
        float maxValue = 100;
        [SerializeField, Tooltip("The base value for this stat. This is the value that the character will always trend towards with no external factors influencing the current value."), Range(0, 1)]
        float m_BaseNormalizedValue = 0;

        [HideInInspector, SerializeField]
        float m_CurrentNormalizedValue;

        public StatChangedEvent onValueChanged = new StatChangedEvent();

        /// <summary>
        /// Get a human readable description of the current status of this stat.
        /// That is, it's value, whether it is wihtin the desired range etc.
        /// </summary>
        public string statusDescription
        {
            get {
                string msg = name + " is " + normalizedValue;
                return msg; 
            }
        }

        /// <summary>
        /// Called every tick to allow for the state to be updated over time.
        /// </summary>
        internal virtual void OnUpdate()
        {
            // Do nothing by default
        }

        /// <summary>
        /// Set the current value of this stat. If an attempt is made to set the value 
        /// outside the allowable range (0 to 1) then the value will
        /// be clamped.
        /// </summary>
        public float normalizedValue {
            get { return m_CurrentNormalizedValue; }
            internal set
            {
                if (m_CurrentNormalizedValue != value)
                {
                    float old = m_CurrentNormalizedValue;
                    m_CurrentNormalizedValue = Mathf.Clamp01(value);
                    if (onValueChanged != null) onValueChanged.Invoke(m_CurrentNormalizedValue - old);
                }
            }
        }

        /// <summary>
        /// Set the current absolute value of this stat. If an attempt is set te value above or below the allowable min/max
        /// the value will be clamped.
        /// </summary>
        public float value
        {
            get { return (maxValue - minValue) * normalizedValue; }
            set { normalizedValue = (value - maxValue) / (maxValue - minValue); }
        }

        private void Awake()
        {
            m_CurrentNormalizedValue = m_BaseNormalizedValue;
        }
    }

    /// <summary>
    /// An event notifying that the stat has changed and by how much (normalized).
    /// </summary>
    public class StatChangedEvent : UnityEvent<float>
    {
    }
}
