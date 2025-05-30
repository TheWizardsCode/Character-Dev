using System.Collections;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace WizardsCode.Character
{
    /// <summary>
    /// A basic actor cue contains details of what an actor should do at upon a signal from the director.
    /// </summary>
    [CreateAssetMenu(fileName = "ActorCue", menuName = "Wizards Code/Actor/Cue")]
    public class ActorCue : ScriptableObject
    {
        [SerializeField, Tooltip("A human readable name for this cue that is used in UI and logs.")]
        string m_DisplayName = "New Cue";
        [TextArea, SerializeField, Tooltip("A description of this actor cue.")]
        string m_Description;

        [SerializeField, Tooltip("Duration of this phase of this cue action. If 0 then it is unlimited. Note that this is overridden by the playable in the timeline since the timeline will set the duration of the cue.")]
        float m_Duration = 5;

        // Movement
        [SerializeField, Tooltip("The name of the mark the actor should move to on this cue."), BoxGroup("Movement")]
        string m_MarkName;
        [SerializeField, Tooltip("Should the name of the mark be prefixed with the display name of the actor and a ' - ' separator (with spaces)?"), BoxGroup("Movement")]
        bool m_PrefixWithName = false;
        [SerializeField, Tooltip("Stop movement upon receiving this cue. Note that this will override the markName setting above, that is if this is set and markName is set then no movement will occur."), BoxGroup("Movement")]
        bool m_StopMovement = false;

        // Audio
        [SerializeField, Tooltip("Audio files for spoken lines"), BoxGroup("Audio")]
        public AudioClip audioClip;

        protected BaseActorController m_Actor;

        public float Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        /// <summary>
        /// Get or set the mark name, that is the name of an object in the scene the character should move to when this cue is prompted.
        /// Note that changing the Mark during execution of this cue will
        /// have no effect until it is prompted the next time.
        /// </summary>
        public string markName
        {
            get {
                if (m_PrefixWithName)
                {
                    return $"{m_Actor.displayName} - {m_MarkName}";
                }
                else
                {
                    return m_MarkName;
                }
            }
            set { m_MarkName = value; }
        }

        private float m_EndTime;
        /// <summary>
        /// Prompt an actor to enact the actions identified in this cue.
        /// </summary>
        /// <returns>An optional coroutine that should be started by the calling MonoBehaviour</returns>
        public virtual IEnumerator Prompt(BaseActorController actor)
        {
            m_EndTime = Time.time + Duration;

            m_Actor = actor;

            ProcessMove();
            ProcessAudio();

            yield return UpdateCoroutine();

            yield return new WaitForSeconds(m_EndTime - Time.time);

            yield return Revert(actor);
        }

        /// <summary>
        /// Prompt an actor to revert any setup required by this cue.
        /// </summary>
        /// <returns>An optional coroutine that should be started by the calling MonoBehaviour</returns>
        public virtual IEnumerator Revert(BaseActorController actor)
        {
            return null; // don't do anything by default. This is really only for AnimatorCues and similar that change state.
        }

        protected virtual IEnumerator UpdateCoroutine()
        {
            return null;
        }

        /// <summary>
        /// If this cue has a mark defined move to it.
        /// </summary>
        protected virtual void ProcessMove()
        {
            if (m_StopMovement)
            {
                m_Actor.StopMoving();
                return;
            }

            if (!string.IsNullOrWhiteSpace(markName))
            {
                GameObject go = GameObject.Find(markName);
                if (go != null)
                {
                    m_Actor.MoveTo(go.transform);
                } else
                {
                    Debug.LogWarning(m_Actor.name + "  has a mark set, but the mark doesn't exist in the scene. The name set is " + markName);
                }
            }
        }

        protected virtual void ProcessAudio()
        {
            if (audioClip != null)
            {
                AudioSource source = m_Actor.GetComponent<AudioSource>();
                source.clip = audioClip;
                source.Play();
            }
        }
    }
}
