#if SALSA
using CrazyMinnow.SALSA;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.AnimationControl
{
    /// <summary>
    /// The AnimationlayerController configures the weights of layers in the Animator based on what the character
    /// is currently doing.
    /// </summary>
    public class AnimationLayerController : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 4f), Tooltip("The duration of the transition from one weight to the next, if this is too short the animations will look jerky.")]
        float m_ChangeDuration = 0.5f;
        [SerializeField, Range(0, 1f), Tooltip("The weight for the layer used when the character is deemed to be talking.")]
        float m_TalkingLayerWieght = 1f;

        private Animator m_animator;
        private float m_GoalWeight;
        private float m_ChangeProgress = 0;

#if SALSA
        private Salsa m_salsa;
#else
        private bool m_IsTalking;
#endif

        // TODO Should look up the index of the talking layer
        private const int TALKING_LAYER_INDEX = 1;

        [Obsolete("You should use an AnimationActorCue instead.")]
        private void Awake()
        {
            m_animator = GetComponentInParent<Animator>();

#if SALSA
            m_salsa = GetComponent<Salsa>();
#endif
        }

        void Update()
        {
            float currentWeight = m_animator.GetLayerWeight(TALKING_LAYER_INDEX);
            if (!Mathf.Approximately(currentWeight, m_GoalWeight))
            {
                m_animator.SetLayerWeight(TALKING_LAYER_INDEX, Mathf.Lerp(currentWeight, m_GoalWeight, m_ChangeDuration));
            } else
            {
                m_ChangeProgress = 0;
            }
        }

#if Salsa
        public bool isTalking
        {
            get
            {
                return m_salsa.IsSALSAing;
            }

            set
            {
                Debug.LogWarning("Using Salsa to manage LipSync and therefore to manage whether the character is talking or not. Should not be calling isTalking = true/false.");
            }
        }
#else
        public bool isTalking
        {
            get
            {
                return m_IsTalking;
            }

            set
            {
                m_IsTalking = value;
                if (m_IsTalking)
                {
                    m_GoalWeight = m_TalkingLayerWieght;
                } else
                {
                    m_GoalWeight = 0;
                }
            }
        }
#endif
    }
}