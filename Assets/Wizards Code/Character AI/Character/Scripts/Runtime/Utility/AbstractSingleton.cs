using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.BackgroundAI
{
    /// <summary>
    /// To create an manage a singleton simply implement this abstract calss.
    /// 
    /// Inspired by http://wiki.unity3d.com/index.php/Singleton
    /// </summary>
    public abstract class AbstractSingleton<T> : MonoBehaviour where T : AbstractSingleton<T>
    {
        [SerializeField, Tooltip("If set to true this instance will persist between levels.")]
        bool m_IsPersistant = false;

        static T m_Instance;

        public static T Instance { get { return m_Instance; } }

        public virtual void OnEnable()
        {
            if (m_IsPersistant)
            {
                if (!m_Instance)
                {
                    m_Instance = this as T;
                }
                else
                {
                    Destroy(gameObject);
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                m_Instance = this as T;
            }
        }
    }

}
