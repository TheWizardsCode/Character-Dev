using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.Character
{
    /// <summary>
    /// An actor cue contains details of what an actor should do at upon a signal from the director.
    /// </summary>
    [CreateAssetMenu(fileName = "ActorCue", menuName = "Wizards Code/Actor/Cue")]
    public class ActorCue : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Tooltip("The name of the mark the actor should move to on this cue.")]
        public string markName;

        [Header("Sound")]
        [SerializeField, Tooltip("Audio files for spoken lines")]
        public AudioClip audioClip;

        [Header("Dialogue")]
        [SerializeField, Tooltip("Dialogue to speak on this cue."), TextArea(10, 20)]
        public string dialogue;

        public enum ParameterType { Float, Int, Bool, Trigger }
        [Header("Animation Parameters")]
        [SerializeField, Tooltip("The name of the animation parameter to set the value to.")]
        public string paramName;
        [SerializeField, Tooltip("The type of parameter to set.")]
        public ParameterType paramType;
        [SerializeField, Tooltip("The float value of the parameter, value is ignored if parameter is not a float")]
        public float paramFloatValue;
        [SerializeField, Tooltip("The int value of the parameter, value is ignored if parameter is not a int")]
        public int paramIntValue;
        [SerializeField, Tooltip("The bool value of the parameter, value is ignored if parameter is not a bool")]
        public bool paramBoolValue;

        [Header("Animation Clips")]
        [SerializeField, Tooltip("Tha name of the animation clip to play.")]
        public string animationClipName;
        [SerializeField, Tooltip("The layer on which the animation clip resides.")]
        public int animationLayer;
        [SerializeField, Tooltip("The normalized time from which to start the animation.")]
        public float animationNormalizedTime = 0;

        /// <summary>
        /// Prompt and actor to enact the actions identified in this cue.
        /// </summary>
        public virtual void Prompt(ActorCharacter actor)
        {
            ProcessMove(actor);
            ProcessAudio(actor);
            ProcessAnimationParameters(actor);
            ProcessAnimationClips(actor);
        }

        /// <summary>
        /// If this cue has any animation parameter changes then have an actor make those changes.
        /// </summary>
        /// <param name="actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationParameters(ActorCharacter actor)
        {
            if (!string.IsNullOrWhiteSpace(paramName))
            {
                Animator animator = actor.GetComponent<Animator>();
                switch (paramType)
                {
                    case ActorCue.ParameterType.Trigger:
                        animator.SetTrigger(paramName);
                        break;
                    case ActorCue.ParameterType.Bool:
                        animator.SetBool(paramName, paramBoolValue);
                        break;
                    case ActorCue.ParameterType.Int:
                        animator.SetInteger(paramName, paramIntValue);
                        break;
                    case ActorCue.ParameterType.Float:
                        animator.SetBool(paramName, paramBoolValue);
                        break;
                }
            }
        }

        /// <summary>
        /// The name of an animation clip to play upon this cue.
        /// </summary>
        /// <param name="actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationClips(ActorCharacter actor)
        {
            if (string.IsNullOrWhiteSpace(animationClipName))
            {
                Animator animator = actor.GetComponent<Animator>();
                animator.Play(animationClipName, animationLayer, animationNormalizedTime);
            }
        }

        /// <summary>
        /// If this cue has any audio defined within it then have an actor enact play that audio.
        /// </summary>
        void ProcessMove(ActorCharacter actor)
        {
            if (!string.IsNullOrWhiteSpace(markName))
            {
                NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
                Transform mark = GameObject.Find(markName).transform;
                agent.SetDestination(mark.position);
            }
        }

        void ProcessAudio(ActorCharacter actor)
        {
            if (audioClip != null)
            {
                AudioSource source = actor.GetComponent<AudioSource>();
                source.clip = audioClip;
                source.Play();
            }
        }
    }
}
