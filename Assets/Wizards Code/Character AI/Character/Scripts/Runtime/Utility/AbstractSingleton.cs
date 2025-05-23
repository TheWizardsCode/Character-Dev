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
        bool m_IsPersistent = false;

        static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<T>();
                    if (m_Instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        m_Instance = singletonObject.AddComponent<T>();
                    }
                }
                return m_Instance;
            }
        }

        public virtual void OnEnable()
        {
            if (m_IsPersistent)
            {
                if (m_Instance == null)
                {
                    m_Instance = this as T;
                }
                else if (m_Instance != this)
                {
                    Destroy(gameObject);
                    return;
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
