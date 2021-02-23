using System.Collections;
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

        [Header("Animation Layers")]
        [SerializeField, Tooltip("The name of the layer to control the weight of. An emptry field means the layer weight has no effect.")]
        string m_LayerName = "";
        [SerializeField, Tooltip("The weight of the layer")]
        float m_LayerWeight = 1;
        [SerializeField, Range(0.1f, 20), Tooltip("The speed at which we will change fromt he current layer weight to the new layer weight. Larger is faster.")]
        float m_LayerChangeSpeed = 5;

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
        private IEnumerator coroutine;

        /// <summary>
        /// Prompt and actor to enact the actions identified in this cue.
        /// </summary>
        /// <returns>An optional coroutine that shouold be started by the calling MonoBehaviour</returns>
        public virtual IEnumerator Prompt(ActorController actor)
        {
            ProcessMove(actor);
            ProcessAudio(actor);
            ProcessAnimationLayerWeights(actor);
            ProcessAnimationParameters(actor);
            ProcessAnimationClips(actor);

            return coroutine;
        }

        private void ProcessAnimationLayerWeights(ActorController actor)
        {
            int layerIndex = actor.Animator.GetLayerIndex(m_LayerName);
            if (actor.Animator.GetLayerWeight(layerIndex) != m_LayerWeight)
            {
                coroutine = ChangeLayerWeight(layerIndex, m_LayerWeight, actor);
            }
        }

        internal IEnumerator ChangeLayerWeight(int layerIndex, float targetWeight, ActorController actor)
        {
            float currentWeight = actor.Animator.GetLayerWeight(layerIndex);
            while (!Mathf.Approximately(currentWeight, m_LayerWeight))
            {
                currentWeight = actor.Animator.GetLayerWeight(layerIndex);
                float delta = (m_LayerWeight - currentWeight) * (Time.deltaTime * m_LayerChangeSpeed);
                actor.Animator.SetLayerWeight(layerIndex, currentWeight + delta);
                yield return new WaitForEndOfFrame();
            }

            actor.Animator.SetLayerWeight(layerIndex, m_LayerWeight);
        }

        /// <summary>
        /// If this cue has any animation parameter changes then have an actor make those changes.
        /// </summary>
        /// <param name="actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationParameters(ActorController actor)
        {
            if (!string.IsNullOrWhiteSpace(paramName))
            {
                switch (paramType)
                {
                    case ActorCue.ParameterType.Trigger:
                        actor.Animator.SetTrigger(paramName);
                        break;
                    case ActorCue.ParameterType.Bool:
                        actor.Animator.SetBool(paramName, paramBoolValue);
                        break;
                    case ActorCue.ParameterType.Int:
                        actor.Animator.SetInteger(paramName, paramIntValue);
                        break;
                    case ActorCue.ParameterType.Float:
                        actor.Animator.SetBool(paramName, paramBoolValue);
                        break;
                }
            }
        }

        /// <summary>
        /// The name of an animation clip to play upon this cue.
        /// </summary>
        /// <param name="actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationClips(ActorController actor)
        {
            if (string.IsNullOrWhiteSpace(animationClipName))
            {
                actor.Animator.Play(animationClipName, animationLayer, animationNormalizedTime);
            }
        }

        /// <summary>
        /// If this cue has any audio defined within it then have an actor enact play that audio.
        /// </summary>
        void ProcessMove(ActorController actor)
        {
            if (!string.IsNullOrWhiteSpace(markName))
            {
                NavMeshAgent agent = actor.GetComponent<NavMeshAgent>();
                Transform mark = GameObject.Find(markName).transform;
                agent.SetDestination(mark.position);
            }
        }

        void ProcessAudio(ActorController actor)
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
