using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using WizardsCode.Stats;
using UnityEditor;
using UnityEngine.AI;

namespace WizardsCode.Character
{
    /// <summary>
    /// This generic AI behaviour will seek an interactable within a defined range that will
    /// impact a specific set of states. The most appropriate interactable will be selected
    /// and the actor will move towards it.
    /// 
    /// Upon arrival the interactable should impart an influencer on the actor.
    /// </summary>
    public class GenericInteractionBehaviour : AbstractAIBehaviour
    {
        [Header("Interactables")]
        [SerializeField, Tooltip("The range within which the Actor can sense interactables that this behaviour can impact. This does not affect interactables that are recalled from memory.")]
        float awarenessRange = 10;

        private List<Interactable> cachedAvailableInteractables = new List<Interactable>();
        private Vector3 positionAtLastInteractableCheck = Vector3.zero;
        private List<Interactable> nearbyInteractablesCache = new List<Interactable>();
        internal Interactable CurrentInteractableTarget = default;
        private Interactable m_Interactable;

        /// <summary>
        /// Start an interaction with a given object as part of this behaviour. This is
        /// where animations, sounds, FX and similar should be started.
        /// </summary>
        /// <param name="interactable">The interactable we are working on.</param>
        internal virtual void StartBehaviour(Interactable interactable)
        {
            m_ActorController.LookAtTarget = interactable.transform;
            m_Interactable = interactable;

            base.StartBehaviour();
        }

        public override void Update()
        {
            if (CurrentState == State.MovingTo || CurrentState == State.Inactive)
            {
                return;
            }

            if (m_timeline)
            {
                if (CurrentState == State.Starting)
                {
                    Brain.Actor.TurnToFace(m_Interactable.transform.position);
                }
            }

            base.Update();
        }

        protected override void OnUpdateState()
        {
            if (CurrentState == State.Starting)
            {
                Brain.Actor.TurnToFace(m_Interactable.transform.position);
            }
            base.OnUpdateState();
        }

        public override bool IsAvailable
        {
            get
            {
                if (!base.IsAvailable) return false;

                UpdateAvailableInteractablesCache();

                // Check there is a valid interactable
                if (cachedAvailableInteractables.Count == 0)
                {
                    CurrentInteractableTarget = null;
                }
                else
                {
                    float nearestMag = float.MaxValue;
                    //TODO select the optimal interactible based on distance and amount of influence
                    for (int interactablesIndex = 0; interactablesIndex < cachedAvailableInteractables.Count; interactablesIndex++)
                    {
                        float mag = Vector3.SqrMagnitude(transform.position - cachedAvailableInteractables[interactablesIndex].transform.position);
                        if (mag < nearestMag)
                        {
                            nearestMag = mag;
                            CurrentInteractableTarget = cachedAvailableInteractables[interactablesIndex];
                        }
                    }
                }

                if (CurrentInteractableTarget == null)
                {
                    reasoning.AppendLine("Couldn't see or recall a suitable interactable nearby, will have to find one first.");
                    return false;
                }
                else
                {
                    reasoning.Append("Maybe go to ");
                    reasoning.Append(CurrentInteractableTarget.DisplayName);
                    reasoning.Append(" to ");
                    reasoning.AppendLine(DisplayName);
                    return true;
                }
            }
        }

        internal override float Weight(Brain brain)
        {
            if (CurrentInteractableTarget != null)
            {
                return base.Weight(brain);
            } else
            {
                return base.Weight(brain) / 2;
            }
        }

        /// <summary>
        /// Updates the cache of interractables in the area and from memory that can be used by this
        /// behaviour. Only interactables that have the desired influences on the actor are returned.
        /// </summary>
        // OPTIMIZATION: make this a coroutine as it could be costly and we therefore shouldn't block on it
        private void UpdateAvailableInteractablesCache()
        {
            cachedAvailableInteractables.Clear();

            //TODO share cached interactables across all behaviours and only update if character has moved more than 1 unity
            List<Interactable> candidateInteractables = GetNearbyInteractables();

            // Iterate over them keeping only the ones that satsify all desiredStateImpacts
            for (int i = 0; i < candidateInteractables.Count; i++)
            {
                if (IsValidInteractable(candidateInteractables[i]))
                {
                    cachedAvailableInteractables.Add(candidateInteractables[i]);
                }
            }

            if (cachedAvailableInteractables.Count ==0 && Memory != null)
            {
                //TODO rather than get all memories and then test for DesiredStateImpact add a method to do it in one pass
                MemorySO[] memories = Memory.GetAllMemories(awarenessRange * 5);
                Interactable interactable;
                for (int i = 0; i < memories.Length; i++)
                {
                    if (memories[i].about)
                    {
                        interactable = memories[i].about.GetComponentInChildren<Interactable>();

                        reasoning.Append("I remember ");
                        reasoning.Append(interactable.DisplayName);

                        //TODO if memory is of an already cached interactable we can skip

                        if (IsValidInteractable(interactable))
                        {
                            cachedAvailableInteractables.Add(interactable);
                            reasoning.AppendLine(" is near here, that's a good place.");
                        }
                        else
                        {
                            reasoning.AppendLine(" is near here, but it's not a suitable place.");
                        }
                    } else
                    {
                        List<Interactable> interactables = GetInteractiablesNear(memories[i].position, awarenessRange);
                        if (interactables.Count > 0)
                        {
                            for (int y = 0; y < interactables.Count; y++)
                            {
                                if (IsValidInteractable(interactables[y]))
                                {
                                    cachedAvailableInteractables.Add(interactables[y]);
                                    reasoning.AppendLine(" is near here, and it was a suitable place before, maybe it is now.");
                                }
                            }
                        } 
                        else
                        {
                            reasoning.AppendLine(" is near here, but I don't think there is anything left for me there.");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Does the interactable offer the desired impact and does it have the required stats 
        /// to deliver the objects influence?
        /// That is, if a behaviour requires 10 cash to be delievered does the interactable have
        /// 10 cash to deliver?
        /// </summary>
        /// <param name="interactable">The interactable to be tested</param>
        /// <returns>True if the interactable can deliver on all desired influences</returns>
        private bool IsValidInteractable(Interactable interactable)
        {
            if (!HasDesiredImpact(interactable))
            {
                return false;
            }

            reasoning.Append(interactable.name);
            reasoning.Append(" might be a good place to ");
            reasoning.AppendLine(interactable.InteractionName);

            if (!interactable.HasSpaceFor(Brain))
            {
                reasoning.AppendLine("Looks like it is full.");
                return false;
            }

            if (interactable.IsOnCooldownFor(Brain))
            {
                reasoning.AppendLine("I Went there recently, let's try somewhere different.");
                return false;
            }

            if (!interactable.HasRequiredObjectStats())
            {
                reasoning.AppendLine("Looks like they don't have what I need.");
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(m_ActorController.transform.position, interactable.GetInteractionPointFor(ActorController).position, NavMesh.AllAreas, path))
            {
                reasoning.AppendLine("Unfortunately it doesn't look like I can reach there from here.");
                return false;
            }

            reasoning.AppendLine("Looks like a good candidate.");
            return true;
        }

        /// <summary>
        /// Scan for nearby interactables that have capacity for an actor.
        /// </summary>
        /// <returns>A list of interactables within range that have space for an actor.</returns>
        internal List<Interactable> GetNearbyInteractables()
        {
            if (positionAtLastInteractableCheck != Vector3.zero
                && Vector3.SqrMagnitude(positionAtLastInteractableCheck - transform.position) <= 1)
            {
                positionAtLastInteractableCheck = transform.position;
                return nearbyInteractablesCache;
            }

            nearbyInteractablesCache = GetInteractiablesNear(transform.position, awarenessRange);

            return nearbyInteractablesCache;
        }

        // REFACTOR this code is duplicated in MemoryController
        internal List<Interactable> GetInteractiablesNear(Vector3 position, float range)
        {
            List<Interactable> interactables = new List<Interactable>();

            //OPTIMIZATION Put interactables on a layer to make the physics operation faster
            Collider[] hitColliders = Physics.OverlapSphere(position, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            Interactable[] currentInteractables;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                currentInteractables = hitColliders[i].GetComponentsInParent<Interactable>();
                for (int idx = 0; idx < currentInteractables.Length; idx++)
                {
                    if (currentInteractables[idx].HasSpaceFor(Brain))
                    {
                        interactables.Add(currentInteractables[idx]);
                    }
                }
            }

            return interactables;
        }
    }
}
