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
                if (m_Controller != null
                    && value != null)
                {
                    if (value.ReserveFor(this))
                    {
                        //TODO move to an interaction point not to the transform position
                        m_Controller.TargetPosition = value.transform.position;
                    }
                }

                m_TargetInteractable = value;
            }
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

            if (TargetInteractable != null && Vector3.SqrMagnitude(TargetInteractable.transform.position - m_Controller.TargetPosition) > 0.7f)
            {
                m_Controller.TargetPosition = TargetInteractable.transform.position;
            }

            base.Update();

            UpdateActiveBehaviour();
        }

        /// <summary>
        /// Iterates over all the behaviours available to this actor and picks the most important one to be executed next.
        /// </summary>
        private void UpdateActiveBehaviour()
        {
            //TODO Allow tasks to be interuptable
            if (CurrentBehaviour != null && CurrentBehaviour.IsExecuting) return;

            StringBuilder log = new StringBuilder();
            AbstractAIBehaviour candidateBehaviour = null;
            float highestWeight = float.MinValue;
            float currentWeight = 0;

            log.Append(DisplayName);
            log.AppendLine(" considered what to do next.");

            for (int i = 0; i < m_Behaviours.Length; i++)
            {
                if (m_Behaviours[i].IsAvailable)
                {
                    log.Append("Option ");
                    log.AppendLine((i + 1).ToString());
                    log.AppendLine(m_Behaviours[i].DisplayName);
                    log.AppendLine(m_Behaviours[i].reasoning.ToString());

                    currentWeight = m_Behaviours[i].Weight(this);
                    if (currentWeight > highestWeight)
                    {
                        candidateBehaviour = m_Behaviours[i];
                        highestWeight = currentWeight;
                    }
                }
            }

            if (candidateBehaviour == null) return;

            CurrentBehaviour = candidateBehaviour;
            CurrentBehaviour.IsExecuting = true;
            TargetInteractable = CurrentBehaviour.CurrentInteractableTarget;

            log.Append(DisplayName);
            log.Append(" finally decided to ");
            log.Append(TargetInteractable.InteractionName);
            log.Append(" at ");
            log.Append(TargetInteractable.name);

            //TODO don't log brains thinking to console, cache X decisions and write the rest to a file.
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

                    Memory.AddMemory(influencer, isGood);
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

            msg += "\n\nCurrent Behaviour";
            msg += "\n" + CurrentBehaviour + " (time to abort / end " + (CurrentBehaviour.EndTime - Time.timeSinceLevelLoad).ToString("0.0") + ")";
            if (TargetInteractable != null)
            {
                msg += "\nTarget interactable: " + TargetInteractable.InteractionName + " at " + TargetInteractable.name;
            } else
            {
                msg += "\nNo target interactable";
            }

            return msg;
        }
#endif
    }
}
