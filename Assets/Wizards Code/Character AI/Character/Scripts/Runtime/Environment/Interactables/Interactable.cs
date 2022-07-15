using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using WizardsCode.Character.Stats;
using System;
using static WizardsCode.Character.StateSO;
using UnityEngine.Serialization;
using WizardsCode.Character.WorldState;

namespace WizardsCode.Character
{
    /// <summary>
    /// Marks an object as interactable so that a actors can find them. 
    /// Records the effects the interactable can have on an actor and
    /// the object itself when interacting.
    /// 
    /// A stats tracker required in an ancestor.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour
    {
        #region Inspector Values
        [Header("Overview")]
        [SerializeField, TextArea, Tooltip("A description of this interactable action.")]
        string m_Description;
        [SerializeField, Tooltip("The type of this interactable, this is used for sorting and filtering world state. This should represent the primary purpose of this interactable, there may be other interactables on the same object and there may be additional effects from this interactable. However, the type represents the primary purpose.")]
        InteractableTypeSO m_Type;
        [SerializeField, Tooltip("The name of the interaction from the perspective of the actor interacting with this item.")]
        [FormerlySerializedAs("m_InteractionName")]
        string m_InteractionNameFromActorsPerspective = "";

        [Header("Character Settings")]
        [SerializeField, Tooltip("How many characters can interact using this influencer at any one time.")]
        int m_MaxInteractors = 1;
        [SerializeField, Tooltip("The set of character stats and the influence to apply to them when a character interacts with the object.")]
        internal StatInfluence[] m_CharacterInfluences;
        [SerializeField, Tooltip("The position an actor should be in to interact with this interactable. If this is left blank then a best guess interaction point will be selected.")]
        internal Transform m_InteractionPoint;

        [Header("Object Settings")]
        [SerializeField, Tooltip("When an actor has finished interacting with the object should the object be destroyed?")]
        bool m_DestroyOnUse = false;
        [SerializeField, Tooltip("The set of object stats and the influence to apply to them when a character interacts with the object.")]
        internal StatInfluence[] m_ObjectInfluences;
        [SerializeField, Tooltip("The cooldown time before a character can be influenced by this influencer again.")]
        float m_Cooldown = 30;
        #endregion

        #region Variables
        List<StatsTracker> m_Reservations = new List<StatsTracker>();

        StatsTracker m_StatsTracker;
        private Dictionary<StatsTracker, float> m_TimeOfLastInfluence = new Dictionary<StatsTracker, float>();
        private List<StatsTracker> m_ActiveInteractors = new List<StatsTracker>();
        #endregion

        # region Properties
        /// <summary>
        /// Get the position and rotation that an Actor should take in order to
        /// interact with this interactable;
        /// </summary>
        [Obsolete("Use GetInteractionPointFor(Actor) instead.")] // 6/23/22
        public virtual Transform interactionPoint
        {
            get
            {
                return GetInteractionPointFor(null);
            }
        }

        public virtual Transform GetInteractionPointFor(BaseActorController actor)
        {
            if (m_InteractionPoint == null)
            {
                float distance = float.MinValue;
                Collider[] colliders = GetComponents<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].isTrigger && distance < colliders[i].bounds.extents.x)
                    {
                        distance = colliders[i].bounds.extents.x * 0.8f;
                    }
                    else
                    {
                        distance = 1;
                    }
                }
                m_InteractionPoint = new GameObject("Interaction Point").transform;
                m_InteractionPoint.position = transform.position + transform.forward * distance;
                m_InteractionPoint.rotation = Quaternion.LookRotation(transform.position - m_InteractionPoint.transform.position, Vector3.up);
                m_InteractionPoint.SetParent(transform);
            }

            return m_InteractionPoint;
        }

        /// <summary>
        /// Get the StatInfluences that act upon a character interacting with this item.
        /// </summary>
        public StatInfluence[]  CharacterInfluences {
            get { return m_CharacterInfluences; }
        }

        public string DisplayName
        {
            //TODO allow the designer to customize this name
            get { return gameObject.name; }
        }

        /// <summary>
        /// The name of this interaction. Used as an ID for this interaction.
        /// </summary>
        public string InteractionName
        {
            get { return m_InteractionNameFromActorsPerspective; }
            set { m_InteractionNameFromActorsPerspective = value; }
        }

        /// <summary>
        /// Get the StatInfluences that act upon this object when a character interacts with this item.
        /// </summary>
        public StatInfluence[] ObjectInfluences
        {
            get { return m_ObjectInfluences; }
        }

        public InteractableTypeSO Type 
        { 
            get { return m_Type; }
        }
        #endregion

        #region Lifecycle
        void Awake()
        {
            m_StatsTracker = GetComponentInParent<StatsTracker>();
        }

        private void Start()
        {
            InteractableManager.Instance.Register(this);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == this.gameObject) return;

            Brain brain = other.transform.root.GetComponentInChildren<Brain>();
            if (brain == null || !brain.ShouldInteractWith(this))
            {
                return;
            }

            StartCharacterInteraction(brain);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject == this.gameObject) return;

            Brain brain = other.transform.root.GetComponentInChildren<Brain>();
            if (brain == null
                || brain.ActiveBlockingBehaviour == null
                || brain.ActiveBlockingBehaviour.CurrentState != AbstractAIBehaviour.State.MovingTo
                || !brain.ShouldInteractWith(this))
            {
                return;
            }

            StartCharacterInteraction(brain);
        }
        #endregion

        #region Availability
        /// <summary>
        /// Reserve this interactable for a given actor. This actor should
        /// be on their way to the interactable. Only a limited number of actors can reserve
        /// this interactable. Once a reservation is no longer needed then call to ClearReservation(brain).
        /// </summary>
        /// <param name="statsTracker">The actor who reseved this interactable.</param>
        /// <returns>True if the the reservation was succesful.</returns>
        internal bool ReserveFor(StatsTracker statsTracker)
        {
            if (m_Reservations.Count + m_ActiveInteractors.Count >= m_MaxInteractors)
            {
                return false;
            }

            m_Reservations.Add(statsTracker);
            return true;
        }

        /// <summary>
        /// Clears any reservation for this interactable by an actor using
        /// the ReservedFor(brain) method;
        /// </summary>
        internal void ClearReservation(StatsTracker stats)
        {
            m_Reservations.Remove(stats);
        }

        /// <summary>
        /// Test to see if this interactable will affect a state in a way that
        /// is desired.
        /// </summary>
        /// <param name="stateImpact">The desired state impact</param>
        /// <returns>True if the desired impact will result from interaction, otherwise false.</returns>
        public bool HasInfluenceOn(DesiredStatImpact stateImpact) {
            for (int i = 0; i < CharacterInfluences.Length; i++)
            {
                if (CharacterInfluences[i].statTemplate.name == stateImpact.statTemplate.name)
                {
                    switch (stateImpact.objective)
                    {
                        case Objective.LessThan:
                            return CharacterInfluences[i].maxChange < 0;
                        case Objective.Approximately:
                            return Mathf.Approximately(CharacterInfluences[i].maxChange, 0);
                        case Objective.GreaterThan:
                            return CharacterInfluences[i].maxChange > 0;
                        default:
                            Debug.LogError("Don't know how to handle objective " + stateImpact.objective);
                            break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Test to see if this influencer trigger is on cooldown for a given actor.
        /// </summary>
        /// <param name="stats">The Stats of the interacting object we are testing against</param>
        /// <returns>True if this influencer is on cooldown, meaning the actor cannot use it yet.</returns>
        internal bool IsOnCooldownFor(StatsTracker stats)
        {
            float lastTime;
            if (m_TimeOfLastInfluence.TryGetValue(stats, out lastTime))
            {
                return Time.timeSinceLevelLoad < lastTime + m_Cooldown;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tests to see if the interactable has space in the reservation queue 
        /// for this actor, or has an existing reservation for it.
        /// </summary>
        /// <param name="stats">The StatsTracker of the object we are testing for</param>
        /// <returns></returns>
        public bool HasSpaceFor(StatsTracker stats)
        {
            bool hasSpace = m_MaxInteractors > m_ActiveInteractors.Count + m_Reservations.Count; 
            if (hasSpace)
            {
                return true;
            } else
            {
                return m_Reservations.Contains(stats);
            }
        }

        internal bool HasRequiredObjectStats()
        {
            bool isValid = true;
            for (int i = 0; i < ObjectInfluences.Length; i++)
            {
                if (ObjectInfluences[i].maxChange < 0 
                    && m_StatsTracker.GetOrCreateStat(ObjectInfluences[i].statTemplate).Value < Math.Abs(ObjectInfluences[i].maxChange))
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }
        #endregion

        #region Interaction
        internal void StartCharacterInteraction(StatsTracker stats)
        {
            if (!HasSpaceFor(stats))
            {
                stats.ClearTarget();
                return;
            }

            if (stats is Brain)
            {
                Brain brain = (Brain)stats;
                GenericInteractionBehaviour behaviour = (GenericInteractionBehaviour)brain.ActiveBlockingBehaviour;
                behaviour.StartBehaviour(this);
            }

            for (int i = 0; i < CharacterInfluences.Length; i++)
            {
                StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                influencer.InteractionName = CharacterInfluences[i].statTemplate.name + " influencer from " + InteractionName + " (" + GetInstanceID() + ")";
                influencer.Trigger = this;
                influencer.stat = CharacterInfluences[i].statTemplate;
                influencer.maxChange = CharacterInfluences[i].maxChange;
                influencer.CooldownDuration = m_Cooldown;

                if (stats.TryAddInfluencer(influencer))
                {
                    m_Reservations.Remove(stats);
                    m_ActiveInteractors.Add(stats);
                    m_TimeOfLastInfluence.Remove(stats);
                    m_TimeOfLastInfluence.Add(stats, Time.timeSinceLevelLoad);
                }
            }

            AddObjectInfluence();
        }

        internal void StopCharacterInteraction(StatsTracker statsTracker)
        {
            m_ActiveInteractors.Remove(statsTracker);
            if (m_DestroyOnUse)
            {
                if (statsTracker is Brain)
                {
                    ((Brain)statsTracker).Actor.ResetLookAt();
                }
                Destroy(gameObject, 0.05f);
            }
        }

        private void AddObjectInfluence()
        {
            for (int i = 0; i < ObjectInfluences.Length; i++)
            {
                StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
                influencer.InteractionName = ObjectInfluences[i].statTemplate.name + " influencer from " + InteractionName + " (" + GetInstanceID() + ")";
                influencer.Trigger = this;
                influencer.stat = ObjectInfluences[i].statTemplate;
                influencer.maxChange = ObjectInfluences[i].maxChange;
                influencer.CooldownDuration = m_Cooldown;

                m_StatsTracker.TryAddInfluencer(influencer);
            }
        }
        #endregion
    }
}
