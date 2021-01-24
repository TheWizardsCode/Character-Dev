using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace WizardsCode.Character
{
    /// <summary>
    /// The director is responsible for giving characters direction during a performance.
    /// </summary>
    public class Director : MonoBehaviour
    {
        [SerializeField, Tooltip("The actors this director is directing.")]
        ActorCharacter actor;

        [SerializeField, Tooltip("Cues that the director must give to the actor.")]
        ActorCue[] cues;

        [SerializeField, Tooltip("A button that, when pressed, will cue the actor to start the next segment.")]
        Button cueButton;

        [SerializeField, Tooltip("If set to true the director will loop back to the first cue when the last has been completed.")]
        bool loop = false;

        int cueIndex = -1;

        private void Start()
        {
            if (cueButton != null)
            {
                cueButton.onClick.AddListener(Prompt);
            }
            SetupNextCue();
        }

        /// <summary>
        /// Prompt the next cue.
        /// </summary>
        public void Prompt()
        {
            ActorCue cue = cues[cueIndex];
            cue.Prompt(actor);
            SetupNextCue();
        }

        private void SetupNextCue()
        {
            cueIndex++;
            if (cueButton == null) return;

            if (cueIndex >= cues.Length)
            {
                if (loop)
                {
                    cueIndex = 0;
                }
                else
                {
                    cueButton.interactable = false;
                    cueButton.GetComponentInChildren<Text>().text = "Finished";
                    return;
                }
            }
            else
            {
                cueButton.interactable = true;
            }

            cueButton.GetComponentInChildren<Text>().text = "Prompt " + (cueIndex + 1) + "\n" + cues[cueIndex].name;
        }
    }
}
