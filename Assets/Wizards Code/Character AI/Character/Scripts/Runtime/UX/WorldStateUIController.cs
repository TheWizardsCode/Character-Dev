using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Text;
using System.Linq;
using WizardsCode.Character.WorldState;

namespace WizardsCode.Character
{
    public class WorldStateUIController : MonoBehaviour
    {
        [SerializeField, Tooltip("The label for the world state sumamry.")]
        TextMeshProUGUI stateSummaryLabel;

        [SerializeField, Tooltip("The types of interactable to report in the status line.")]
        InteractableTypeSO[] typesToTrack;

        float m_FrequencyOfUpdates = 1;
        float m_TimeOfNextUpdate = 0;

        private void Update()
        {
            if (Time.realtimeSinceStartup > m_TimeOfNextUpdate)
            {
                m_TimeOfNextUpdate = Time.realtimeSinceStartup + m_FrequencyOfUpdates;

                StringBuilder stateSummary = new StringBuilder();
                stateSummary.Append("Active Brains: ");
                stateSummary.Append(ActorManager.Instance.ActiveBrains.Count);
                for (int i = 0; i < ActorManager.Instance.ActiveBehaviours.Count; i++) {
                    stateSummary.Append(" ");
                    stateSummary.Append(ActorManager.Instance.ActiveBehaviours.ElementAt(i).Key);
                    stateSummary.Append(": ");
                    stateSummary.Append(ActorManager.Instance.ActiveBehaviours.ElementAt(i).Value);
                }

                stateSummary.AppendLine();

                for (int i = 0; i < typesToTrack.Length; i++) {
                    stateSummary.Append(typesToTrack[i].DisplayName);
                    stateSummary.Append(": ");
                    stateSummary.Append(InteractableManager.Instance.GetCount(typesToTrack[i]));
                }

                stateSummaryLabel.text = stateSummary.ToString();
            }
        }
    }
}
