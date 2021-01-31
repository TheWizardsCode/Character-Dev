using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using WizardsCode.Character.Stats;

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

        void Awake()
        {
            m_Influencer = GetComponent<StatsInfluencerTrigger>();
        }

        public bool Influences(StatSO stat) {
            if (m_Influencer == null) return false;

            return m_Influencer.Stat.name == stat.name;
        }
    }
}
