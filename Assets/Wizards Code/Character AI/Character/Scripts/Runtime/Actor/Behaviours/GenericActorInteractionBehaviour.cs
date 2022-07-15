using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using WizardsCode.Stats;
using UnityEngine.AI;
using WizardsCode.BackgroundAI;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// A GenericActorInteractionBehaviour is a behaviour that happens
    /// involving two or more characters. Unlike the GenericInteractionBehaviour
    /// these behaviours need a number of other characters to be within range and in agreement that they will each carry out
    /// the same behaviour.
    /// 
    /// Characters manage their own stat effects based on the interaction they have.
    /// </summary>
    public class GenericActorInteractionBehaviour : AbstractAIBehaviour
    {
        #region Inspector Properties
        [Header("Actor Interaction Config")]
        [SerializeField, Tooltip("If consent is required then the interaction will only start when enough actors have consented to participate. So, for example, a conversation will require consent but an attack will not.")]
        bool m_RequireConsent = true;
        [SerializeField, Tooltip("What is the minimum number of actors need in the group carrying out this behaviour, including this actor. Before starting this behaviour at least this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MinGroupSize = 2;
        [SerializeField, Tooltip("What is the maximum number of actors need in the group carrying out this behaviour, including this actor. Before starting this behaviour at most this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MaxGroupSize = 5;
        [SerializeField, Tooltip("The distance from the centre of the interacting group that this actor should stand.")]
        float m_GroupDistance = 1;
        [SerializeField, Tooltip("How long will the actor wait for handshaking to complete. If this time has passed and the minimum group size has not been met then the behaviour will be aborted.")]
        float m_HandshakeTimeout = 4;
        [SerializeField, Tooltip("How long before this actor can fire this same behaviour?")]
        float m_CooldownDuration = 60;
        [SerializeField, NavMeshAreaMask, Tooltip("The area mask that indicates NavMesh areas that this interaction can take place.")]
        public int m_NavMeshMask = NavMesh.AllAreas;
        #endregion

        float m_CooldownEndTime = float.MinValue;
        private float m_Duration;
        private float m_HandshakeEndTime;
        private bool m_IsHandshaking = false;
        internal List<StatsTracker> participants = new List<StatsTracker>();
        private NavMeshAgent m_Agent;
        protected float m_SqrArrivingDistance;

        private string InteractionPointName { get; set; }

        public override bool IsAvailable
        {
            get
            {
                if (Time.timeSinceLevelLoad < m_CooldownEndTime)
                {
                    reasoning.Clear();
                    reasoning.AppendLine("Still on cooldown for " + this.DisplayName);
                    return false;
                }

                if (!base.IsAvailable) return false;

                reasoning.Append("Maybe ");
                reasoning.Append(DisplayName);
                reasoning.AppendLine(" (sensed required objects)");
                return true;
            }
        }

        protected override void Init()
        {
            base.Init();

            m_Agent = GetComponentInParent<NavMeshAgent>();
            InteractionPointName = Brain.DisplayName + " - Interaction point";

            m_SqrArrivingDistance = Brain.Actor.ArrivingDistance * Brain.Actor.ArrivingDistance;
        }

        protected override void OnUpdateState()
        {
            if (CurrentState == State.Inactive) return;

            //TODO OPTIMIZATION probably don't need to update participants and position every tick
            int count = participants.Count;
            UpdateParticipantsList();
            UpdateInteractionPosition(true);

            if (m_IsHandshaking)
            {
                if (participants.Count >= m_MinGroupSize)
                {
                    m_IsHandshaking = false;

                    EndTime = Time.timeSinceLevelLoad + m_Duration;
                    AddCharacterInfluencers(m_Duration);
                    if (m_OnStartCue != null)
                    {
                        Brain.Actor.Prompt(m_OnStartCue);
                        EndTime = Time.timeSinceLevelLoad + m_OnStartCue.m_Duration;
                    }
                }
                else if (Time.timeSinceLevelLoad > m_HandshakeEndTime)
                {
                    m_IsHandshaking = false;
                    EndTime = FinishBehaviour();
                }
                return;
            }

            // at the time of writing this comment we don't support adding participants during an interaction, this is here to accomodate for that when we do support it
            if (participants.Count > m_MaxGroupSize)
            {
                if (participants[participants.Count - 1] is Brain)
                {
                    AbstractAIBehaviour behaviour = ((Brain)participants[participants.Count - 1]).ActiveBlockingBehaviour;
                    if (behaviour != null)
                    {
                        behaviour.FinishBehaviour();
                    }
                }
            }

            // If this is an interuptable behaviour participants may finish early, leaving us with a group that is too small
            if (participants.Count < m_MinGroupSize)
            {
                Debug.Log(Brain.DisplayName + " is stopping " + DisplayName + " because the group size is too small");
                EndTime = FinishBehaviour();
            }

            base.OnUpdateState();
        }

        /// <summary>
        /// Test to see if the actor is at the interaction point for this action.
        /// </summary>
        protected virtual bool IsAtInteractionPoint
        {
            get
            {
                return (Brain.Actor.MoveTargetPosition - transform.position).sqrMagnitude < 1f;
            }
        }

        internal override void StartBehaviour()
        {
            m_CooldownEndTime = m_CooldownDuration + Time.timeSinceLevelLoad;
            m_HandshakeEndTime = Time.timeSinceLevelLoad + m_HandshakeTimeout;
            m_IsHandshaking = m_RequireConsent;

            GenericActorInteractionBehaviour[] behaviours;
            for (int i = 0; i < SensedThings.Count; i++)
            {
                //TODO would be more efficient to pull the behaviours from the target brain
                //TODO this is duplicated in UpdateParticipantsList
                behaviours = SensedThings[i].GetComponentsInChildren<GenericActorInteractionBehaviour>();
                if (behaviours.Length == 0)
                {
                    continue;
                }

                for (int idx = 0; idx < behaviours.Length; idx++)
                {
                    if (behaviours[idx].DisplayName == this.DisplayName)
                    {
                        behaviours[idx].InviteToGroup(Brain);
                    }
                }
            }

            base.StartBehaviour();

            UpdateParticipantsList();
            UpdateInteractionPosition(true);
        }

        /// <summary>
        /// Invite this actor to join a group proposing to enact this behaviour.
        /// </summary>
        /// <param name="stats">The stats of the participant to invite.</param>
        internal void InviteToGroup(StatsTracker stats)
        {
            //TODO actors should be more likley to engage with other actors they like or who have valuable information.             
            m_IsHandshaking = m_RequireConsent;
        }

        protected override float BaseWeight(StatsTracker stats)
        {
            float weight = base.BaseWeight(stats);
            if (m_IsHandshaking)
            {
                return weight * 1.2f;
            } else
            {
                return weight;
            }
        }

        protected Vector3 m_InteractionPoint;
        protected Vector3 m_InteractionGroupCenter;

        protected override State CheckEnvironment()
        {
            if (!IsAtInteractionPoint)
            {
                return State.Starting;
            }
            return base.CheckEnvironment();
        }

        /// <summary>
        /// Find a position, relative to the other actor, that is ideal for this interaction to take place.
        /// 
        /// </summary>
        /// <param name="requireNavmesh">If true ensure that the position is on the navmesh.</param>
        protected virtual void UpdateInteractionPosition(bool requireNavmesh)
        {
            // Find a point where we will meet the actors to interact
            float totalX = 0;
            float totalY = 0;
            for (int i = 0; i < participants.Count; i++)
            {
                if (participants[i] != Brain)
                {
                    totalX += participants[i].GetInteractionPosition().x;
                    totalY += participants[i].GetInteractionPosition().z;
                    BaseActorController other = participants[i].GetComponentInParent<BaseActorController>();
                    if (other != null)
                    {
                        m_ActorController.LookAtTarget = other.head;
                    } else
                    {
                        m_ActorController.LookAtTarget = participants[i].transform;
                        m_ActorController.LookAtTarget.position = new Vector3(m_ActorController.LookAtTarget.position.x, m_ActorController.LookAtTarget.position.y, m_ActorController.LookAtTarget.position.z);
                    }
                }
            }

            float centerX;
            float centerZ;
            float count = m_RequireConsent ? participants.Count : participants.Count - 1;
            if (m_RequireConsent)
            {
                // If it requires consent, assume participants will move towards one another
                totalX += Brain.GetInteractionPosition().x;
                totalY += Brain.GetInteractionPosition().z;
                centerX = totalX / count;
                centerZ = totalY / count;
            } else 
            {
                centerX = totalX / count;
                centerZ = totalY / count;
            }

            m_InteractionGroupCenter = new Vector3(centerX, 0, centerZ);
            Vector3 heading = m_InteractionGroupCenter - Brain.Actor.transform.position;
            heading.y = 0;
            heading.Normalize();
            m_InteractionPoint = m_InteractionGroupCenter + (-m_GroupDistance * heading);

            if (Terrain.activeTerrain != null)
            {
                m_InteractionPoint.y = Terrain.activeTerrain.SampleHeight(m_InteractionPoint);
            }

            if (requireNavmesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(m_InteractionPoint, out hit, 5, m_NavMeshMask))
                {
                    m_InteractionPoint = hit.position;
                }
                else
                {
                    EndTime = FinishBehaviour();
                    return;
                }
            }
            Brain.Actor.MoveTargetPosition = m_InteractionPoint;

            // If we are too far away start moving and set the callback to perform this action when arriving
            if (Vector3.SqrMagnitude(Brain.Actor.MoveTargetPosition - m_InteractionPoint) > m_SqrArrivingDistance)
            {
                Brain.Actor.MoveTo(m_InteractionPoint);
            }
        }

        private void UpdateParticipantsList()
        {
            float nearestSqrMag = float.MaxValue;

            participants.Clear();
            participants.Add(Brain);

            GenericActorInteractionBehaviour[] behaviours;
            for (int i = 0; i < SensedThings.Count; i++)
            {
                if (m_RequireConsent)
                {
                    //TODO would be more efficient to pull the behaviours from the target brain
                    //TODO this is duplicated in StartBehaviour
                    behaviours = SensedThings[i].GetComponentsInChildren<GenericActorInteractionBehaviour>();
                    if (behaviours.Length == 0)
                    {
                        continue;
                    }

                    //TODO Don't test if the currently active behaviour is the same. Instead have a "handshake" protocol in which both actors agree to participate in the same behaviour on the next frame.
                    AbstractAIBehaviour behaviour = behaviours[0].Brain.ActiveBlockingBehaviour;
                    if (behaviour != null && behaviour.DisplayName == this.DisplayName)
                    {
                        float sqrMag = Vector3.SqrMagnitude(transform.root.position - behaviours[0].Brain.Actor.transform.position);

                        if (sqrMag < nearestSqrMag)
                        {
                            participants.Add(behaviours[0].Brain);
                            nearestSqrMag = sqrMag;
                        }
                    }
                }
                else
                {
                    StatsTracker stats = SensedThings[i].GetComponentInChildren<StatsTracker>();
                    if (stats)
                    {
                        float sqrMag = Vector3.SqrMagnitude(transform.root.position - stats.transform.root.position);

                        if (sqrMag < nearestSqrMag)
                        {
                            participants.Add(stats);
                            nearestSqrMag = sqrMag;
                        }
                    }
                }

                while (participants.Count > m_MaxGroupSize)
                {
                    participants.RemoveAt(Random.Range(0, participants.Count - 1)); // the last one will always be nearest if oversized
                }
            }
         }

        /// <summary>
        /// Finish the behaviour, prompting any cue needed.
        /// </summary>
        /// <returns>The time at which this behaviour should end, if zero then it ends immediately.</returns>
        internal override float FinishBehaviour()
        {
            for (int i = 0; i < participants.Count; i++)
            {
                if (participants[i] is Brain)
                {
                    AbstractAIBehaviour behaviour = ((Brain)participants[i]).ActiveBlockingBehaviour;
                    if (behaviour != null)
                    {
                        ((GenericActorInteractionBehaviour)behaviour).RemoveParticipant(Brain);
                    }
                }
            }

            return base.FinishBehaviour();
        }

        internal void RemoveParticipant(Brain brain)
        {
            participants.Remove(brain);
        }
    }
}
