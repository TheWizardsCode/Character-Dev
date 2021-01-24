using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode
{
    /// <summary>
    ///  DebugInfo can be attached to any game object. On startup it will scan for any
    ///  components that implement the IDebug interface on the same object, not in parents 
    ///  or children. DebugInfo will then pull 
    ///  information from these components and present it to the user when running
    ///  in the editor and the GameObject is selected.
    /// </summary>
    public class DebugInfo : MonoBehaviour
    {
        IDebug[] components;

        void Start()
        {
            components = gameObject.transform.GetComponents<IDebug>();
        }

        private void OnDrawGizmosSelected()
        {
            if (components == null) return;

            string msg = gameObject.name;
            for (int i = 0; i < components.Length; i++)
            {
                string status = components[i].StatusText();
                if (!string.IsNullOrEmpty(status))
                {
                    msg += status;
                }
            }

            Vector3 pos = transform.position;
            pos.x += 2;
            pos.y += transform.lossyScale.y * 2;

            ExtendedGizmos.DrawString(msg, pos);
        }
    }
}
