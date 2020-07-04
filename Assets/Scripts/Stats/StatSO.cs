using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// A StatSO tracks the base value and current value of a Stat. This can be used by an AI
    /// system to decide on actions to take.
    /// </summary>
    public class StatSO : ScriptableObject
    {
        [Header("Details")]
        [SerializeField, Tooltip("The human readable name for this stat.")]
        string displayName = "No Name Stat";
        [SerializeField, Tooltip("The base value for this stat. This is the value that the character will always trend towards with no external factors influencing the current value."), Range(-100, 100)]
        float m_BaseValue = 0;

        [HideInInspector]
        /// The desiredState for this stat. This indicates the value the character should seek to achieve with this state.
        public DesiredState desiredState;
        [HideInInspector, SerializeField]
        float m_CurrentValue;

        /// <summary>
        /// Get a human readable description of the current status of this stat.
        /// That is, it's value, whether it is wihtin the desired range etc.
        /// </summary>
        public string statusDescription
        {
            get {
                string msg = name + " is " + value;
                switch (goal) {
                    case DesiredState.Goal.Decrease:
                        msg += " which is too high";
                        break;
                    case DesiredState.Goal.NoAction:
                        msg += " which is abou right.";
                        break;
                    case DesiredState.Goal.Increase:
                        msg += " which is too low";
                        break;
                }
                return msg; 
            }
        }

        /// <summary>
        /// Set the current value of this stat. If an attempt is made to set the value 
        /// outside the allowable range (-100 to 100) then the value will
        /// be adjusted to fit this range.
        /// </summary>
        public float value {
            get { return m_CurrentValue; }
            internal set
            {
                m_CurrentValue = Mathf.Clamp(value, -100, 100);
            }
        }

        private void Awake()
        {
            m_CurrentValue = m_BaseValue;
        }

        /// <summary>
        /// Called by the StatsController to update the stat based on current conditions.
        /// </summary>
        internal virtual void OnUpdate()
        {
            //Debug.Log("Stat \"" + name + "\" has value of " + value);
        }

        /// <summary>
        /// Get the current goal relating to this stat. That is do we currently want to 
        /// increase, decrease or maintaint his stat.
        /// </summary>
        /// <returns>The current goal for this stat.</returns>
        public DesiredState.Goal goal
        {
            get
            {
                switch (desiredState.objective)
                {
                    case DesiredState.Objective.LessThan:
                        if (value > desiredState.targetValue)
                        {
                            return DesiredState.Goal.Decrease;
                        }
                        break;

                    case DesiredState.Objective.Approximately:
                        if (value > desiredState.targetValue * 1.1)
                        {
                            return DesiredState.Goal.Decrease;
                        }
                        else
                        {
                            if (value < desiredState.targetValue * 0.9)
                            {
                                return DesiredState.Goal.Increase;
                            }
                        }
                        break;

                    case DesiredState.Objective.GreaterThan:
                        if (value < desiredState.targetValue)
                        {
                            return DesiredState.Goal.Increase;
                        }
                        break;
                }
                return DesiredState.Goal.NoAction;
            }
        }

        /// <summary>
        /// Describe the goal in human readable form.
        /// </summary>
        public string describeGoal { 
            get
            {
                if (goal == DesiredState.Goal.NoAction)
                {
                    return "No current goal for " + name;
                } else
                {
                    return "Goal: " + goal + " " + name + " from " + Mathf.RoundToInt(value) + " to " + desiredState.targetValue;
                }
            }
        }
    }

    /// <summary>
    /// DesiredState is a struct that captures the desired state of a 
    /// stat. This can be used, for example, in AI where the AI may
    /// choose an action based on the Desired State and the actual state
    /// of a stat.
    /// </summary>
    [Serializable]
    public struct DesiredState
    {
        public enum Goal { Decrease, NoAction, Increase }
        public enum Objective { LessThan, Approximately, GreaterThan }
        [Tooltip("The name of the stat we are defining a desired state for.")]
        public string statName;
        [Tooltip("State objective indicates whether our target value is a minimum, maxium or goal.")]
        public Objective objective;
        [Tooltip("The target value of this stat.")]
        public float targetValue;

    }
}
