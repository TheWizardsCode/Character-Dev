using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character.WorldState
{
    /// <summary>
    /// An Interactable Type describes an interactable from a high level perspective.
    /// </summary>
    [CreateAssetMenu(fileName = "new InteractableType", menuName = "Wizards Code/Interactables/New Interactable Type")]
    public class InteractableTypeSO : ScriptableObject
    {
        [SerializeField, Tooltip("A world unique name for this InteractableType.")]
        string m_DisplayName;
        [SerializeField, TextArea, Tooltip("A human readable description of this Interactable Type.")]
        string m_Description;

        public string DisplayName
        {
            get
            {
                return m_DisplayName;
            }
        }
    }
}
