using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WizardsCode.Character.Stats
{
    public class MemoryController : MonoBehaviour
    {
        [SerializeField, Tooltip("The set of currently retained short term memories.")]
        List<MemorySO> m_ShortTermMemories = new List<MemorySO>();
        [SerializeField, Tooltip("The set of currently retained long term memories. Long term memories are discard when space runs out.")]
        List<MemorySO> m_LongTermMemories = new List<MemorySO>();
        [SerializeField, Tooltip("The threshold at which short term memories or collections of memories are moved to long term memory.")]
        int m_LongTermMemoryThreshold = 100;

        int m_ShortTermMemorySize = 5;
        int m_LongTermMemorySize = 10;

        /// <summary>
        /// Get all short term memories, about a Game Object.
        /// </summary>
        /// <param name="go">The Game Object to retrieve memories about.</param>
        /// <returns></returns>
        public MemorySO[] RetrieveShortTermMemoriesAbout(GameObject go)
        {
            return m_ShortTermMemories.Where(m => GameObject.ReferenceEquals(go, m.about)).ToArray<MemorySO>();
        }

        /// <summary>
        /// Get all long term memories.
        /// </summary>
        /// <returns>All long term memories currently stored.</returns>
        public MemorySO[] RetrieveLongTermMemories()
        {
            return m_LongTermMemories.ToArray<MemorySO>();
        }

        /// <summary>
        /// Get all short term memories.
        /// </summary>
        /// <returns>All memories in the short term memory.</returns>
        public MemorySO[] RetrieveShortTermMemories()
        {
            return m_ShortTermMemories.ToArray<MemorySO>();
        }

        /// <summary>
        /// Get all long term memories, about a Game Object.
        /// </summary>
        /// <param name="go">The Game Object to retrieve memories about.</param>
        /// <returns></returns>
        public MemorySO[] RetrieveLongTermMemoriesAbout(GameObject go)
        {
            return m_LongTermMemories.Where(m => GameObject.ReferenceEquals(go, m.about)).ToArray<MemorySO>();
        }

        /// <summary>
        /// Add a memory to the short term memory. 
        /// If there is an existing short term memory that is similar to the new one then do not duplicate.
        /// If there is space left in the memory then is simply inserted.
        /// If there is no space left then discard the weakest memory.
        /// </summary>
        /// <param name="memory"></param>
        public void AddMemory(MemorySO memory)
        {
            if (HasSimilarShortTermMemory(memory))
            {
                return;
            }

            MoveFromShortToLongTermMemory();

            if (m_ShortTermMemories.Count == m_ShortTermMemorySize)
            {
                DiscardShortTermMemory();
            }

            m_ShortTermMemories.Add(memory);
        }

        /// <summary>
        /// Scan short term memory to see if there is an existing memory similar to this one.
        /// </summary>
        /// <param name="memory">The memory we are looking for similarities to</param>
        /// <returns>True if a similar memory is found, otherwise false.</returns>
        private bool HasSimilarShortTermMemory(MemorySO memory)
        {
            MemorySO[] memories = RetrieveShortTermMemoriesAbout(memory.about);
            if (memories.Length > 0)
            {
                for (int i = 0; i < memories.Length; i++)
                {
                    if (memories[i].traitName == memory.traitName)
                    {
                        if (Time.timeSinceLevelLoad < memories[i].m_Time + memories[i].cooldown)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Discard a sindle short term memory. Normally the weakest memory (the one with the least influence will be discarded).
        /// If there are multiple candidates a random one will be selected.
        /// </summary>
        private void DiscardShortTermMemory()
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Move any memory or collections of memories that have an influence over the long term memory threshold
        /// from short to long term memory. For a collection to be moved the `about` Game Object must be identical
        /// and the total influence must be over the threshold level.
        /// </summary>
        internal void MoveFromShortToLongTermMemory()
        {
            var grouped = from memory in m_ShortTermMemories
                          group memory by new
                          {
                              memory.about,
                              memory.traitName,
                              memory
                          } into grp
                          select new
                          {
                              about = grp.Key.about,
                              traitName = grp.Key.traitName,
                              totalInfluence = grp.Sum(i => i.influence)
                          };

            foreach (var item in grouped)
            {
                if (item.totalInfluence >= m_LongTermMemoryThreshold)
                {
                    MemorySO memory = ScriptableObject.CreateInstance<MemorySO>();
                    memory.about = item.about;
                    memory.traitName = item.traitName;
                    memory.influence = item.totalInfluence;
                    AddToLongTermMemory(memory);

                    ForgetShortTermMemories(item.about);
                }
            }
        }

        /// <summary>
        /// Forget all short term memories about a game object.
        /// </summary>
        /// <param name="about">The game object to forget about.</param>
        private void ForgetShortTermMemories(GameObject about)
        {
            m_ShortTermMemories.RemoveAll(item => GameObject.ReferenceEquals(item.about, about));
        }

        /// <summary>
        /// Add a memory to the long term memory if there is space. If there is not space then remove the
        /// weakest memory that is weaker than the memory being added, or discard the added memory.
        /// </summary>
        /// <param name="memory">The memory to try to add</param>
        /// <returns>True if the memory was committed to long term memory.</returns>
        private bool AddToLongTermMemory(MemorySO memory)
        {
            if (m_LongTermMemories.Count <= m_LongTermMemorySize)
            {
                m_LongTermMemories.Add(memory);
                return true;
            }
            else
            {
                throw new NotImplementedException("Make space in long term memory");
            }
        }

        /// <summary>
        /// Create a memory about an influencer object. See `AddMemory(MemorySO memory)`.
        /// </summary>
        /// <param name="influencer">The influencer that this memory should record.</param>
        internal void AddMemory(StatInfluencerSO influencer)
        {
            MemorySO memory = ScriptableObject.CreateInstance<MemorySO>();
            memory.about = influencer.generator;
            memory.traitName = "ShortTermTest";
            memory.influence = 5;
            AddMemory(memory);
        }
    }
       
}
