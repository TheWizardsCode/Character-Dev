using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character.Stats
{
    /// <summary>
    /// A memory records an event in the game world that impacted the characters personality traits.
    /// </summary>
    public class MemorySO : ScriptableObject
    {
        [SerializeField, Tooltip("The game object that this memory is about.")]
        GameObject m_About;
        [SerializeField, Tooltip("The name of the trait that was changed by the action this memory embodies.")]
        string m_AffectedTraitName;
        [SerializeField, Tooltip("Is this a good memory, that is one that the character would like to repeat if possible and appropriate.")]
        bool m_IsGood = true;
        [SerializeField, Tooltip("Whether this is a negative or positive memory on a range of -100 (terrifying) to 100 (nirvana)."), Range(-100, 100)]
        float m_Influence = 0;
        [SerializeField, Tooltip("The cooldown period before a character can be influenced by this object again, in seconds.")]
        float m_Cooldown = 5;

        // The time since level load that this memory was created. 
        // TODO: this is not saved between level loads, which probably means it is reset each time. This will cause bugs that allow memories to be formed to frequently between level loads. Need to use a game time and save this value.
        internal float m_Time;

        public string description
        {
            get
            {
                string msg = about.gameObject.name + " is ";
                msg += isGood ? "good because it " : "bad because it ";
                msg += influence > 0 ? " increased " : " decreased ";
                msg += statName;
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

        public string statName
        {
            get { return m_AffectedTraitName; }
            set { m_AffectedTraitName = value; }
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
