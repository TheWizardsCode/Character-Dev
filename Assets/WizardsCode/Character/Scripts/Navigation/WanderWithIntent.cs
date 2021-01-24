using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character.Stats;
using WizardsCode.Stats;
using Random = UnityEngine.Random;

namespace WizardsCode.Character
{
    /// <summary>
    /// Make the cahracter wander semi-randomly semi-Purposefully. 
    /// They will wander until randomly until such a timme as a monitored
    /// stat falls below a given level, at which point they
    /// will try to find an influencer that will raise that stat.
    /// 
    /// When seeking an influencer they will consult their memory
    /// to find a suitable influencer.
    /// </summary>
    [RequireComponent(typeof(StatsController))]
    [RequireComponent(typeof(MemoryController))]
    public class WanderWithIntent : Wander
    {
        StatsController statsController;
        MemorySO nearestMemoryOfInterest;
        StatSO focusedStat;

        protected override void Start()
        {
            base.Start();
            statsController = GetComponent<StatsController>();
        }

        /// <summary>
        /// Checks to see if any stats are not in their desired states. If any are not then
        /// look in memories to see if the character knows of a place to address that. If
        /// a place is recalled and they are ready to return then they move towards it. 
        /// Otherwise wander.
        /// </summary>
        protected override void UpdateMove()
        {
            StatSO[] stats = statsController.GetStatsNotInDesiredState();

            nearestMemoryOfInterest = null;
            focusedStat = null;
            float sqrMagnitude = float.PositiveInfinity;
            for (int i = 0; i < stats.Length; i++) {
                //Debug.Log(gameObject.name + " desires to " + stats[i].goal + " " + stats[i].name + " to " + stats[i].desiredState.targetValue);
                //Debug.Log(stats[i].name + " is currently " + stats[i].value);

                // Find the nearest place that is in current memory that helps achieve one of the characters goals
                MemorySO[] memories = memory.GetMemoriesInfluencingStat(stats[i].name);
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

#if UNITY_EDITOR

        public override string StatusText()
        {
            string msg = "";
            if (focusedStat != null)
            {
                msg += "\n\nIntent (from Wander with Intent)";
                msg += "\nNearest object of interest: " + nearestMemoryOfInterest.about.name;
            }
            else
            {
                msg += "\n\nNo current Intent (from Wander with Intent)";
            }

            return msg;
        }
#endif
    }
}