using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using WizardsCode.Stats;

namespace WizardsCode.Character.Stats
{
    public class StatsUIController : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField, Tooltip("The character whos states we want to view")]
        [FormerlySerializedAs("character")]
        Brain m_SelectedCharacter;
        [SerializeField, Tooltip("The selection manager in the scene.")]
        ClickToSelect m_SelectionManager;

        [Header("UI Elements")]
        [SerializeField, Tooltip("The text element to display the desription of the actors current behaviour.")]
        TextMeshProUGUI m_BehaviourLabel;
        [SerializeField, Tooltip("The UI template to use to display a stat.")]
        [FormerlySerializedAs("statePanelTemplate")]
        RectTransform statPanelTemplate;


        private void Awake()
        {
            if (m_SelectionManager == null)
            {
                Debug.LogWarning($"You have not configured the selection manager in {this}. Using `FindObjectOfType` to try to discover it. You should manually set this up in the inspector.");
                m_SelectionManager = GameObject.FindObjectOfType<ClickToSelect>();
            }

            if (m_SelectionManager == null && m_SelectedCharacter == null)
            {
                Debug.LogWarning("The StatsUIController has neither a ClickToSelect selection manager or a pre-defined SelectedCharacter. Disabliing the StatsUIController.");
                this.enabled = false;
            }
        }

        Dictionary<string, StatUIPanel> stateUIObjects = new Dictionary<string, StatUIPanel>();
        void Update()
        {
            if (m_SelectionManager != null 
                && m_SelectionManager.CurrentlySelected != null 
                && (m_SelectedCharacter == null 
                    || !GameObject.ReferenceEquals(m_SelectedCharacter, m_SelectionManager.CurrentlySelected)))
            {
                m_SelectedCharacter = m_SelectionManager.CurrentlySelected;
                ClearStatesUI();

                StateSO[] states = m_SelectedCharacter.DesiredStates;
                if (states.Length != transform.childCount)
                {
                    for (int i = 0; i < states.Length; i++)
                    {
                        if (states[i].statTemplate == null) continue;

                        StatSO stat = m_SelectedCharacter.GetOrCreateStat(states[i].statTemplate);

                        StatUIPanel stateUI;
                        if (!stateUIObjects.TryGetValue(stat.DisplayName, out stateUI))
                        {
                            stateUI = Instantiate(statPanelTemplate, transform).GetComponent<StatUIPanel>();
                            stateUI.stat = stat;
                            stateUI.gameObject.SetActive(true);

                            stateUIObjects.Add(stat.DisplayName, stateUI);
                        }
                    }
                }
            }

            //OPTIMIAZTION: don't update every frame
            if (m_SelectedCharacter != null)
            {
                if (m_BehaviourLabel != null)
                {
                    if (m_SelectedCharacter.ActiveBlockingBehaviour != null)
                    {
                        string duration = Mathf.Clamp(m_SelectedCharacter.ActiveBlockingBehaviour.EndTime - Time.timeSinceLevelLoad, 0, float.MaxValue).ToString("0.0");
                        m_BehaviourLabel.text = $"{m_SelectedCharacter.DisplayName} - {m_SelectedCharacter.ActiveBlockingBehaviour.DisplayName} - Status is {m_SelectedCharacter.ActiveBlockingBehaviour.CurrentState}. Finishes in {duration}";
                    } else
                    {
                        m_BehaviourLabel.text = $"{m_SelectedCharacter.DisplayName} - Idle.";
                    }
                }
            }
        }

        private void ClearStatesUI()
        {
            stateUIObjects.Clear();
            for (int i = transform.childCount - 1; i >= 0; --i)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
            transform.DetachChildren();
        }
    }
}
