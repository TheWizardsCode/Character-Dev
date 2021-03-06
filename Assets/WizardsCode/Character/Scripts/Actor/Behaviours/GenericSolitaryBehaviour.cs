using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character
{
    /// <summary>
    /// A Generic Solitary Behaviour is one that does not need another actor
    /// or interactable to be performed. It also does not produce anything
    /// outside of changes to the actors stats.
    /// </summary>
    public class GenericSolitaryBehaviour : AbstractAIBehaviour
    {
        [Header("Solitary Action Controls")]
        [SerializeField, Tooltip("The time, in seconds, between possible executions of this behaviour.")]
        float m_CooldownDuration = 60;

        float m_EndCooldownTime = float.MaxValue;

        public override bool IsAvailable
        {
            get {
                if (m_EndCooldownTime < Time.timeSinceLevelLoad)
                {
                    return false;
                }
                return base.IsAvailable; 
            }
        }

        internal override void StartBehaviour(float duration)
        {
            m_EndCooldownTime = Time.timeSinceLevelLoad + m_CooldownDuration;
            base.StartBehaviour(duration);
        }
    }
}
