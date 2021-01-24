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
        [SerializeField, Tooltip("The name of this state")]
        string name = "No Name State";
        [SerializeField, Tooltip("State objective indicates whether our target value is a minimum, maxium or goal.")]
        Objective m_Objective;
        [SerializeField, Tooltip("The target value of this stat.")]
        float m_TargetValue;
        [SerializeField, Tooltip("The stat that manages this state.")]
        StatSO m_Stat;

        public StatSO statTemplate
        {
            get { return m_Stat; }
            set { m_Stat = value; }
        }

        public Objective objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public float targetValue
        {
            get { return m_TargetValue; }
            set { m_TargetValue = value; }
        }
    }
}
