using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;

namespace WizardsCode.Character.AI
{
    public class DeathBehaviour : GenericSolitaryBehaviour
    {
        protected override void OnUpdateState()
        {
            Brain.active = false;
            if (CurrentState == State.Finalizing && AnimatorController.Animator)
            {
                AnimatorController.Animator.enabled = false;
            }
            base.OnUpdateState();
        }
    }
}
