using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// A memory records an event in the game world that impacted the characters personality traits.
    /// </summary>
    public class MemorySO : ScriptableObject
    {
        [SerializeField, Tooltip("The game object that this memory is about.")]
        GameObject m_About;
        [SerializeField, Tooltip("The stat that was changed by the action this memory embodies.")]
        StatSO m_AffectedStat;
        [SerializeField, Tooltip("Is this a good memory, that is one that the character would like to repeat if possible and appropriate.")]
        bool m_IsGood = true;
        [SerializeField, Tooltip("Whether this is a negative or positive memory on a range of -100 (terrifying) to 100 (nirvana)."), Range(-100, 100)]
        float m_Influence = 0;
        [SerializeField, Tooltip("The cooldown period before a character can be influenced by this object again, in seconds.")]
        float m_Cooldown = 5;

        //TODO: this is not saved between level loads, which probably means it is reset each time. This will cause bugs that allow memories to be formed to frequently between level loads. Need to use a game time and save this value.
        float m_Time;
        /// <summary>
        /// The time since level load that this memory was created.
        /// </summary>
        public float time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        public string description
        {
            get
            {
                string msg = about != null ? about.gameObject.name + " is " : "Influence from unknown source is ";
                msg += isGood ? "good because it " : "bad because it ";
                msg += influence > 0 ? "increased " : "decreased ";
                msg += stat.name;
                msg += " by " + Mathf.Abs(influence);
                return msg;
            }
        }

        private void Awake()
        {
            m_Time = Time.timeSinceLevelLoad;
        }

        public bool isGood
        {
            get { return m_IsGood; }
            set { m_IsGood = value; }
        }

        public GameObject about {
            get {return m_About;}
            set { m_About = value; }
        }

        public StatSO stat
        {
            get { return m_AffectedStat; }
            set { m_AffectedStat = value; }
        }

        public float influence
        {
            get { return m_Influence; }
            set { m_Influence = value; }
        }

        public float cooldown {
            get { return m_Cooldown; }
            set { m_Cooldown = value; } 
        }

        /// <summary>
        /// Test to see if this character is ready to return to this influencer. This is based on the
        /// time passed since the memory was last renewed.
        /// </summary>
        public bool readyToReturn
        {
            get { return Time.timeSinceLevelLoad > m_Time + cooldown; }
        }
    }
}
