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

        [Header("UI Templates")]
        [SerializeField, Tooltip("The UI template to use to display a stat.")]
        RectTransform statePanelTemplate;

        private void Awake()
        {
            if (m_SelectedCharacter == null)
            {
                m_SelectedCharacter = GameObject.FindObjectOfType<Brain>();
            }

            if (m_SelectionManager == null)
            {
                m_SelectionManager = GameObject.FindObjectOfType<ClickToSelect>();
            }

            if (m_SelectionManager == null && m_SelectedCharacter == null)
            {
                Debug.LogWarning("The StatsUIController has neither a ClickToSelect selection manager or a pre-defined SelectedCharacter. Disabliing the StatsUIController.");
                this.enabled = false;
            }
        }

        Dictionary<StatSO, StatUIPanel> stateUIObjects = new Dictionary<StatSO, StatUIPanel>();
        void Update()
        {
            if (m_SelectionManager != null && m_SelectionManager.CurrentlySelected != null && (m_SelectedCharacter == null || !GameObject.ReferenceEquals(m_SelectedCharacter.gameObject, m_SelectionManager.CurrentlySelected)))
            {
                m_SelectedCharacter = m_SelectionManager.CurrentlySelected.GetComponentInChildren<Brain>();

                stateUIObjects.Clear();

                for (int i = transform.childCount - 1; i >= 0; --i)
                {
                    GameObject.Destroy(transform.GetChild(i).gameObject);
                }
                transform.DetachChildren();
            }

            //TODO: don't update every frame
            StateSO[] states = m_SelectedCharacter.DesiredStates;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].statTemplate == null) continue;

                //TODO cache results rather than grabbing stat every cycle
                StatSO stat = m_SelectedCharacter.GetOrCreateStat(states[i].statTemplate);

                StatUIPanel stateUI;
                if (!stateUIObjects.TryGetValue(stat, out stateUI)) {
                    stateUI = Instantiate(statePanelTemplate, transform).GetComponent<StatUIPanel>();
                    stateUI.stat = stat;
                    stateUI.gameObject.SetActive(true);

                    stateUIObjects.Add(stat, stateUI);
                }
            }
        }
    }
}
