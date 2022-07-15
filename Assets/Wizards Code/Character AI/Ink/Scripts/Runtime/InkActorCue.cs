#if INK_PRESENT
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using WizardsCode.Character;

namespace WizardsCode.Ink
{
    /// <summary>
    /// A basic actor cue contains details of what an actor should do at upon a signal from the director.
    /// </summary>
    [CreateAssetMenu(fileName = "ActorCue", menuName = "Wizards Code/Actor/Cue")]
    public class InkActorCue : ActorCue
    {
        [Header("Ink")]
        [SerializeField, Tooltip("The name of the knot to jump to on this cue.")]
        string m_KnotName;
        [SerializeField, Tooltip("The name of the stitch to jump to on this cue.")]
        string m_StitchName;


        /// <summary>
        /// Prompt and actor to enact the actions identified in this cue.
        /// </summary>
        /// <returns>An optional coroutine that shouold be started by the calling MonoBehaviour</returns>
        public override IEnumerator Prompt(BaseActorController actor)
        {
            m_Actor = actor;

            ProcessMove();
            ProcessAudio();
            ProcessInk();

            return UpdateCoroutine();
        }

        internal void ProcessInk()
        {
            if (!string.IsNullOrEmpty(m_KnotName) || !string.IsNullOrEmpty(m_StitchName))
            {
                InkManager.Instance.ChoosePath(m_KnotName, m_StitchName);
            }
        }
    }
}
#endif