#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode
{
    public interface IDebug
    {
        /// <summary>
        /// Get the full status description for this component. This will
        /// typically be retrieved by the DebugInfo component and
        /// recorded or displayed based on the configuration of that
        /// component.
        /// </summary>
        /// <returns>A string describing the current status of this object.</returns>
        string StatusText();
    }
}
#endif
