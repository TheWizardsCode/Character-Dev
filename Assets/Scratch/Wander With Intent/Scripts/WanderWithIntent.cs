using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.Character.Stats;
#if UNITY_EDITOR
using WizardsCode.Editor;
#endif

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
        MemoryController memory;
        StatsController statsController;
        MemorySO nearestMemoryOfInterest;
        StatSO focusedStat;

        void Start()
        {
            memory = GetComponent<MemoryController>();
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

                MemorySO[] memories = memory.RecallMemoriesInfluencingStat(stats[i].name);

                for (int y = 0; y < memories.Length; y++)
                {
                    if (memories[y].readyToReturn)
                    {
                        if (stats[i].goal == DesiredState.Goal.Increase && memories[y].influence < 0)
                        {
                            continue;
                        }
                        else if (stats[i].goal == DesiredState.Goal.Decrease && memories[y].influence > 0)
                        {
                            continue;
                        }

                        float distance = Vector3.SqrMagnitude(memories[y].about.transform.position - gameObject.transform.position);
                        if (distance < sqrMagnitude)
                        {
                            focusedStat = stats[i];
                            nearestMemoryOfInterest = memories[y];
                            sqrMagnitude = distance;
                        }
                    }
                }

                if (nearestMemoryOfInterest != null)
                {
                    //Debug.Log("It looks like " + nearestMemoryOfInterest.about + " is the best place to achieve that goal");

                    GameObject target = nearestMemoryOfInterest.about;
                    Collider col = target.GetComponent<Collider>();

                    if (!col.bounds.Contains(transform.position))
                    {
                        float xSize = target.transform.lossyScale.x;
                        float zSize = target.transform.lossyScale.z;

                        Vector3 pos = target.transform.position;
                        if (Random.value > 0.5f)
                        {
                            pos.x += Random.Range(xSize, col.bounds.max.x - xSize);
                        }
                        else
                        {
                            pos.x -= Random.Range(xSize, col.bounds.max.x - xSize);
                        }

                        if (Random.value > 0.5f)
                        {
                            pos.z += Random.Range(zSize, col.bounds.max.x - zSize);
                        }
                        else
                        {
                            pos.z -= Random.Range(zSize, col.bounds.max.x - zSize);
                        }
                        currentTarget = pos;
                        return;
                    }
                }
            }

            base.UpdateMove();
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Vector3 pos = transform.position;
            pos.x += 1;
            pos.y += transform.lossyScale.y * 2;

            string msg = gameObject.name;
            if (focusedStat != null)
            {
                msg += "\n" + focusedStat.describeGoal;
                msg += "\nNearest object of interest: " + nearestMemoryOfInterest.about.name;
            }
            ExtendedGizmos.DrawString(msg, pos);

            DrawWanderAreaGizmo();
            DrawWanderTargetGizmo();
            DrawWanderRangeGizmo();
        }
#endif
    }
}