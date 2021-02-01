using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Utility.UI
{
    /// <summary>
    /// Attach to an instrcutions panel so that it can be removed by pressing the ESC key.
    /// While the panel is visible the game will be paused.
    /// </summary>
    [ExecuteAlways]
    public class InstructionsPanel : MonoBehaviour
    {
        [SerializeField, Tooltip("If the documentation is to be loaded from a file in the Documentation folder enter its name here. The content of this file will be placed into the UI on startup.")]
        string documentationFilename = "";
        [SerializeField, Tooltip("If the documentation is to be loaded from a file this is the text component to load the content into.")]
        TMP_Text documentationTextGUI;

        void Awake()
        {
            Time.timeScale = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (!string.IsNullOrEmpty(documentationFilename) && documentationTextGUI)
            {
                //TODO don't hardcode the path to the documentation folder
                TextAsset content = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/WizardsCode/Character/Documentation/" + documentationFilename, typeof(TextAsset));
                if (content != null)
                {
                    documentationTextGUI.text = content.text;
                } else
                {
                    Debug.LogError("Unable to find documentation file '" + documentationFilename + "' configured in " + this);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }
}
