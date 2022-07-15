#if INK_PRESENT
using UnityEngine;

namespace WizardsCode.Ink
{
    /// <summary>
    /// An InkTrigger will instruct the InkManager to jump to a particular Knot when the player enters the trigger area.
    /// </summary>
    public class InkTrigger : MonoBehaviour
    {
        [SerializeField, Tooltip("The name of the knot to trigger. If left blank the story will be continued from the current position.")]
        string m_Knot;
        [SerializeField, Tooltip("The name of the stitch to trigger. If left blank the story will be continued from the current position.")]
        string m_Stitch;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (!string.IsNullOrEmpty(m_Knot))
                {
                    InkManager.Instance.JumpToPath(m_Knot, m_Stitch);
                }
                InkManager.Instance.SetPlayerControl(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                InkManager.Instance.SetPlayerControl(true);
            }
        }
    }
}
#endif