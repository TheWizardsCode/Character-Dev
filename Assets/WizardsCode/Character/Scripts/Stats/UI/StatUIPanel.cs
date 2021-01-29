using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using WizardsCode.Stats;
using System;
using System.Globalization;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// Connects a stat to a UI panel displaying info about that stat.
    /// </summary>
    public class StatUIPanel : MonoBehaviour
    {
        [SerializeField, Tooltip("The state description label")]
        TextMeshProUGUI stateLabel;
        [SerializeField, Tooltip("The label for displaying the name and current value of this stat.")]
        TextMeshProUGUI statLabel;
        [SerializeField, Tooltip("The slider for setting the value of this stat.")]
        Slider statSlider;

        /// <summary>
        /// The stat that this UI panel represents.
        /// </summary>
        public StatSO stat { get; set; }

        private void OnEnable()
        {
            statSlider.value = stat.normalizedValue;
            statSlider.onValueChanged.AddListener(delegate { SetValue(statSlider.value); });
        }

        private void OnDisable()
        {
            statSlider.onValueChanged.RemoveListener(delegate { SetValue(statSlider.value); });
        }

        private void SetValue(float value)
        {
            stat.normalizedValue = value;
        }

        private void Update()
        {
            if (stat == null) return;

            statLabel.text = stat.name + "\n" + stat.normalizedValue.ToString("P0", CultureInfo.InvariantCulture);
        }
    }
}
