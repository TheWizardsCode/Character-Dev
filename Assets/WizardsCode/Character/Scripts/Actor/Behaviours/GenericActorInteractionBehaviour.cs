using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using WizardsCode.Stats;
using UnityEngine.AI;
using WizardsCode.Utility;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// A GenericCharacterInteractionBehaviour is a behaviour that happens
    /// involving two or more characters. Unlike the GenericInteractionBehaviour
    /// these behaviours need a number of other characters to be within range and in agreement that they will each carry out
    /// the same behaviour.
    /// 
    /// Characters manage their own stat effects based on the interaction they have.
    /// </summary>
    public class GenericActorInteractionBehaviour : AbstractAIBehaviour
    {
        [Header("Actor Interaction Config")]
        [SerializeField, Tooltip("What is the minimum number of actors need in the group carrying out this behaviour, including this actor. Before starting this behaviour at least this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MinGroupSize = 2;
        [SerializeField, Tooltip("What is the maximum number of actors need in the group carrying out this behaviour, including this actor. Before starting this behaviour at most this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MaxGroupSize = 5;
        [SerializeField, Tooltip("The distance from the centre of the interacting group that this actor should stand.")]
        float m_GroupDistance;
        [SerializeField, Tooltip("How long will the actor wait for handshaking to complete. If this time has passed and the minimum group size has not been met then the behaviour will be aborted.")]
        float m_HandshakeTimeout = 4;
        [SerializeField, Tooltip("How long before this actor can fire this same behaviour?")]
        float m_CooldownDuration = 60;
        [SerializeField, NavMeshAreaMask, Tooltip("The area mask that indicates NavMesh areas that this interaction can take place.")]
        public int m_NavMeshMask = NavMesh.AllAreas;

        float m_CooldownEndTime = float.MinValue;
        private float m_Duration;
        private float m_HandshakeEndTime;
        private bool m_IsHandshaking = false;
        List<Brain> participants = new List<Brain>();
        private NavMeshAgent m_Agent;
        private Vector3 groupCenter;
        private Transform interactionPointT;

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
                reasoning.AppendLine(" (see required senses)");
                return true;
            }
        }

        protected override void Init()
        {
            base.Init();

            m_Agent = GetComponentInParent<NavMeshAgent>();
            InteractionPointName = Brain.DisplayName + " - Interaction point";
        }

        protected override void OnUpdate()
        {
            UpdateParticipantsList();
            UpdateGroupPositions(true);

            if (m_IsHandshaking)
            {
                if (participants.Count >= m_MinGroupSize)
                {
                    m_IsHandshaking = false;

                    EndTime = Time.timeSinceLevelLoad + m_Duration;
                    AddCharacterInfluencers(m_Duration);
                    if (m_OnStartCue != null)
                    {
                        StartCoroutine(m_OnStartCue.Prompt(Brain.Actor));
                    }
                }
                else if (Time.timeSinceLevelLoad > m_HandshakeEndTime)
                {
                    m_IsHandshaking = false;
                    FinishBehaviour();
                }
                return;
            }

            Vector3 lookTarget = groupCenter;
            lookTarget.y = Brain.Actor.transform.position.y;
            Brain.Actor.transform.LookAt(lookTarget);

            // at the time of writing this comment we don't support adding participants during an interaction, this is here to accomodate for that when we do support it
            if (participants.Count > m_MaxGroupSize)
            {
                participants[participants.Count - 1].ActiveBlockingBehaviour.FinishBehaviour();
            }

            // If this is an interuptable behaviour participants may finish early, leaving us with a group that is too small
            if (participants.Count < m_MinGroupSize)
            {
                Debug.Log(Brain.DisplayName + " is stopping " + DisplayName + " because the group size is too small");
                FinishBehaviour();
                return;
            }

            base.OnUpdate();
        }

        internal override void StartBehaviour(float duration)
        {
            m_Duration = duration;
            m_CooldownEndTime = m_CooldownDuration + Time.timeSinceLevelLoad;
            m_HandshakeEndTime = Time.timeSinceLevelLoad + m_HandshakeTimeout;
            m_IsHandshaking = true;

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
        }

        /// <summary>
        /// Invite this actor to join a group proposing to enact this behaviour.
        /// </summary>
        /// <param name="brain">The brain extending the invite.</param>
        internal void InviteToGroup(Brain brain)
        {
            //TODO actors should be more likley to engage with other actors they like or who have valuable information. The m_IsHanshaking status impacts this behaviours weight. Increase it more for some actor invitations.
            m_IsHandshaking = true;
        }

        protected override float BaseWeight(Brain brain)
        {
            float weight = base.BaseWeight(brain);
            if (m_IsHandshaking)
            {
                return weight * 1.2f;
            } else
            {
                return weight;
            }
        }

        private void UpdateGroupPositions(bool setOnNavMesh)
        {
            // Find a point where we will meet the actors to interact
            float totalX = 0;
            float totalY = 0;
            for (int i = 0; i < participants.Count; i++)
            {
                totalX += participants[i].transform.position.x;
                totalY += participants[i].transform.position.z;
            }

            float centerX = totalX / participants.Count;
            float centerZ = totalY / participants.Count;
            groupCenter = new Vector3(centerX, 0, centerZ);
            Vector3 interactionPoint = groupCenter + (-m_GroupDistance * Brain.Actor.transform.forward);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(interactionPoint, out hit, 5, m_NavMeshMask))
            {
                interactionPoint = hit.position;
            }
            else
            {
                FinishBehaviour();
                return;
            }

            if (!interactionPointT)
            {
                interactionPointT = new GameObject(InteractionPointName).transform;
            }
            interactionPointT.position = interactionPoint;

            if (m_OnStartCue != null)
            {
                m_OnStartCue.Mark = InteractionPointName;
            } 
            
            if (setOnNavMesh)
            {
                if (m_Agent != null)
                {
                    m_Agent.SetDestination(interactionPoint);
                } else
                {
                    Debug.LogError(Brain.DisplayName + " is attempting to set an interaction point on the navmesh but does not have a NavMeshAgent component.");
                }
            }
        }

        private void UpdateParticipantsList()
        {
            participants.Clear();
            participants.Add(Brain);

            GenericActorInteractionBehaviour[] behaviours;
            for (int i = 0; i < SensedThings.Count; i++)
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
                    participants.Add(behaviours[0].Brain);
                }
            }
         }

        internal override void FinishBehaviour()
        {
            for (int i = 0; i < participants.Count; i++)
            {
                ((GenericActorInteractionBehaviour)participants[i].ActiveBlockingBehaviour).RemoveParticipant(Brain);
            }

            base.FinishBehaviour();
        }

        internal void RemoveParticipant(Brain brain)
        {
            participants.Remove(brain);
        }
    }
}
