#if NEOFPS2
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.NeoFPSExtenstions
{
    [RequireComponent(typeof(Collider))]
    public class KickBoxingDamage : MonoBehaviour
    {
        [SerializeField, Tooltip("The base amount of damage done by this melee damage weapon.")]
        float m_BaseDamage = 5;
        [SerializeField, Tooltip("The force to impart on the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler on the hit object.")]
        float m_ImpactForce = 15f;
        [SerializeField, Tooltip("Cooldown duration to prevent retriggering of damage in a single attack.")]
        float m_Cooldown = 1.5f;

        static List<IDamageHandler> s_DamageHandlers = new List<IDamageHandler>(4);
        float nextDamageTime = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (Time.timeSinceLevelLoad > nextDamageTime)
            {
                IDamageHandler damageHandler = other.GetComponent<IDamageHandler>();
                if (damageHandler != null)
                {
                    Vector3 position = other.ClosestPoint(transform.position);
                    damageHandler.AddDamage(m_BaseDamage);
                    ShakeHandler.Shake(position, 0.2f, 1f, 5, 0.2f);

                    nextDamageTime = Time.timeSinceLevelLoad + m_Cooldown;
                }
            }
        }
    }
}
#endif