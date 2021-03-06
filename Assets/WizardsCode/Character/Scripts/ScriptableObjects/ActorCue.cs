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
        string markName;

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

        private ActorController m_Actor;
        private int m_LayerIndex;

        /// <summary>
        /// Get or set the mark name, that is the name of an object in the scene the character should move to when this cue is prompted.
        /// Note that changing the Mark during execution of this cue will
        /// have no effect until it is prompted the next time.
        /// </summary>
        public string Mark
        {
            get { return markName; }
            set { markName = value; }
        }

        /// <summary>
        /// Prompt and actor to enact the actions identified in this cue.
        /// </summary>
        /// <returns>An optional coroutine that shouold be started by the calling MonoBehaviour</returns>
        public virtual IEnumerator Prompt(ActorController actor)
        {
            m_Actor = actor;

            ProcessMove();
            ProcessAudio();
            ProcessAnimationLayerWeights();
            ProcessAnimationParameters();
            ProcessAnimationClips();

            return UpdateCoroutine();
        }

        private void ProcessAnimationLayerWeights()
        {
            m_LayerIndex = m_Actor.Animator.GetLayerIndex(m_LayerName);
        }

        internal IEnumerator UpdateCoroutine()
        {
            // Process Layers
            if (m_LayerIndex >= 0 && m_Actor.Animator.GetLayerWeight(m_LayerIndex) != m_LayerWeight)
            {
                float originalWeight = m_Actor.Animator.GetLayerWeight(m_LayerIndex);
                float currentWeight = originalWeight;
                while (!Mathf.Approximately(currentWeight, m_LayerWeight))
                {
                    currentWeight = m_Actor.Animator.GetLayerWeight(m_LayerIndex);
                    float delta = (m_LayerWeight - currentWeight) * (Time.deltaTime * m_LayerChangeSpeed);
                    m_Actor.Animator.SetLayerWeight(m_LayerIndex, currentWeight + delta);
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /// <summary>
        /// If this cue has any animation parameter changes then have an actor make those changes.
        /// </summary>
        /// <param name="m_Actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationParameters()
        {
            if (!string.IsNullOrWhiteSpace(paramName))
            {
                switch (paramType)
                {
                    case ActorCue.ParameterType.Trigger:
                        m_Actor.Animator.SetTrigger(paramName);
                        break;
                    case ActorCue.ParameterType.Bool:
                        m_Actor.Animator.SetBool(paramName, paramBoolValue);
                        break;
                    case ActorCue.ParameterType.Int:
                        m_Actor.Animator.SetInteger(paramName, paramIntValue);
                        break;
                    case ActorCue.ParameterType.Float:
                        m_Actor.Animator.SetBool(paramName, paramBoolValue);
                        break;
                }
            }
        }

        /// <summary>
        /// The name of an animation clip to play upon this cue.
        /// </summary>
        /// <param name="m_Actor">The actor to enact the animation changes.</param>
        private void ProcessAnimationClips()
        {
            if (!string.IsNullOrWhiteSpace(animationClipName))
            {
                m_Actor.Animator.Play(animationClipName, animationLayer, animationNormalizedTime);
            }
        }

        /// <summary>
        /// If this cue has a mark defined move to it.
        /// </summary>
        void ProcessMove()
        {
            if (!string.IsNullOrWhiteSpace(markName))
            {
                NavMeshAgent agent = m_Actor.GetComponent<NavMeshAgent>();
                GameObject go = GameObject.Find(markName);
                if (go != null)
                {
                    agent.SetDestination(go.transform.position);
                } else
                {
                    Debug.LogWarning(m_Actor.name + "  has a mark set, but the mark doesn't exist in the scene. The name set is " + markName);
                }
            }
        }

        void ProcessAudio()
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
