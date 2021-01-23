using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using WizardsCode;
using WizardsCode.Stats;
#endif

namespace WizardsCode.Character
{
    public class MemoryController : MonoBehaviour
#if UNITY_EDITOR
        , IDebug
#endif
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
        public MemorySO[] GetShortTermMemoriesAbout(GameObject go)
        {
            return m_ShortTermMemories.Where(m => GameObject.ReferenceEquals(go, m.about)).ToArray<MemorySO>();
        }

        /// <summary>
        /// Retrive all memories (long and short term) about influencers of a stat.
        /// </summary>
        /// <param name="name">The stat we are interested in.</param>
        /// <returns>A set of influencers known to affect the desired stat.</returns>
        public MemorySO[] GetMemoriesInfluencingStat(string name)
        {
            MemorySO[] shortTermMemories = m_ShortTermMemories.Where(m => m.stat.name == name).ToArray<MemorySO>();
            MemorySO[] longTermMemories = m_LongTermMemories.Where(m => m.stat.name == name).ToArray<MemorySO>();

            MemorySO[] all = new MemorySO[shortTermMemories.Length + longTermMemories.Length];
            shortTermMemories.CopyTo(all, 0);
            longTermMemories.CopyTo(all, shortTermMemories.Length);

            return all;
        }

        /// <summary>
        /// Get all long term memories.
        /// </summary>
        /// <returns>All long term memories currently stored.</returns>
        public MemorySO[] GetLongTermMemories()
        {
            return m_LongTermMemories.ToArray<MemorySO>();
        }

        /// <summary>
        /// Get all short term memories.
        /// </summary>
        /// <returns>All memories in the short term memory.</returns>
        public MemorySO[] GetShortTermMemories()
        {
            return m_ShortTermMemories.ToArray<MemorySO>();
        }

        /// <summary>
        /// Get all memories (long and short term), about a Game Object.
        /// </summary>
        /// <param name="go">The Game Object to retrieve memories about.</param>
        /// <returns></returns>
        public MemorySO[] GetAllMemoriesAbout(GameObject go)
        {
            MemorySO[] shortTerm = m_ShortTermMemories.Where(m => ReferenceEquals(go, m.about))
                                     .ToArray();
            MemorySO[] longTerm = m_LongTermMemories.Where(m => ReferenceEquals(go, m.about))
                                     .ToArray();

            MemorySO[] memories = new MemorySO[longTerm.Length + shortTerm.Length];
            longTerm.CopyTo(memories, 0);
            shortTerm.CopyTo(memories, longTerm.Length);

            return memories;
        }

        /// <summary>
        /// Get all long term memories, about a Game Object.
        /// </summary>
        /// <param name="go">The Game Object to retrieve memories about.</param>
        /// <returns></returns>
        public MemorySO[] GetLongTermMemoriesAbout(GameObject go)
        {
            return m_LongTermMemories.Where(m => GameObject.ReferenceEquals(go, m.about)).ToArray<MemorySO>();
        }

        /// <summary>
        /// Add a memory to the short term memory. 
        /// If there is an existing short term memory that is similar to the new one then do not duplicate;
        /// instead strengthen the existing memory.
        /// If there is space left in the memory then is simply inserted.
        /// If there is no space left then discard the weakest memory.
        /// </summary>
        /// <param name="memory">The memory to add</param>
        
        public void AddMemory(MemorySO memory)
        {
            MemorySO existingMemory = GetSimilarShortTermMemory(memory);
            if (existingMemory != null)
            {
                if (existingMemory.isGood != memory.isGood)
                {
                    if (Math.Abs(existingMemory.influence) < Math.Abs(memory.influence))
                    {
                        existingMemory.isGood = memory.isGood;
                    }
                }
                existingMemory.influence += memory.influence;
                existingMemory.time = memory.time;
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
        /// <returns>Return an existing short term memory that is similar, if one exists, or null.</returns>
        public MemorySO GetSimilarShortTermMemory(MemorySO memory)
        {
            MemorySO[] memories = GetShortTermMemoriesAbout(memory.about);
            if (memories.Length > 0)
            {
                for (int i = 0; i < memories.Length; i++)
                {
                    if (memories[i].stat == memory.stat)
                    {
                        return memories[i];
                    }
                }
            }
            return null;
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
                              memory.stat,
                              memory
                          } into grp
                          select new
                          {
                              about = grp.Key.about,
                              traitName = grp.Key.stat,
                              totalInfluence = grp.Sum(i => i.influence)
                          };

            foreach (var item in grouped)
            {
                if (item.totalInfluence >= m_LongTermMemoryThreshold)
                {
                    MemorySO memory = ScriptableObject.CreateInstance<MemorySO>();
                    memory.about = item.about;
                    memory.stat = item.traitName;
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
        /// <param name="isGood">Is this a good memory that represents an experience to be repeated?</param>
        internal void AddMemory(StatInfluencerSO influencer, bool isGood)
        {
            MemorySO memory = ScriptableObject.CreateInstance<MemorySO>();
            memory.about = influencer.generator;
            memory.stat = influencer.stat;
            memory.influence = influencer.maxChange;
            memory.cooldown = influencer.cooldown;
            memory.isGood = isGood;
            AddMemory(memory);
        }


#if UNITY_EDITOR
        string IDebug.StatusText()
        {
            string msg = "";
            if (m_ShortTermMemories.Count > 0)
            {
                msg += "\n\nShort Term Memories";
                for (int i = 0; i < m_ShortTermMemories.Count; i++)
                {
                    msg += "\n" + m_ShortTermMemories[i].description;
                }
            }

            if (m_LongTermMemories.Count > 0)
            {
                msg += "\n\nLong Term Memories";
                for (int i = 0; i < m_LongTermMemories.Count; i++)
                {
                    msg += m_LongTermMemories[i].description;
                }
            }

            return msg;
        }
#endif
    }

}
