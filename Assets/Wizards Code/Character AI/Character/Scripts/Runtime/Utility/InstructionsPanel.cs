using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.BackgroundAI.UI
{
    /// <summary>
    /// Attach to an instrcutions panel so that it can be removed by pressing the ESC key.
    /// While the panel is visible the game will be paused.
    /// </summary>
    [ExecuteAlways]
    public class InstructionsPanel : MonoBehaviour
    {
        [SerializeField, Tooltip("The text asset containing the content to be displayed as help.")]
        TextAsset documentation;

        [SerializeField, Tooltip("If the documentation is to be loaded from a file this is the text component to load the content into.")]
        TMP_Text documentationTextGUI;

        void Awake()
        {
            Time.timeScale = 0;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (documentation && documentationTextGUI)
            {
                documentationTextGUI.text = documentation.text;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }
}
