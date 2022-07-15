using UnityEngine;
using System.Collections;
using WizardsCode.Stats;

namespace WizardsCode.Character.AI
{
    public class AttackBehaviour : GenericActorInteractionBehaviour
    {
        [Header("Melee Combat")]
        [SerializeField, Tooltip("The optimal range from which to attack.")]
        float m_OptimalAttackRange = 1.5f;
        [SerializeField, Tooltip("The time after the start if the `OnPerformCue` is first until the character attempts to inflict damage on the enemy.")]
        public float m_TimeUntilDamage = 1;
        [SerializeField, Tooltip("The set of character stats and the influence to apply to enemies this character manages to hit them.")]
        internal StatInfluence[] m_EnemyHitInfluences;

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
            Brain.Actor.TurnToFace(target.transform.position);

            yield return new WaitForSeconds(m_TimeUntilDamage);

            for (int i = 0; i < m_EnemyHitInfluences.Length; i++)
            {
                target.TryAddInfluence(m_EnemyHitInfluences[i], 0);
                Debug.Log($"'{Brain.DisplayName}' {DisplayName} '{target.DisplayName}' for {m_EnemyHitInfluences[i].maxChange} {m_EnemyHitInfluences[i].statTemplate.DisplayName}");
                yield return null;
            }
        }

        protected override void UpdateInteractionPosition(bool setOnNavMesh)
        {
            if (CurrentState == State.Starting && target == null)
            {
                int attempt = 0;
                target = participants[Random.Range(0, participants.Count)];
                while (target == Brain && attempt < 4)
                {
                    attempt++;
                    target = participants[Random.Range(0, participants.Count)];
                }
                if (target == Brain)
                {
                    return;
                }
            }

            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 offset = direction * m_OptimalAttackRange;
            m_InteractionPoint = target.transform.position - offset;
            m_InteractionGroupCenter = m_InteractionPoint;

            Brain.Actor.MoveTo(m_InteractionPoint);
        }
    }
}
