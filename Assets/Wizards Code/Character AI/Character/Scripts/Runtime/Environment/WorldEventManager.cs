using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character.WorldState;
using WizardsCode.Stats;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Checks for and manages world events that occur as a result of the current world state.
    /// For example, actors may become pregnant, this is a world event.
    /// </summary>
    public class WorldEventManager : MonoBehaviour
    {
        //TODO this should be a generic stat mapped to a generic event
        public StatSO m_PregnancyStatTemplate;
        public StateSO m_PregnantState;
        public float m_PregnancyEventCheckFrequency = 5;
        public bool m_RepeatingChanceOfPregnancy = false;

        private float m_TimeOfNextPregnancyCheck = float.MinValue;

        public void Update()
        {
            //TODO Don't check all possible events in a single update, need to spread out over time.

            //TODO Generalize this to manage an arbitrary set of possible world events
            if (m_TimeOfNextPregnancyCheck < Time.timeSinceLevelLoad)
            {
                List<Brain> brains = ActorManager.Instance.GetAllActorsWith(m_PregnancyStatTemplate);
                for (int i = 0; i < brains.Count; i++)
                {
                    if (!brains[i].SatisfiesState(m_PregnantState))
                    {
                        if (Random.value <= brains[i].GetStat(m_PregnancyStatTemplate).NormalizedValue)
                        {
                            brains[i].GetStat(m_PregnancyStatTemplate).NormalizedValue = 1;
                        }
                    }
                }
                m_TimeOfNextPregnancyCheck = Time.timeSinceLevelLoad + m_PregnancyEventCheckFrequency;
            }
        }
    }
}
