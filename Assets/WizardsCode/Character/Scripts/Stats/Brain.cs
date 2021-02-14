using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WizardsCode.Character;
using WizardsCode.Character.Stats;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Stats {
    /// <summary>
    /// The Brain is responsible for tracking the stats and goal states of the character and
    /// making decisions and plans to reach those goal stats.
    /// </summary>
    public class Brain : StatsTracker
#if UNITY_EDITOR
        , IDebug
#endif
    {
        ActorController m_Controller;
        AbstractAIBehaviour[] m_Behaviours = default;
        private Interactable m_TargetInteractable;

        public AbstractAIBehaviour CurrentBehaviour { get; set; }

        public MemoryController Memory { get; private set; }

        internal Interactable TargetInteractable
        {
            get { return m_TargetInteractable; }
            set
            {
                if (Actor != null
                    && value != null)
                {
                    if (value.ReserveFor(this))
                    {
                        //TODO move to an interaction point not to the transform position
                        Actor.TargetPosition = value.transform.position;
                    }
                }

                m_TargetInteractable = value;
            }
        }

        public ActorController Actor {
            get { return m_Controller; } 
        }

        private void Awake()
        {
            m_Controller = GetComponent<ActorController>();
            Memory = GetComponentInChildren<MemoryController>();
            m_Behaviours = GetComponentsInChildren<AbstractAIBehaviour>();
        }

        /// <summary>
        /// Decide whether the actor should interact with an influencer trigger they just entered.
        /// </summary>
        /// <param name="interactable">The influencer trigger that was activated and can now be interacted with.</param>
        /// <returns></returns>
        internal bool ShouldInteractWith(Interactable interactable)
        {
            if (interactable != null && GameObject.ReferenceEquals(interactable, TargetInteractable))
            {
                return true;
            } else
            {
                return false;
            }
        }

        internal override void Update()
        {
            if (Time.timeSinceLevelLoad < m_TimeOfNextUpdate) return;

            if (TargetInteractable != null && Vector3.SqrMagnitude(TargetInteractable.transform.position - Actor.TargetPosition) > 0.7f)
            {
                Actor.TargetPosition = TargetInteractable.transform.position;
            }

            base.Update();

            UpdateActiveBehaviour();
        }

        /// <summary>
        /// Iterates over all the behaviours available to this actor and picks the most important one to be executed next.
        /// </summary>
        private void UpdateActiveBehaviour()
        {
            if (CurrentBehaviour != null && CurrentBehaviour.IsExecuting && !CurrentBehaviour.IsInteruptable) return;

            bool isInterupting = false;
            if (CurrentBehaviour != null && CurrentBehaviour.IsExecuting)
            {
                isInterupting = true;
            }

            StringBuilder log = new StringBuilder();
            AbstractAIBehaviour candidateBehaviour = null;
            float highestWeight = float.MinValue;
            float currentWeight = 0;

            for (int i = 0; i < m_Behaviours.Length; i++)
            {
                log.Append("Considering: ");
                log.AppendLine(m_Behaviours[i].DisplayName);
                if (m_Behaviours[i].IsAvailable)
                {
                    log.AppendLine(m_Behaviours[i].reasoning.ToString());

                    currentWeight = m_Behaviours[i].Weight(this);
                    if (currentWeight > highestWeight)
                    {
                        candidateBehaviour = m_Behaviours[i];
                        highestWeight = currentWeight;
                    }
                }
                log.AppendLine(m_Behaviours[i].reasoning.ToString());
            }

            if (candidateBehaviour == null) return;

            if (isInterupting && candidateBehaviour != CurrentBehaviour)
            {
                CurrentBehaviour.FinishBehaviour();
            }

            CurrentBehaviour = candidateBehaviour;
            CurrentBehaviour.EndTime = 0;
            CurrentBehaviour.IsExecuting = true;
            if (CurrentBehaviour is GenericInteractionAIBehaviour)
            {
                TargetInteractable = ((GenericInteractionAIBehaviour)CurrentBehaviour).CurrentInteractableTarget;
            } else
            {
                TargetInteractable = null;
                CurrentBehaviour.StartBehaviour(CurrentBehaviour.AbortDuration);
            }

            log.Insert(0, "\n");
            if (TargetInteractable != null)
            {
                log.Insert(0, TargetInteractable.name);
                log.Insert(0, " at ");
                log.Insert(0, TargetInteractable.InteractionName);
            }
            else
            {
                log.Insert(0, CurrentBehaviour.DisplayName);
            }
            log.Insert(0, " decided to ");
            log.Insert(0, DisplayName);

            Log(log.ToString());
        }

        /// <summary>
        /// Add an influencer to this controller. If this controller is not managing the required stat then 
        /// do nothing.
        /// </summary>
        /// <param name="influencer">The influencer to add.</param>
        /// <returns>True if the influencer was added, otherwise false.</returns>
        public override bool TryAddInfluencer(StatInfluencerSO influencer)
        {
            if (Memory != null && influencer.Trigger != null)
            {
                MemorySO[] memories = Memory.GetAllMemoriesAbout(influencer.Generator);
                for (int i = 0; i < memories.Length; i++)
                {
                    if (memories[i].stat == influencer.stat && memories[i].time + memories[i].cooldown > Time.timeSinceLevelLoad)
                    {
                        return false;
                    }
                }
            }

            if (Memory != null)
            {
                StatSO stat = GetOrCreateStat(influencer.stat);
                List<StateSO> states = GetDesiredStatesFor(stat);
                bool isGood = true;
                for (int i = 0; i < states.Count; i++)
                {
                    switch (states[i].objective)
                    {
                        case StateSO.Objective.LessThan:
                            if (influencer.maxChange > 0)
                            {
                                isGood = false;
                            }
                            break;
                        case StateSO.Objective.Approximately:
                            float currentDelta = states[i].normalizedTargetValue - stat.NormalizedValue;
                            float influencedDelta = states[i].normalizedTargetValue - (stat.NormalizedValue + influencer.maxChange);
                            if (currentDelta < influencedDelta)
                            {
                                isGood = false;
                            }
                            break;
                        case StateSO.Objective.GreaterThan:
                            if (influencer.maxChange < 0)
                            {
                                isGood = false;
                            }
                            break;
                    }

                    if (influencer.Generator != null)
                    {
                        Memory.AddMemory(influencer, isGood);
                    }
                }
            }

            return base.TryAddInfluencer(influencer);
        }

        /// <summary>
        /// Record a decision or action in the log.
        /// </summary>
        /// <param name="log"></param>
        private void Log(string log)
        {
            if (string.IsNullOrEmpty(log)) return;

            //TODO don't log to console, log to a characters history
            Debug.Log(log);
        }

#if UNITY_EDITOR
        string IDebug.StatusText()
        {
            string msg = DisplayName;
            msg += "\n\nStats";
            for (int i = 0; i < m_Stats.Count; i++)
            {
                msg += "\n" + m_Stats[i].statusDescription;
            }

            msg += GetActiveInfluencersDescription();

            msg += "\n\nUnsatisfied Desired States";
            if (UnsatisfiedDesiredStates.Length == 0) msg += "\nNone";
            for (int i = 0; i < UnsatisfiedDesiredStates.Length; i++)
            {
                StatSO stat = GetOrCreateStat(UnsatisfiedDesiredStates[i].statTemplate);
                msg += "\nIs not ";
                msg += UnsatisfiedDesiredStates[i].name + " ";
                msg += " (" + stat.name + " should be " + UnsatisfiedDesiredStates[i].objective + " " + UnsatisfiedDesiredStates[i].normalizedTargetValue + ")";
            }

            if (Memory == null)
            {
                msg += "\n\nThis actor has no memory.";
            } else
            {
                msg += "\n\nThis actor has a memory.";
                msg += "\nShort term memories: " + Memory.GetShortTermMemories().Length;
                msg += "\nLong term memories: " + Memory.GetLongTermMemories().Length;
            }

            if (CurrentBehaviour != null)
            {
                float timeLeft = Mathf.Clamp(CurrentBehaviour.EndTime - Time.timeSinceLevelLoad, 0, float.MaxValue);
                msg += "\n\nCurrent Behaviour";
                msg += "\n" + CurrentBehaviour + " (time to abort / end " + timeLeft.ToString("0.0") + ")";
                if (TargetInteractable != null)
                {
                    msg += "\nTarget interaction: " + TargetInteractable.InteractionName + " at " + TargetInteractable.name;
                }
                else
                {
                    msg += "\nNo target interaction";
                }
            } else
            {
                msg += "\n\nCurrent Behaviour";
                msg += "\nNone";
            }

            return msg;
        }
#endif
    }
}
