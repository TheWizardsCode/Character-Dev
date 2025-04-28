using UnityEngine;
using System.Collections;
using static WizardsCode.Character.ActorCueAnimator;
using System;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace WizardsCode.Character
{
    /// <summary>
    /// An actor cue that extends the basic ActorCue with Animator control features.
    /// </summary>
    [CreateAssetMenu(fileName = "Animation ActorCue", menuName = "Wizards Code/Actor/Animation Cue")]
    public class ActorCueAnimator : ActorCue
    {
        [Header("Animation Layers")]
        [SerializeField, Tooltip("The name of the layer to control the weight of. An emptry field means the layer weight has no effect.")]
        string m_LayerName = "";
        [SerializeField, Range(0f, 1), Tooltip("The weight of the layer")]
        float m_LayerWeight = 1;
        [SerializeField, Range(0f, 20), Tooltip("The time in seconds that it will take to reach the new layer weight.")]
        float m_LayerWeightChangeTime = 0.5f;
        [SerializeField, Tooltip("Should the layer weight be reverted back to the original level once this cue has completed?")]
        bool m_RevertLayerWeight = false;

        public enum ParameterType { Float, Int, Bool, Trigger }
        [Header("Animation Parameters")]
        [SerializeField, Tooltip("A set of animation parameter changes to make.")]
        AnimationParameter[] m_AnimationParams;

        [Header("Animation Clips")]
        [SerializeField, Tooltip("An animation to play that is not in an Animator.")]
        AnimationClip m_AnimationClip;
        [SerializeField, Tooltip("Should the duration of this cue be set to the duration of the clip?")]
        bool m_DurationMatchesAnimation = true;
        [SerializeField, Tooltip("The normalized time from which to start the animation.")]
        float animationNormalizedTime = 0;

        private int m_LayerIndex;
        private float m_OriginalLayerWeight;

        public float layerWeightChangeTime
        {
            get { return m_LayerWeightChangeTime; }
        }

        /// <summary>
        /// If true then the duration of this cue will be set to the duration of the animation clip.
        /// If false then the duration will be set to the value of the Duration property in the Inspector.
        /// 
        /// Note that this is overridden by the playable in the timeline since the timeline will set the duration of the cue.
        /// </summary>
        public bool DurationMatchesAnimation
        {
            get { return m_DurationMatchesAnimation; }
            set { m_DurationMatchesAnimation = value; }
        }

        public AnimationClip Clip { get { return m_AnimationClip; } }

        AnimatorActorController m_AnimatorController;
        /// <summary>
        /// If the actor controller is an animator controller then this will return it, otherwise
        /// it will return a null.
        /// </summary>
        internal AnimatorActorController AnimatorController {
            get
            {
                if (m_AnimatorController == null)
                {
                    m_AnimatorController = m_Actor as AnimatorActorController;
                    if (m_AnimatorController == null)
                    {
                        Debug.LogError("The actor controller is not an animator controller.");
                        return null;
                    }
                }

                return m_AnimatorController;
            }
        }

        private void ProcessAnimationLayerWeights()
        {
            if (AnimatorController.Animator != null && !string.IsNullOrEmpty(m_LayerName))
            {
                m_LayerIndex = AnimatorController.Animator.GetLayerIndex(m_LayerName);
                m_OriginalLayerWeight = AnimatorController.Animator.GetLayerWeight(m_LayerIndex);
            }
        }

        private IEnumerator RevertAnimationLayerWeights()
        {
            if (m_RevertLayerWeight)
            {
                if (AnimatorController.Animator != null && m_LayerIndex >= 0 && AnimatorController.Animator.GetLayerWeight(m_LayerIndex) != m_OriginalLayerWeight)
                {
                    float originalWeight = AnimatorController.Animator.GetLayerWeight(m_LayerIndex);
                    float time = 0;
                    while (!Mathf.Approximately(AnimatorController.Animator.GetLayerWeight(m_LayerIndex), m_LayerWeight))
                    {
                        time += Time.deltaTime;
                        AnimatorController.Animator.SetLayerWeight(m_LayerIndex,
                            Mathf.Lerp(originalWeight, m_LayerWeight, time / m_LayerWeightChangeTime));
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            AnimatorController.PlayAnimatorController();

            yield return null;
        }

        public override IEnumerator Prompt(BaseActorController actor)
        {
            m_Actor = actor;

            if (m_AnimationClip != null && DurationMatchesAnimation)
            {
                Duration = m_AnimationClip.length;
            }

            ProcessAnimationLayerWeights();
            ProcessAnimationParameters();
            AnimatorController.PlayAnimationClip(Clip);

            yield return base.Prompt(actor);
        }

        public override IEnumerator Revert(BaseActorController actor)
        {
            yield return RevertAnimationLayerWeights();
            RevertAnimationParameters();
            yield return base.Revert(actor);
        }

        protected override IEnumerator UpdateCoroutine()
        {
            // Update Layers
            if (AnimatorController.Animator != null && m_LayerIndex >= 0 && AnimatorController.Animator.GetLayerWeight(m_LayerIndex) != m_LayerWeight)
            {
                float originalWeight = AnimatorController.Animator.GetLayerWeight(m_LayerIndex);
                float time = 0;
                while (!Mathf.Approximately(AnimatorController.Animator.GetLayerWeight(m_LayerIndex), m_LayerWeight))
                {
                    time += Time.deltaTime;
                    AnimatorController.Animator.SetLayerWeight(m_LayerIndex,
                        Mathf.Lerp(originalWeight, m_LayerWeight, time / m_LayerWeightChangeTime));
                    yield return new WaitForEndOfFrame();
                }
            }

            yield return base.UpdateCoroutine();
        }

        /// <summary>
        /// If this cue has any animation parameter changes then have an actor make those changes.
        /// </summary>
        /// <param name="m_Actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationParameters()
        {
            for (int i = 0; i < m_AnimationParams.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(m_AnimationParams[i].paramName))
                {
                    switch (m_AnimationParams[i].paramType)
                    {
                        case ParameterType.Trigger:
                            AnimatorController.Animator.SetTrigger(m_AnimationParams[i].paramName);
                            break;
                        case ParameterType.Bool:
                            m_AnimationParams[i].originalBoolValue = AnimatorController.Animator.GetBool(m_AnimationParams[i].paramName);
                            AnimatorController.Animator.SetBool(m_AnimationParams[i].paramName, m_AnimationParams[i].paramBoolValue);
                            break;
                        case ParameterType.Int:
                            m_AnimationParams[i].originalIntValue = AnimatorController.Animator.GetInteger(m_AnimationParams[i].paramName);
                            AnimatorController.Animator.SetInteger(m_AnimationParams[i].paramName, m_AnimationParams[i].paramIntValue);
                            break;
                        case ParameterType.Float:
                            m_AnimationParams[i].originalFloatValue = AnimatorController.Animator.GetFloat(m_AnimationParams[i].paramName);
                            AnimatorController.Animator.SetFloat(m_AnimationParams[i].paramName, m_AnimationParams[i].paramFloatValue);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// If this cue has any animation parameter changes then have an actor revert those changes.
        /// </summary>
        /// <param name="m_Actor">The actor to enact the animation changes.</param>
        private void RevertAnimationParameters()
        {
            for (int i = 0; i < m_AnimationParams.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(m_AnimationParams[i].paramName))
                {
                    switch (m_AnimationParams[i].paramType)
                    {
                        case ParameterType.Bool:
                            AnimatorController.Animator.SetBool(m_AnimationParams[i].paramName, m_AnimationParams[i].originalBoolValue);
                            break;
                        case ParameterType.Int:
                            AnimatorController.Animator.SetInteger(m_AnimationParams[i].paramName, m_AnimationParams[i].originalIntValue);
                            break;
                        case ParameterType.Float:
                            AnimatorController.Animator.SetFloat(m_AnimationParams[i].paramName, m_AnimationParams[i].originalFloatValue);
                            break;
                    }
                }
            }
        }
    }

    [Serializable]
    struct AnimationParameter
    {
        [SerializeField, Tooltip("The name of the animation parameter to set the value to.")]
        public string paramName;
        [SerializeField, Tooltip("The type of parameter to set.")]
        public ParameterType paramType;
        [SerializeField, Tooltip("The float value of the parameter, value is ignored if parameter is not a float")]
        public float paramFloatValue;
        [SerializeField, Tooltip("The int value of the parameter, value is ignored if parameter is not a int")]
        public int paramIntValue;
        [SerializeField, Tooltip("The bool value of the parameter, value is ignored if parameter is not a bool")]
        public bool paramBoolValue;

        internal float originalFloatValue;
        internal int originalIntValue;
        internal bool originalBoolValue;
    }
}
