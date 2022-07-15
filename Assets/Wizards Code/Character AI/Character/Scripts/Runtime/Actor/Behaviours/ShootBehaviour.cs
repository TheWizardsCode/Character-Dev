using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character.AI;
using WizardsCode.Stats;

namespace WizardsCode.Character.AI
{
    public class ShootBehaviour : GenericActorInteractionBehaviour
    {
        [Header("Shooter Combat")]
        [SerializeField, Tooltip("The optimal range from which to attack.")]
        float m_OptimalAttackRange = 30f;
        [SerializeField, Tooltip("The time after the start if the `OnPerformCue` is first called until the character attempts to inflict damage on the enemy.")]
        public float m_TimeUntilDamage = 1;
        [SerializeField, Tooltip("The set of character stats and the influence to apply to enemies this character manages to hit.")]
        internal StatInfluencerSO[] m_EnemyHitInfluences;

        StatsTracker target;

        /// <summary>
        /// Start a coroutine that will perform the damage check at the appropriate time.
        /// </summary>
        protected override void OnPrepareCue()
        {
            StartCoroutine(DealDamageCo());
        }

        private IEnumerator DealDamageCo()
        {
            yield return new WaitForSeconds(m_TimeUntilDamage);

            for (int i = 0; i < m_EnemyHitInfluences.Length; i++)
            {
                target.TryAddInfluencer(m_EnemyHitInfluences[i]);
                Debug.Log($"{DisplayName} hit {target.DisplayName} for {m_EnemyHitInfluences[i].maxChange} {m_EnemyHitInfluences[i].stat.DisplayName}");
                yield return null;
            }
        }

        protected override void UpdateInteractionPosition(bool requireNavMesh)
        {
            if (target == null) return;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 offset = direction * m_OptimalAttackRange;
            m_InteractionPoint = target.transform.position - offset;
            m_InteractionGroupCenter = m_InteractionPoint;

            Brain.Actor.MoveTo(m_InteractionPoint);
        }
    }
}
