using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    public abstract class AbstractAIBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("The required states for this behaviour to be enabled.")]
        StateSO[] m_RequiredStates = new StateSO[0];

        StatsController controller;

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Called when the behaviour is started, from the `Start` method of the underlying
        /// `MonoBehaviour`.
        /// </summary>
        protected virtual void Init()
        {
            controller = GetComponent<StatsController>();

            if (controller == null)
            {
                if (m_RequiredStates.Length > 0)
                {
                    Debug.LogError(gameObject.name + " has required states defined but has no StatsController against which to check these states.");
                }
            }
        }

        public void Update()
        {
            for (int i = 0; i < m_RequiredStates.Length; i++)
            {
                if (!m_RequiredStates[i].IsSatisfiedFor(controller)) return;
            }

            OnUpdate();
        }

        /// <summary>
        /// Called whenever this behaviour needs to be updated. 
        /// </summary>
        protected abstract void OnUpdate();
    }
}
