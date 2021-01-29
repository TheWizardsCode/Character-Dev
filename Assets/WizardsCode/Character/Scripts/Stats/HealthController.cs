using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;

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
        StatsController controller;
        int deathTriggerID;

        private void Start()
        {
            controller = GetComponent<StatsController>();
            SetHitPontsNormalized(1);
            deathTriggerID = Animator.StringToHash(deathTriggerName);
        }

        private void SetHitPontsNormalized(float value)
        {
            health = controller.GetOrCreateStat(healthTemplate.name, value);
            health.onValueChanged.AddListener(OnHealthChanged);
        }

        private void OnHealthChanged(float normalizedDelta)
        {
            if (m_Animator != null && health.normalizedValue == 0)
            {
                m_Animator.SetTrigger(deathTriggerID);
            }
        }
    }
}
