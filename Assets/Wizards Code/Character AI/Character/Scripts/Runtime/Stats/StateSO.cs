using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;
using UnityEngine.Serialization;

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
        //TODO need to ensure name is game unique
        string DisplayName = "No Name State";
        [SerializeField, Tooltip("The stat that manages this state.")]
        StatSO m_Stat;
        [SerializeField, Tooltip("State objective indicates whether our target value is a minimum, maxium or goal.")]
        Objective m_Objective;
        [SerializeField, Tooltip("The normalized target value of this stat."), Range(0f,1f)]
        [FormerlySerializedAs("m_NormalizeTargetValue")] // changed 4/21/22
        float m_NormalizedTargetValue;

        [Header("Effects")]
        [SerializeField, Tooltip("An influencer to apply whenever this state is not in the desired state. This can be used to do things like decrease health if a character is below a given energy threshold.")]
        List<StatInfluencerSO> m_NotInDesiredState = new List<StatInfluencerSO>();
        [SerializeField, Tooltip("An influencer to apply whenever this state is in the desired state. This can be used to do things like increase health if a character is above a given energy threshold.")]
        List<StatInfluencerSO> m_InDesiredState = new List<StatInfluencerSO>();

        [Header("Behaviours")]
        [SerializeField, Tooltip("Behaviours to add when this state is satisfied, and remove when unsatisfied.")]
        List<AbstractAIBehaviour> m_SatisfiedBehaviours = new List<AbstractAIBehaviour>();
        [SerializeField, Tooltip("Behaviours to add when this state is unsatisfied, and remove when satisfied.")]
        List<AbstractAIBehaviour> m_UnsatisfiedBehaviours = new List<AbstractAIBehaviour>();

        [Header("Sub States")]
        [SerializeField, Tooltip("A collection of states that must also be satisfied for this state to be satisfied.")]
        List<StateSO> m_SubStates = new List<StateSO>(); 

        public List<StatInfluencerSO> InfluencersToApplyWhenNotInDesiredState
        {
            get
            {
                return m_NotInDesiredState;
            }
        }
        public List<StatInfluencerSO> InfluencersToApplyWhenInDesiredState
        {
            get
            {
                return m_InDesiredState;
            }
        }

        /// <summary>
        /// A list of behaviours that should be added to a character when this state is 
        /// satisfied and removed when unsatisfied.
        /// </summary>
        public List<AbstractAIBehaviour> SatisfiedBehaviours
        {
            get { return m_SatisfiedBehaviours; }
        }

        /// <summary>
        /// A list of behaviours that should be added to a character when this state is 
        /// unsatisfied and removed when satisfied.
        /// </summary>
        public List<AbstractAIBehaviour> UnsatisfiedBehaviours
        {
            get
            {
                return m_UnsatisfiedBehaviours; 
            }
        }

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
            get { return m_NormalizedTargetValue; }
            set { m_NormalizedTargetValue = value; }
        }

        /// <summary>
        /// If all the conditions of this state are satisfied then this will return true.
        /// </summary>
        public bool IsSatisfiedFor (StatsTracker statsTracker) {
            if (statTemplate != null)
            {
                StatSO stat = statsTracker.GetOrCreateStat(statTemplate);

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
                if (!m_SubStates[i].IsSatisfiedFor(statsTracker))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
