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
        string deathTriggerName = "Die";

        StatSO health;
        Brain controller;
        int deathTriggerID;

        private void Start()
        {
            controller = GetComponent<Brain>();

            health = controller.GetOrCreateStat(healthTemplate, 1);
            health.onValueChanged.AddListener(OnHealthChanged);

            deathTriggerID = Animator.StringToHash(deathTriggerName);
        }

        /// <summary>
        /// Set the value of hit points to a normalized value.
        /// </summary>
        /// <param name="value">The normalized value to use. That is a value between 0 and 1, where 1 is equivalent to the max possible value and 0 is the equivalent of the minimal possible value.</param>
        public void SetHitPointsNormalized(float value)
        {
            health.NormalizedValue = value;
        }

        /// <summary>
        /// Set the current hit points to an absolute value. If an attempt to set the value
        /// above or below the maximum or minium allowable values it will be clamped.
        /// </summary>
        /// <param name="value">The value to set hit points to.</param>
        public void SetHitPoints(float value)
        {
            health.Value = value;
        }

        private void OnHealthChanged(float normalizedDelta)
        {
            if (m_Animator != null && health.NormalizedValue == 0)
            {
                m_Animator.SetTrigger(deathTriggerID);

                Brain brain = GetComponent<Brain>();
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
