using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.AnimationControl
{
    /// <summary>
    /// A State Machine Behaviour that will set a Float Parameter on an animator on a cycle between two values.
    /// It will increase the value until a maximum is hit and then descrease back to the minimum and then
    /// cycle again.
    /// </summary>
    public class CyclicFloatParameterBehaviour : StateMachineBehaviour
    {
        [SerializeField, Range(0, 1), Tooltip("The minimum value in the cyclic range.")]
        float m_MinValue = 0;
        [SerializeField, Range(0, 1), Tooltip("The maximum value in the cyclic range.")]
        float m_MaxValue = 1;
        [SerializeField, Tooltip("The name of the parameter to set.")]
        string m_ParameterName = "CycleFloat";
        [SerializeField, Range(1,100), Tooltip("The smoothness of cycle from the current value to the new value.")]
        float m_SpeedOfCycle = 30;
        [SerializeField, Tooltip("Should the change LERP (true) or be linear (false)?")]
        bool m_Lerp = false;

        private bool isIncreasing;
        private int cyclicParamHash;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            cyclicParamHash = Animator.StringToHash(m_ParameterName);
            isIncreasing = animator.GetFloat(cyclicParamHash) <= m_MaxValue;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            float currentValue = animator.GetFloat(cyclicParamHash);

            if (isIncreasing && currentValue < m_MaxValue) {
                if (m_Lerp)
                {
                    animator.SetFloat(cyclicParamHash, Mathf.Lerp(currentValue, m_MaxValue, m_SpeedOfCycle * Time.deltaTime));
                } else
                {
                    float step = ((m_MaxValue - m_MinValue) / m_SpeedOfCycle) * Time.deltaTime;
                    animator.SetFloat(cyclicParamHash, currentValue + step);
                }
            } else if (!isIncreasing && currentValue > m_MinValue)
            {
                if (m_Lerp)
                {
                    animator.SetFloat(cyclicParamHash, Mathf.Lerp(currentValue, m_MinValue, m_SpeedOfCycle * Time.deltaTime));
                }
                else
                {
                    float step = ((m_MaxValue - m_MinValue) / m_SpeedOfCycle) * Time.deltaTime;
                    animator.SetFloat(cyclicParamHash, currentValue - step);
                }
            }
            else
            {
                isIncreasing = !isIncreasing;
            }
        }
    }
}