using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using WizardsCode.Utility;

namespace WizardsCode.Character
{
    /// <summary>
    /// This generic AI building behaviour allows an actor to build something
    /// in the environment. It extends the AbstractAIBehaviour and thus provides
    /// all the same configuration options. But adds information about how to
    /// manage the build process.
    /// 
    /// Items that an actor build will go into their memory.
    /// </summary>
    public class GenericBuildingAIBehaviour : AbstractAIBehaviour
    {
        [Header("Built Object")]
        [SerializeField, Tooltip("The prefab to spawn when the build is complete.")]
        SpawnerDefinition m_BuiltPrefab;

        internal override void FinishBehaviour()
        {
            base.FinishBehaviour();

            m_BuiltPrefab.InstantiatePrefabs(transform.position);
            MemorySO memory = ScriptableObject.CreateInstance<MemorySO>();
            memory.about = this.gameObject;
            memory.interactionName = DisplayName;
            memory.isGood = true;

            Brain.Memory.AddMemory(memory);
        }
    }
}
