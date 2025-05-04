using NaughtyAttributes;
using UnityEngine;

namespace WizardsCode.Animation {
    /// <summary>
    /// This behaviour is used to set a random integer value for a discreet parameter in the animator.
    /// </summary>
    public class DiscreetRandomIntParameterBehaviour : StateMachineBehaviour
    {
        [SerializeField, Tooltip("Should the parameter be randomized on state machine enter? If this is true then the parameter will be randomized when the state machine is entered.")]
        private bool randomizeOnStateMachineEnter = true;
        [SerializeField, Tooltip("Should the parameter be randomized on state enter? If this is true then the parameter will be randomized when the state is entered.")]
        private bool randomizeOnStateEnter = false;
        [SerializeField, Tooltip("The name of the parameter to set.")]
        private string parameterName = "RandomInt";
        [SerializeField, Tooltip("The minimum and maximum possible values of the parameter."), MinMaxSlider(0.0f, 100.0f)]
        private Vector2Int minMax = new Vector2Int(0, 100);

        private int paramHash;

        void OnEnable()
        {
            paramHash = Animator.StringToHash(parameterName);
        }

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if (!randomizeOnStateMachineEnter)
            {
                return;
            }
            int randomValue = Random.Range(minMax.x, minMax.y + 1);
            animator.SetInteger(paramHash, randomValue);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!randomizeOnStateEnter)
            {
                return;
            }
            int randomValue = Random.Range(minMax.x, minMax.y + 1);
            animator.SetInteger(paramHash, randomValue);
        }
    }
}