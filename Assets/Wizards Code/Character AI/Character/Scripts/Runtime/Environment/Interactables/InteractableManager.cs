using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.BackgroundAI;
using System;

namespace WizardsCode.Character.WorldState
{
    /// <summary>
    /// Tracks all Interactables in the world so that we can quickly and easily query world state.
    /// </summary>
    public class InteractableManager : AbstractSingleton<InteractableManager>
    {
        [HideInInspector, SerializeField]
        List<Interactable> m_InteractablesList = new List<Interactable>();

        Dictionary<InteractableTypeSO, List<Interactable>> m_InteractablesByType = new Dictionary<InteractableTypeSO, List<Interactable>>();

        /// <summary>
        /// Register an interactable in the world with the manager. Normally interactables will all this
        /// from their `Awake` method.
        /// </summary>
        /// <param name="interactable">The interactable to register.</param>
        internal void Register(Interactable interactable)
        {
            if (!m_InteractablesList.Contains(interactable))
            {
                m_InteractablesList.Add(interactable);

                List<Interactable> all;
                if (m_InteractablesByType.TryGetValue(interactable.Type, out all))
                {
                    all.Add(interactable);
                } else
                {
                    all = new List<Interactable>();
                    all.Add(interactable);
                    m_InteractablesByType.Add(interactable.Type, all);
                }
            }
        }

        /// <summary>
        /// Get the number of interactables in the world of a specific type.
        /// </summary>
        /// <param name="type">The type of the interactable we are looking for.</param>
        /// <returns>The number of interactables of this type in this world.</returns>
        internal int GetCount(InteractableTypeSO type)
        {
            List<Interactable> all;
            if (m_InteractablesByType.TryGetValue(type, out all))
            {
                return all.Count;
            } else
            {
                return 0;
            }
        }
    }
}
