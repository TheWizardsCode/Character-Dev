using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// State is a condition that captures the desired state of a 
    /// stat. This can be used, for example, in AI where the AI may
    /// choose an action based on the Desired State and the actual state
    /// of a stat.
    /// </summary>
    [CreateAssetMenu(fileName = "New State", menuName = "Wizards Code/Stats/New State")]
    public class StateSO : ScriptableObject
    {
        public enum Goal { Decrease, NoAction, Increase }
        public enum Objective { LessThan, Approximately, GreaterThan }

        [Header("Stat Requirements")]
        [SerializeField, Tooltip("The name of this state")]
        //TODO need to change name to ID and ensure it is game unique
        string name = "No Name State";
        [SerializeField, Tooltip("The stat that manages this state.")]
        StatSO m_Stat;
        [SerializeField, Tooltip("State objective indicates whether our target value is a minimum, maxium or goal.")]
        Objective m_Objective;
        [SerializeField, Tooltip("The normalized target value of this stat."), Range(0f,1f)]
        float m_NormalizeTargetValue;

        [Header("Sub States")]
        [SerializeField, Tooltip("A collection of states that must also be satisfied for this state to be satisfied.")]
        List<StateSO> m_SubStates = new List<StateSO>(); 

        public StatSO statTemplate
        {
            get { return m_Stat; }
            set { m_Stat = value; }
        }

        public StateSO[] SubStates
        {
            get { return m_SubStates.ToArray(); }
        }

        public Objective objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public float normalizedTargetValue
        {
            get { return m_NormalizeTargetValue; }
            set { m_NormalizeTargetValue = value; }
        }

        /// <summary>
        /// If all the conditions of this state are satisfied then this will return true.
        /// </summary>
        public bool IsSatisfiedFor (Brain controller) {
            if (statTemplate != null)
            {
                StatSO stat = controller.GetOrCreateStat(statTemplate);

                switch (objective)
                {
                    case Objective.LessThan:
                        if (stat.NormalizedValue >= normalizedTargetValue)
                        {
                            return false;
                        }
                        break;
                    case Objective.Approximately:
                        return Mathf.Approximately(stat.NormalizedValue, normalizedTargetValue);
                    case Objective.GreaterThan:
                        if (stat.NormalizedValue <= normalizedTargetValue)
                        {
                            return false;
                        }
                        break;
                }
            }

            // Check substates
            for (int i = 0; i < m_SubStates.Count; i++)
            {
                if (!m_SubStates[i].IsSatisfiedFor(controller))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
