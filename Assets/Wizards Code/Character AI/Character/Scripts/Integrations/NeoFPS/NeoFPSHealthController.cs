#if NEOFPS
using NeoFPS;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character.Stats;
using WizardsCode.Stats;
using static NeoFPS.BasicHealthManager;

namespace WizardsCode.Character.Intergration 
{
    /// <summary>
    /// Bridges between a Neo FPS Health Manager and a Wizards Code Character Stats Tracker.
    /// 
    /// Place this on an AI instead of a NeoFPS Health Manager so that the it takes damage from the Neo FPS damage system.
    /// </summary>
    public class NeoFPSHealthController
        : HealthController, IHealthManager, INeoSerializableComponent
    {
        [Tooltip("An event called whenever the health changes")]
        public event HealthDelegates.OnHealthChanged onHealthChanged;
        [Tooltip("An event called whenever the alive status of the character changes.")]
        public event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        [Tooltip("An event called whenever the Max Health of the character changes.")]
        public event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

        /// <summary>
        /// NeoFPS paramater, pass through to WizardsCode Health Controller
        /// </summary>
        public bool isAlive => IsAlive;

        /// <summary>
        /// NeoFPS paramater, pass through to WizardsCode Health Controller
        /// </summary>
        public float health
        {
            get { return Health; }
            set { Health = value; }
        }

        /// <summary>
        /// NeoFPS paramater, pass through to WizardsCode Health Controller
        /// </summary>
        public float healthMax
        {
            get { return m_Health.MaxValue; }
            set { m_Health.MaxValue = value; }
        }

        /// <summary>
        /// NeoFPS paramater, pass through to WizardsCode Health Controller
        /// </summary>
        public float normalisedHealth
        {
            get { return m_Health.NormalizedValue; }
            set { m_Health.NormalizedValue = value; }
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage)
        {
            TakeDamage(damage);
        }


        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method.
        /// TODO: Handle criticals
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage, bool critical)
        {
            TakeDamage(damage);
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method.
        /// TODO: Handle criticals, damage source
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage, IDamageSource source)
        {
            TakeDamage(damage);
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method.
        /// TODO: Handle criticals, and raycastHit
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage, bool critical, RaycastHit hit)
        {
            TakeDamage(damage);
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method.
        /// TODO: Handle criticals, damage source
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage, bool critical, IDamageSource source)
        {
            TakeDamage(damage);
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method.
        /// TODO: Handle criticals, damage source and raycastHit
        /// </summary>
        /// <param name="damage"></param>
        public void AddDamage(float damage, bool critical, IDamageSource source, RaycastHit hit)
        {
            TakeDamage(damage);
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method
        /// </summary>
        /// <param name="damage"></param>
        public void AddHealth(float h)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method
        /// </summary>
        /// <param name="damage"></param>
        public void AddHealth(float h, IDamageSource source)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method
        /// </summary>
        /// <param name="damage"></param>
        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// NeoFPS method, pass through to appropriate WizardsCode method
        /// </summary>
        /// <param name="damage"></param>
        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnHealthChanged(float oldValue)
        {
            if (onHealthChanged != null)
                onHealthChanged(oldValue, Health, false, null);
            base.OnHealthChanged(oldValue);

            if (!IsAlive)
            {
                if (onIsAliveChanged != null)
                {
                    onIsAliveChanged.Invoke(IsAlive);
                }
            }
        }
    }
}
#endif