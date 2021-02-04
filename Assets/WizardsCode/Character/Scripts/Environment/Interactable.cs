using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using WizardsCode.Character.Stats;
using System;
using static WizardsCode.Character.StateSO;

namespace WizardsCode.Character
{
    /// <summary>
    /// Marks an object as interactable so that a actors can find them. 
    /// Records the effects the interactable can have on an actor when
    /// interacting.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        StatsInfluencerTrigger m_Influencer;

        /// <summary>
        /// The time it takes, under normal circumstances, to interact with this thing.
        /// </summary>
        public float Duration {
            get
            {
                if (m_Influencer == null) return 0;

                return m_Influencer.Duration;
            }
        }

        void Awake()
        {
            m_Influencer = GetComponent<StatsInfluencerTrigger>();
        }

        /// <summary>
        /// Test to see if this interactable will affect a state in a way that
        /// is desired.
        /// </summary>
        /// <param name="stateImpact">The desired state impact</param>
        /// <returns>True if the desired impact will result from interaction, otherwise false.</returns>
        public bool Influences(DesiredStatImpact stateImpact) {
            if (m_Influencer == null) return false;

            bool result = m_Influencer.StatTemplate.name == stateImpact.statTemplate.name;
            switch (stateImpact.objective)
            {
                case Objective.LessThan:
                    result &= m_Influencer.MaxChange < 0;
                    break;
                case Objective.Approximately:
                    result &= Mathf.Approximately(m_Influencer.MaxChange, 0);
                    break;
                case Objective.GreaterThan:
                    result &= m_Influencer.MaxChange > 0;
                    break;
                default:
                    Debug.LogError("Don't know how to handle objective " + stateImpact.objective);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Test to see if the interactable is on cooldown for a given actor.
        /// </summary>
        /// <param name="brain">The brain of the actor we are testing for.</param>
        /// <returns>True if this influencer is on cooldown, meaning the actor cannot use it yet.</returns>
        internal virtual bool IsOnCooldownFor(Brain brain)
        {
            if (m_Influencer == null) return false;

            return m_Influencer.IsOnCooldownFor(brain);
        }
    }
}
