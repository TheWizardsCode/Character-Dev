using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Text;

namespace WizardsCode.Character
{
    public class WorldStateUIController : MonoBehaviour
    {
        [SerializeField, Tooltip("The label for the world state sumamry.")]
        TextMeshProUGUI stateSummaryLabel;

        float m_FrequencyOfUpdates = 1;
        float m_TimeOfNextUpdate = 0;

        private void Update()
        {
            if (Time.realtimeSinceStartup > m_TimeOfNextUpdate)
            {
                m_TimeOfNextUpdate = Time.realtimeSinceStartup + m_FrequencyOfUpdates;

                StringBuilder stateSummary = new StringBuilder();
                stateSummary.Append("Spawned Items: ");
                stateSummary.Append(ActorManager.Instance.SpawnedItems.Count);

                stateSummaryLabel.text = stateSummary.ToString();
            }
        }
    }
}
