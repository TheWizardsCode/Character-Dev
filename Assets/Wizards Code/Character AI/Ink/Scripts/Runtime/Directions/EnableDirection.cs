using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Ink;

namespace WizardsCode.Ink
{
    /// <summary>
    /// Enable or Disable an object in the scene.
    /// 
    /// Use:
    /// Enable: OBJECT_NAME, TRUE_OR_FALSE
    /// </summary>
    public class EnableDirection : AbstractDirection
    {

        /// <summary>
        /// Enable or Disable an object
        ///
        /// </summary>
        /// <param name="parameters">OBJECT_NAME, TRUE_OR_FALSE/param>
        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 2))
            {
                return;
            }

            Transform transform = InkManager.Instance.FindTarget(parameters[0].Trim());
            if (transform)
            {
                if (parameters[1].Trim().ToLower() == "true")
                {
                    transform.gameObject.SetActive(true);
                }
                else if (parameters[1].Trim().ToLower() == "false")
                {
                    transform.gameObject.SetActive(false);
                }
                else
                {
                    LogError($"Recieved direction `Enable: {string.Join(", ", parameters)}`, however, only `true` or `false` values are allowed. `{parameters[1].Trim()}` was supplied. If this object is disabled on start you need to ensure that it is referenced in the `Cached Objects` section of the Ink Manager so that it can be discovered.", parameters);
                }
            }
            else
            {
                LogError($"Recieved direction `Enable: {string.Join(", ", parameters)}`, however, no object with the name `{parameters[0].Trim()}` was found.", parameters);
            }
        }
    }
}
