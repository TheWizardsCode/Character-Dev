using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode.AnimationControl
{
    /// <summary>
    /// EmotionalStateBehaviour is a StateMachineBehaviour that can be attacched to an animation controller to influence which animations are played.
    /// 
    /// It can randomly vary the enjoyment and activation values of a character which is useful in prototyping. However, in production it would normally
    /// be paired with a EmotionalState MonoBehaviour on the character which will provide these values.
    /// 
    /// </summary>
    public class EmotionalStateMachineBehavior : StateMachineBehaviour
    {
        [SerializeField, Tooltip("If true the activation parameter will be randomized. This is good for protyping but in production you will likely want to control through scripts or timeline.")]
        bool m_RandomizeActivation = false;
        [SerializeField, Tooltip("If true the happiness parameter will be randomized. This is good for protyping but in production you will likely want to control through scripts or timeline.")]
        bool m_RandomizeEnjoyment = false;
        [SerializeField, Range(0, 0.5f), Tooltip("Randomness effect, a random value between -x and +x will be applied to activation and/or enjoyment if the above values are true.")]
        float m_Randomness = 0.1f;
        [SerializeField, Range(0.1f, 10f), Tooltip("How often, in seconds, should the characters pose be changed? This controls how long it takes to move from one pose to the next. If randomized values are used this will affect how often the values are changed. ")]
        float m_PoseChangeTime = 2f;

        [SerializeField, Tooltip("The name of the animation parameter that corresponds to the characters level of enjoyment (from depressed, 0, to elated, 1).")]
        string enjoymentParameterName = "Enjoyment";
        [SerializeField, Tooltip("The name of the animation parameter that corresponds to the characters level of activation (from calm, 0, to activated, 1).")]
        string activationParameterName = "Activation";
        
        private float m_timeSinceLastPoseChange = float.PositiveInfinity;
        private float startActivation;
        private float endActivation;
        private float previousActivation;
        private float startEnjoyment;
        private float endEnjoyment;
        private float previousEnjoyment;
        private int activationParamHash;
        private int enjoymentParamHash;
        private EmotionalState emotionalState;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            activationParamHash = Animator.StringToHash(activationParameterName);
            enjoymentParamHash = Animator.StringToHash(enjoymentParameterName);

            emotionalState = animator.GetComponentInParent<EmotionalState>();
        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (emotionalState == null && !(m_RandomizeActivation || m_RandomizeEnjoyment)) return;

            m_timeSinceLastPoseChange += Time.deltaTime;

            if (emotionalState != null)
            {   
                ConfigureForEmotionalState(animator);
            }
            
            if (m_RandomizeActivation || m_RandomizeEnjoyment)
            {   // Add some randomization if it is time to do so and the randomization effect is turned on
                startActivation = animator.GetFloat(activationParamHash);
                startEnjoyment = animator.GetFloat(enjoymentParamHash);
                AddRandomization(animator);
            }

            // Interpolate from the captured start values toward the current targets.
            // Progress is measured in elapsed time, so the animator parameters track
            // the emotional state within m_PoseChangeTime regardless of frame rate.
            PoseChangeValues pose = ComputePoseValues(
                startActivation, endActivation,
                startEnjoyment, endEnjoyment,
                m_timeSinceLastPoseChange, m_PoseChangeTime);
            animator.SetFloat(activationParamHash, pose.activation);
            animator.SetFloat(enjoymentParamHash, pose.enjoyment);
        }

        /// <summary>
        /// The interpolated animator parameter values for a single pose-change update.
        /// </summary>
        public struct PoseChangeValues
        {
            public float activation;
            public float enjoyment;
        }

        /// <summary>
        /// Computes the interpolated Activation/Enjoyment values for a pose change.
        /// Progress is measured as <c>elapsedTime / poseChangeTime</c> (clamped to
        /// [0,1]), which makes the interpolation frame-rate independent: after
        /// <c>poseChangeTime</c> seconds the values reach the targets regardless of
        /// the frame rate.
        /// </summary>
        /// <param name="startActivation">The Activation value when the pose change began.</param>
        /// <param name="endActivation">The target Activation value.</param>
        /// <param name="startEnjoyment">The Enjoyment value when the pose change began.</param>
        /// <param name="endEnjoyment">The target Enjoyment value.</param>
        /// <param name="elapsedTime">Time (in seconds) since the pose change began.</param>
        /// <param name="poseChangeTime">How long the pose change should take, in seconds.</param>
        public static PoseChangeValues ComputePoseValues(
            float startActivation, float endActivation,
            float startEnjoyment, float endEnjoyment,
            float elapsedTime, float poseChangeTime)
        {
            float t = poseChangeTime > 0f ? elapsedTime / poseChangeTime : 1f;
            t = Mathf.Clamp01(t);

            PoseChangeValues values;
            values.activation = Mathf.SmoothStep(startActivation, endActivation, t);
            values.enjoyment = Mathf.SmoothStep(startEnjoyment, endEnjoyment, t);
            return values;
        }

        /// <summary>
        /// Adds some randomization to the pose if the state machine is configured to do so
        /// and enough time has changed since the last post change.
        /// </summary>
        /// <param name="animator">The animator containing the animation state to configure</param>
        private void AddRandomization(Animator animator)
        {
            if (m_timeSinceLastPoseChange >= m_PoseChangeTime)
            {
                if (m_RandomizeActivation)
                {
                    endActivation += Random.Range(-m_Randomness, m_Randomness);
                    endActivation = Mathf.Clamp01(endActivation);
                }

                if (m_RandomizeEnjoyment)
                {
                    endEnjoyment += Random.Range(-m_Randomness, m_Randomness);
                    endEnjoyment = Mathf.Clamp01(endEnjoyment);
                }

                m_timeSinceLastPoseChange = 0;
            }
        }

        /// <summary>
        /// Configure the animator to react according to the current emotional state of the cahracter.
        /// </summary>
        /// <param name="animator"></param>
        private void ConfigureForEmotionalState(Animator animator)
        {
            if (previousActivation != emotionalState.activationValue)
            {
                startActivation = animator.GetFloat(activationParamHash);
                endActivation = emotionalState.activationValue;
                previousActivation = endActivation;
                m_timeSinceLastPoseChange = 0;
            }

            if (previousEnjoyment != emotionalState.enjoymentValue)
            {
                startEnjoyment = animator.GetFloat(enjoymentParamHash);
                endEnjoyment = emotionalState.enjoymentValue;
                previousEnjoyment = endEnjoyment;
                m_timeSinceLastPoseChange = 0;
            }
        }
    }
}