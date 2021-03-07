using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Ink
{
    /// <summary>
    /// An InkTrigger will instruct the InkManager to jump to a particular Knot when the player enters the trigger area.
    /// </summary>
    public class InkTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                InkManager.Instance.IsDisplayingUI = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                InkManager.Instance.IsDisplayingUI = false;
            }
        }
    }
}
