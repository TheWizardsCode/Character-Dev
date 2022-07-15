using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using UnityEngine.AI;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// The HealthController provides an interface for managing health related statistics
    /// and effects.
    /// </summary>
    public class HealthController : MonoBehaviour
    {
        [Header("Stats Controller Config")]
        [SerializeField, Tooltip("A template used to create the main health statistic.")]
        StatSO healthTemplate;

        [Header("Animations")]
        [SerializeField, Tooltip("The animator to use.")]
        Animator m_Animator;
        [SerializeField, Tooltip("A trigger parameter used to start the death animation.")]
        string m_DeathTriggerName = "Die";

        protected StatSO m_Health;
        StatsTracker m_Controller;
        int deathTriggerID;

        public bool IsAlive {  
            get { 
                // TODO: Remove this hacky fix for a race condition in which this gets called in Neo FPS BaseCharacter OnEnable
                return m_Health == null ? true : m_Health.Value > 0; 
            } 
        }
        public float Health { 
            get { return m_Health.Value; } 
            set { m_Health.Value = value; }
        }

        private void Awake()
        {
            m_Controller = GetComponentInChildren<StatsTracker>();

            m_Health = m_Controller.GetOrCreateStat(healthTemplate, 1);
            m_Health.onValueChanged.AddListener(OnHealthChanged);

            deathTriggerID = Animator.StringToHash(m_DeathTriggerName);
        }

        /// <summary>
        /// Set the value of hit points to a normalized value.
        /// </summary>
        /// <param name="value">The normalized value to use. That is a value between 0 and 1, where 1 is equivalent to the max possible value and 0 is the equivalent of the minimal possible value.</param>
        public void SetHitPointsNormalized(float value)
        {
            m_Health.NormalizedValue = value;
        }

        /// <summary>
        /// Set the current hit points to an absolute value. If an attempt to set the value
        /// above or below the maximum or minium allowable values it will be clamped.
        /// </summary>
        /// <param name="value">The value to set hit points to.</param>
        public void SetHitPoints(float value)
        {
            m_Health.Value = value;
        }

        /// <summary>
        /// Damage the AI by a damage amount. The minimum value will be clamped according to the settings in the halth component.
        /// </summary>
        /// <param name="amount">The damage to be applied.</param>
        public void TakeDamage(float amount)
        {
            SetHitPoints(m_Health.Value - amount);
        }

        protected virtual void OnHealthChanged(float oldValue)
        {
            if (m_Animator != null && m_Health.NormalizedValue <= 0)
            {
                m_Animator.SetTrigger(deathTriggerID);

                Brain brain = GetComponentInChildren<Brain>();
                if (brain)
                {
                    brain.enabled = false;
                }
                else
                {
                    Debug.LogWarning("No brain found, so could not disable it on death.");
                }

                NavMeshAgent agent = GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = true;
                }
            }
        }
    }
}
