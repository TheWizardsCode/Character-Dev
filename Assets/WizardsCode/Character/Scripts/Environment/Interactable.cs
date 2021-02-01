using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using WizardsCode.Character.Stats;
using System;

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

        public bool Influences(StatSO stat) {
            if (m_Influencer == null) return false;

            return m_Influencer.Stat.name == stat.name;
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
