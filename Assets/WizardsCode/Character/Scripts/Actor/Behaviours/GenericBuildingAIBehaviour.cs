using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;

namespace WizardsCode.Character
{
    /// <summary>
    /// This generic AI building behaviour allows an actor to build something
    /// in the environment. It extends the AbstractAIBehaviour and thus provides
    /// all the same configuration options. But adds information about how to
    /// manage the build process.
    /// </summary>
    public class GenericBuildingAIBehaviour : AbstractAIBehaviour
    {
        [Header("Built Object")]
        [SerializeField, Tooltip("The prefab to spawn when the build is complete.")]
        GameObject m_BuiltPrefab;

        internal override void FinishBehaviour()
        {
            base.FinishBehaviour();

            GameObject go = Instantiate(m_BuiltPrefab, transform.position, Quaternion.identity);
        }
    }
}
