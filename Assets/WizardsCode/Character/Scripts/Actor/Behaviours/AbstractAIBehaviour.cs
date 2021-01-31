using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using System;
using Random = UnityEngine.Random;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        RequiredState[] m_RequiredStates = new RequiredState[0];
        [SerializeField, Tooltip("The duration within which the actor will be prevented from starting another behaviour.")]
        float m_Duration = 5;

        internal float awarenessRange = 50;

        internal Brain brain;
        internal ActorController controller;

        public RequiredState[] requiredStates {
            get {return m_RequiredStates;}
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Tests to see if this behaviour is availble to be executed. That is are the necessary preconditions
        /// met.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                for (int i = 0; i < m_RequiredStates.Length; i++)
                {
                    if (m_RequiredStates[i].invert)
                    {
                        if (m_RequiredStates[i].state.IsSatisfiedFor(brain)) return false;
                    }
                    else
                    {
                        if (!m_RequiredStates[i].state.IsSatisfiedFor(brain)) return false;
                    }
                }

                return true;
            }
        }

        private bool m_IsExecuting = false;
        private float m_StartTime;

        /// <summary>
        /// Is this behaviour the currently executing behaviour?
        /// </summary>
        public bool IsExecuting {
            get { return m_IsExecuting; }
            internal set
            {
                if (value && !m_IsExecuting)
                {
                    m_StartTime = 0;
                }

                m_IsExecuting = value;
            }
        }

        /// <summary>
        /// Called when the behaviour is started, from the `Start` method of the underlying
        /// `MonoBehaviour`.
        /// </summary>
        protected virtual void Init()
        {
            brain = GetComponentInParent<Brain>();
            if (brain == null)
            {
                if (m_RequiredStates.Length > 0)
                {
                    Debug.LogError(gameObject.name + " has required states defined but has no StatsController against which to check these states.");
                }
            }
            controller = GetComponentInParent<ActorController>();
        }

        public void Update()
        {
            if (!IsExecuting) return;
            OnUpdate();
        }

        /// <summary>
        /// Calculates the current weight for this behaviour between 0 (don't execute)
        /// and 1 (really want to execute). By default this is directly proportional to,
        /// the number of unsatisfied desired states in the brain that this behaviour 
        /// impacts.
        /// 
        /// If there are no unsatisfiedDesiredStates then the weight will be 0.
        /// 
        /// This should nearly always be overridden in specific behaviour implementations.
        /// </summary>
        public virtual float Weight(Brain brain)
        {
            if (brain.UnsatisfiedDesiredStates.Length == 0) return 0f;

            float weight = 0;
            for (int i = 0; i < brain.UnsatisfiedDesiredStates.Length; i++)
            {
                for (int idx = 0; idx < requiredStates.Length; idx++)
                {
                    if (brain.UnsatisfiedDesiredStates[i].name == requiredStates[idx].state.name) weight++;
                }
            }
            return weight / brain.UnsatisfiedDesiredStates.Length;
        }

        internal Interactable[] GetNearbyInteractablesFor(RequiredState[] requiredStates)
        {
            //TODO Cache the interactables near the current location
            //TODO Put interactables on a layer to make the physics operation fsater
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, awarenessRange);
            List<Interactable> availableInteractables = new List<Interactable>();
            Interactable currentInteractable;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractable = hitColliders[i].GetComponent<Interactable>();
                //TODO need to get interactables that affect the state in the way desired (e.g. increase or decrease)
                if (currentInteractable != null && currentInteractable.Influences(requiredStates[0].state.statTemplate))
                {
                    availableInteractables.Add(currentInteractable);
                }
            }

            return availableInteractables.ToArray();
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. By default this will look
        /// for interactables nearby that will satisfy the needs of this behaviour.
        /// </summary>
        protected virtual void OnUpdate()
        {
            if (m_StartTime == 0)
            {
                m_StartTime = Time.timeSinceLevelLoad;

                Interactable[] availableInteractables = GetNearbyInteractablesFor(requiredStates);
                if (availableInteractables.Length == 0)
                {
                    return;
                }

                //TODO find the nearest place in memory that one can influence the required states

                //TODO select the optimal place to eat (distance and amount of influence)
                int idx = Random.Range(0, availableInteractables.Length);

                brain.TargetInteractable = availableInteractables[idx];

                //TODO block lower priority actions until done
            } else
            {
                if (m_StartTime + m_Duration <= Time.timeSinceLevelLoad)
                {
                    IsExecuting = false;
                    m_StartTime = 0;
                }
            }
        }
    }
    
    [Serializable]
    public struct RequiredState
    {
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        public StateSO state;
        [SerializeField, Tooltip("If set to true the state will be required to be inactive.")]
        public bool invert;
    } 
}
