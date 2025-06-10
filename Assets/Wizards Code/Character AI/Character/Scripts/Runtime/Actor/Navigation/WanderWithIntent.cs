using UnityEngine;
using WizardsCode.Stats;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Diagnostics.CodeAnalysis;

namespace WizardsCode.Character
{
    /// <summary>
    /// Make the character wander semi-randomly semi-Purposefully. 
    /// They will wander until semi-randomly, meaning they will walk in roughly the same
    /// direction until either the wander time is complete or until a monitored
    /// stat falls below a given level.
    /// 
    /// When wandering they will actively avoid areas for which they have
    /// a bad memory.
    /// 
    /// If any stats are not within their desired range the character
    /// will try to find an influencer that will address that stat in a positive way,
    /// starting with any that they can remember that are in the wander range.
    /// </summary>
    public class WanderWithIntent : Wander
    {
        [Header("Cooldown Settings")]
        [SerializeField, Tooltip("Time in seconds before the character can change direction again after failing to find a valid path. Helps prevent oscillation between directions.")]
        private float m_DirectionChangeCooldown = 1f;
        
        [SerializeField, Tooltip("Time in seconds before the character can re-enter turning mode after exiting it. Prevents rapid back-and-forth turning.")]
        private float m_TurningModeCooldown = 1f;
        
        [SerializeField, Tooltip("Time in seconds before the character can switch to a different memory target. Prevents rapid target switching.")]
        private float m_MemoryTargetCooldown = 1f;

        [SerializeField, Tooltip("Time in seconds between stat evaluations. Reduces computational overhead and prevents jittery behavior.")]
        private float m_StatEvaluationCooldown = 1f;

        Brain statsController;
        MemorySO nearestMemoryOfInterest;
        StatSO focusedStat;
        
        // Cooldown tracking
        private float m_LastDirectionChangeTime = -999f;
        private float m_LastTurningModeTime = -999f;
        private float m_LastMemoryTargetChangeTime = -999f;
        private float m_LastStatEvaluationTime = -999f;
        private Vector3 m_LastFailedDirection = Vector3.zero;
        private bool m_WasInTurningMode = false;
        private MemorySO m_LastTargetedMemory = null;

        protected override void Init()
        {
            base.Init();
            statsController = GetComponentInParent<Brain>();
        }

        /// <summary>
        /// Checks to see if any stats are not in their desired states. If any are not then
        /// look in memories to see if the character knows of a place to address that. If
        /// a place is recalled and they are ready to return then they move towards it. 
        /// Otherwise wander.
        /// </summary>
        protected override void UpdateMove()
        {
            // Check cooldown for stat evaluation to reduce computational overhead
            if (!CanEvaluateStats())
            {
                // If we can't evaluate stats, check if we should continue with current behavior
                if (nearestMemoryOfInterest != null && !CanChangeMemoryTarget())
                {
                    // Continue moving to current memory target
                    Collider col = nearestMemoryOfInterest.about.GetComponent<Collider>();
                    SetCurrentTargetWithinTriggerOf(col);
                    return;
                }
                
                // Fall back to base wander behavior with direction change cooldown
                base.UpdateMove();
                return;
            }

            // Update stat evaluation timestamp
            UpdateStatEvaluationTime();

            // Get stats from unsatisfied desired states
            List<StatSO> statsList = new List<StatSO>();
            for (int i = 0; i < statsController.UnsatisfiedDesiredStates.Count; i++)
            {
                if (statsController.UnsatisfiedDesiredStates[i].statTemplate != null)
                {
                    StatSO stat = statsController.GetOrCreateStat(statsController.UnsatisfiedDesiredStates[i].statTemplate);
                    statsList.Add(stat);
                }
            }
            StatSO[] stats = statsList.ToArray();

            nearestMemoryOfInterest = null;
            focusedStat = null;
            float sqrMagnitude = float.PositiveInfinity;
            
            for (int i = 0; i < stats.Length; i++) {
                //Debug.Log(gameObject.name + " desires to " + stats[i].goal + " " + stats[i].name + " to " + stats[i].desiredState.targetValue);
                //Debug.Log(stats[i].name + " is currently " + stats[i].value);

                // Find the nearest place that is in current memory that helps achieve one of the characters goals
                MemorySO[] memories = Memory.GetMemoriesInfluencingStat(stats[i]);
                for (int y = 0; y < memories.Length; y++)
                {
                    if (memories[y].readyToReturn)
                    {
                        if (statsController.GetGoalFor(stats[i]) == StateSO.Goal.Increase && memories[y].influence < 0)
                        {
                            continue;
                        }
                        else if (statsController.GetGoalFor(stats[i]) == StateSO.Goal.Decrease && memories[y].influence > 0)
                        {
                            continue;
                        }

                        Collider col = memories[y].about.GetComponent<Collider>();
                        if (!col.bounds.Contains(transform.position))
                        {
                            float distance = Vector3.SqrMagnitude(memories[y].about.transform.position - gameObject.transform.position);
                            if (distance < sqrMagnitude)
                            {
                                // Check if we can change memory target (cooldown)
                                if (m_LastTargetedMemory != memories[y] && !CanChangeMemoryTarget())
                                {
                                    continue; // Skip this memory due to cooldown
                                }

                                focusedStat = stats[i];
                                nearestMemoryOfInterest = memories[y];
                                sqrMagnitude = distance;
                            }
                        }
                    }
                }

                // Head to the remembered place
                if (nearestMemoryOfInterest != null)
                {
                    //Debug.Log("It looks like " + nearestMemoryOfInterest.about + " is the best place to achieve that goal");

                    // Update memory target change time if we're targeting a new memory
                    if (m_LastTargetedMemory != nearestMemoryOfInterest)
                    {
                        UpdateMemoryTargetChangeTime();
                        m_LastTargetedMemory = nearestMemoryOfInterest;
                    }

                    GameObject target = nearestMemoryOfInterest.about;
                    Collider col = target.GetComponent<Collider>();
                    SetCurrentTargetWithinTriggerOf(col);
                    return;
                }
            }

            // if no place in memory wander randomly
            base.UpdateMove();
        }

        /// <summary>
        /// Set the current navigation target to somewhere within the trigger zone,
        /// of a target object.
        /// </summary>
        /// <param name="target">The collider to move within.</param>
        private void SetCurrentTargetWithinTriggerOf(Collider col)
        {
            if (!col.bounds.Contains(transform.position))
            {
                float xSize = col.bounds.extents.x;
                float zSize = col.bounds.extents.z;

                Vector3 pos = col.transform.position;
                if (Random.value > 0.5f)
                {
                    pos.x += Random.Range(0, xSize);
                }
                else
                {
                    pos.x -= Random.Range(0, xSize);
                }

                if (Random.value > 0.5f)
                {
                    pos.z += Random.Range(0, zSize);
                }
                else
                {
                    pos.z -= Random.Range(0, zSize);
                }

                currentTarget = pos;
                return;
            }
        }

        #region Cooldown Helper Methods
        
        /// <summary>
        /// Checks if enough time has passed since the last stat evaluation.
        /// </summary>
        /// <returns>True if stats can be evaluated, false if still in cooldown.</returns>
        private bool CanEvaluateStats()
        {
            return Time.time - m_LastStatEvaluationTime >= m_StatEvaluationCooldown;
        }
        
        /// <summary>
        /// Checks if enough time has passed since the last memory target change.
        /// </summary>
        /// <returns>True if memory target can be changed, false if still in cooldown.</returns>
        private bool CanChangeMemoryTarget()
        {
            return Time.time - m_LastMemoryTargetChangeTime >= m_MemoryTargetCooldown;
        }
        
        /// <summary>
        /// Checks if enough time has passed since the last direction change.
        /// </summary>
        /// <returns>True if direction can be changed, false if still in cooldown.</returns>
        private bool CanChangeDirection()
        {
            return Time.time - m_LastDirectionChangeTime >= m_DirectionChangeCooldown;
        }
        
        /// <summary>
        /// Checks if enough time has passed since exiting turning mode.
        /// </summary>
        /// <returns>True if turning mode can be re-entered, false if still in cooldown.</returns>
        private bool CanEnterTurningMode()
        {
            return Time.time - m_LastTurningModeTime >= m_TurningModeCooldown;
        }
        
        /// <summary>
        /// Updates the stat evaluation timestamp.
        /// </summary>
        private void UpdateStatEvaluationTime()
        {
            m_LastStatEvaluationTime = Time.time;
        }
        
        /// <summary>
        /// Updates the memory target change timestamp.
        /// </summary>
        private void UpdateMemoryTargetChangeTime()
        {
            m_LastMemoryTargetChangeTime = Time.time;
        }
        
        /// <summary>
        /// Updates the direction change timestamp.
        /// </summary>
        private void UpdateDirectionChangeTime()
        {
            m_LastDirectionChangeTime = Time.time;
        }
        
        /// <summary>
        /// Updates the turning mode timestamp.
        /// </summary>
        private void UpdateTurningModeTime()
        {
            m_LastTurningModeTime = Time.time;
        }
        
        #endregion

#if UNITY_EDITOR

        public override string StatusText()
        {
            string msg = "";
            if (focusedStat != null)
            {
                msg += $"\n\nWandering towards {nearestMemoryOfInterest.about.name} (from Wander with Intent)";
            }
            else
            {
                msg += "\n\nWandering aimlessly (from Wander with Intent)";
            }

            return msg;
        }

        void Reset()
        {
            DisplayName = "Wander with Intent";
            Description = "Wander semi-randomly about the world. That is the character will usually pick a new destination in roughly the same direction unless they are reaching the edge of their range. If they are reaching the edge of their range then they will pick a new direction.";
            m_WeightMultiplier = 1;
        }
#endif
    }
}