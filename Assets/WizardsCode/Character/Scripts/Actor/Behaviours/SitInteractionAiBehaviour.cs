using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using UnityEditor;

namespace WizardsCode.Character
{
    /// <summary>
    /// The character will sit on the interactable.
    /// </summary>
    public class SitInteractionAiBehaviour : GenericInteractionAIBehaviour
    {
        [Header("Sitting Setup")]
        [SerializeField, Tooltip("An offset applied to the position of the character when they sit.")]
        float sittingOffset = 0.25f;

        internal override void StartBehaviour(float duration)
        {
            base.StartBehaviour(duration);
            Sit(CurrentInteractableTarget.interactionPoint);
        }

        internal override void FinishBehaviour()
        {
            base.FinishBehaviour();

            Brain.Actor.Animator.SetBool("Sitting", false);
            Vector3 pos = transform.position;
            pos.z += sittingOffset;
            Brain.Actor.MoveTargetPosition = pos;
        }

        public void Sit(Transform sitPosition)
        {
            Brain.Actor.MoveTo(sitPosition.position, () =>
            {
                Brain.Actor.TurnTo(sitPosition.rotation);
            },
            () =>
            {
                Vector3 pos = sitPosition.position;
                pos.z -= sittingOffset; // slide back in the chair a little
                Brain.Actor.MoveTo(pos, null, null, () =>
                {
                    Brain.Actor.isFootIKActive = true;
                    Brain.Actor.Animator.SetBool("Sitting", true);
                });
            },
            null);
        }
    }
}