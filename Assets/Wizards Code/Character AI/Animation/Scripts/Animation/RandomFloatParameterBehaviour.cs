using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Animation
{
    /// <summary>
    /// A State Machine Behaviour that will set a Float Parameter on an animator randomly between
    /// two values.
    /// </summary>
    public class RandomFloatParameterBehaviour : StateMachineBehaviour
    {
        [SerializeField, Range(0, 1), Tooltip("The minimum value in the random range.")]
        float m_MinValue = 0;
        [SerializeField, Range(0, 1), Tooltip("The maximum value in the random range.")]
        float m_MaxValue = 1;
        [SerializeField, Tooltip("The name of the parameter to set.")]
        string m_ParameterName = "Randomness";
        [SerializeField, Range(0.5f, 10), Tooltip("The frequency that the value should be changed.")]
        float m_FrequencyOfRandomChange = 2f;
        [SerializeField, Range(1,100), Tooltip("The speed of lerping from the current value to the new value.")]
        float m_SpeedOfLerp = 30;

        private float m_TimeSinceLastChange;
        private float m_TargetValue;
        private int randomParamHash;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            randomParamHash = Animator.StringToHash(m_ParameterName);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            float currentValue = animator.GetFloat(randomParamHash);

            m_TimeSinceLastChange += Time.deltaTime;

            if (m_TimeSinceLastChange > m_FrequencyOfRandomChange)
            {
                m_TargetValue = Random.Range(m_MinValue, m_MaxValue);
                m_TimeSinceLastChange = 0;
            }

            if (!Mathf.Approximately(m_TargetValue, currentValue)) {
                animator.SetFloat(randomParamHash, Mathf.Lerp(currentValue, m_TargetValue, m_SpeedOfLerp * Time.deltaTime));
            }
        }
    }
}