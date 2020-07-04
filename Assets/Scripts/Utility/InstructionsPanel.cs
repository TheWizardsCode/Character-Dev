using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Utility.UI
{
    /// <summary>
    /// Attach to an instrcutions panel so that it can be removed by pressing the ESC key.
    /// While the panel is visible the game will be paused.
    /// </summary>
    public class InstructionsPanel : MonoBehaviour
    {
        void Awake()
        {
            Time.timeScale = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }
}
