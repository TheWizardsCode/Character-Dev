using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WizardsCode.Stats;

namespace WizardsCode.Character.Stats
{
    public class StatsUIController : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField, Tooltip("The character whos states we want to view")]
        StatsController character;

        [Header("UI Templates")]
        [SerializeField, Tooltip("The UI template to use to display a stat.")]
        RectTransform statePanelTemplate;

        private void Awake()
        {
            if (character == null)
            {
                Debug.LogWarning("StateUIController does not have a character configured. Either add a character in the inspector or remove the component. Destroying object.");
                Destroy(this.gameObject);
            }
        }

        Dictionary<StatSO, StatUIPanel> stateUIObjects = new Dictionary<StatSO, StatUIPanel>();
        void Update()
        {
            //TODO: don't update every frame
            StateSO[] states = character.desiredStates;
            for (int i = 0; i < states.Length; i++)
            {
                //TODO cache results rather than grabbing stat every cycle
                StatSO stat = character.GetOrCreateStat(states[i].statTemplate.name);

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
