using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character.WorldState
{
    /// <summary>
    /// Captures a world state in terms of a count of a particular kind of interactable relative to the number of brains in the world.
    /// </summary>
    [CreateAssetMenu(fileName = "New World State", menuName = "Wizards Code/Stats/New World State")]
    public class WorldStateSO : ScriptableObject
    {
        [SerializeField, Tooltip("The display name for the UI")]
        string m_DisplayName;
        [SerializeField, Tooltip("The type of Interactable we are checking world state against.")]
        InteractableTypeSO m_Type;
        [SerializeField, Tooltip("Maximum number required per active brain.")]
        float m_MaxNumberRequiredPerBrain = 2;

        public string DisplayName
        {
            get { return m_DisplayName; }
        }

        public bool IsValid
        {
            get
            {
                float countPerBrain = InteractableManager.Instance.GetCount(m_Type) / (float)ActorManager.Instance.ActiveBrainsCount;
                return countPerBrain < m_MaxNumberRequiredPerBrain;
            }
        }
    }
}
