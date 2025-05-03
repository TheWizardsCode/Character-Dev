using NaughtyAttributes;
using UnityEngine;

namespace WizardsCode.Animation {
    /// <summary>
    /// This behaviour is used to set a random integer value for a discreet parameter in the animator.
    /// </summary>
    public class DiscreetRandomIntParameterBehaviour : StateMachineBehaviour
    {
        [SerializeField, Tooltip("The name of the parameter to set.")]
        private string parameterName = "RandomInt";
        [SerializeField, Tooltip("The minimum and maximum possible values of the parameter."), MinMaxSlider(0.0f, 100.0f)]
        private Vector2 minMax = new Vector2(0, 100);

        private int paramHash;

        void OnEnable()
        {
            paramHash = Animator.StringToHash(parameterName);
        }

        // public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        // {
        //     int randomValue = Random.Range((int)minMax.x, (int)minMax.y + 1);
        //     animator.SetInteger(paramHash, randomValue);
        // }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            int randomValue = Random.Range((int)minMax.x, (int)minMax.y + 1);
            animator.SetInteger(paramHash, randomValue);
        }
    }
}