using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character
{
    /// <summary>
    /// A character actor performs for the camera and takes cues from a director.
    /// </summary>
    public class ActorCharacter : MonoBehaviour
    {

        public void PlayEmote(string name)
        {
            Animator animator = GetComponent<Animator>();

            animator.Play(name);
        }
    }
}
