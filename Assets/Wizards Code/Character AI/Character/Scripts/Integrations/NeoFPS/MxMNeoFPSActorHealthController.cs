#if MXM_PRESENT 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using UnityEngine.AI;
using MxM;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// MxMNeoFPSActorHealthController bridges between NeoFPS the WizardsCode Character 
    /// system (to handle health) and Motion Matching for Unity (MxM) to handle animation.
    /// </summary>
    public class MxMNeoFPSActorHealthController : MonoBehaviour
    {
        [Header("Stats Controller Config")]
        [SerializeField, Tooltip("A template used to create the main health statistic.")]
        StatSO healthTemplate;

        [Header("Motion Matching")]
        [SerializeField, Tooltip("The actor cue to prompt when the actor dies.")]
        ActorCue m_DieActorCue;

        StatSO health;
        Brain controller;
        private MxMAnimator animator;

        private void Start()
        {
            controller = GetComponentInChildren<Brain>();

            health = controller.GetOrCreateStat(healthTemplate, 1);
            health.onValueChanged.AddListener(OnHealthChanged);
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
            if (health.NormalizedValue == 0)
            {
                m_DieActorCue.Prompt(controller.Actor);
                controller.enabled = false;

                NavMeshAgent agent = GetComponentInParent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                animator = GetComponent<MxMAnimator>();
                animator.OnEventComplete.AddListener(DisableCharacter);
            }
        }

        void DisableCharacter()
        {
            animator.Pause();
            animator.OnEventComplete.RemoveListener(DisableCharacter);

            Behaviour[] components = GetComponentsInChildren<Behaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != this && components[i].GetType() != typeof(Renderer))
                {
                    components[i].enabled = false;
                }
            }

            this.enabled = false;
        }
    }
}
#endif