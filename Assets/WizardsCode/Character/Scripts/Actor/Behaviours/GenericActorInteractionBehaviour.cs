using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;
using WizardsCode.Stats;
using UnityEngine.AI;

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
        [SerializeField, Tooltip("What is the minimum number of actors need in the group carrying out this behaviour. Before starting this behaviour at least this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MinGroupSize = 1;
        [SerializeField, Tooltip("What is the maximum number of actors need in the group carrying out this behaviour. Before starting this behaviour at most this many actors need to have agreed to participate in the shared behaviour. The group members are identified in the Senses definitions.")]
        int m_MaxGroupSize = 5;

        [SerializeField, Tooltip("How long before this actor can fire this same behaviour?")]
        float m_CooldownDuration = 60;

        float m_CooldownEndTime = float.MinValue;
        List<Brain> participants = new List<Brain>();
        private NavMeshAgent m_Agent;

        public override bool IsAvailable
        {
            get
            {
                if (Time.timeSinceLevelLoad < m_CooldownEndTime)
                {
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
        }

        protected override void OnUpdate()
        {
            UpdateParticipantsList();
            MoveToInteractionPoint();

            // at the time of writing this comment we don't support adding participants during an interaction, this is here to accomodate for that when we do support it
            if (participants.Count > m_MaxGroupSize)
            {
                participants[participants.Count - 1].ActiveBlockingBehaviour.FinishBehaviour();
            }

            // If this is an interuptable behaviour participants may finish early, leaving us with a group that is too small
            if (participants.Count < m_MinGroupSize)
            {
                Debug.Log("Stopping behaviour because group size is too small");
                FinishBehaviour();
                return;
            }

            base.OnUpdate();
        }

        internal override void StartBehaviour(float duration)
        {
            m_CooldownEndTime = m_CooldownDuration + Time.timeSinceLevelLoad;

            base.StartBehaviour(duration);
        }

        private void MoveToInteractionPoint()
        {
            if (m_Agent != null)
            {
                // Find a point where we will meet the actos to interact
                var totalX = transform.position.x;
                var totalY = transform.position.y;
                for (int i = 0; i < participants.Count; i++)
                {
                    totalX += participants[i].transform.position.x;
                    totalY += participants[i].transform.position.z;
                }
                var centerX = totalX / (participants.Count + 1);
                var centerZ = totalY / (participants.Count + 1);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(new Vector3(centerX, 0, centerZ), out hit, 5, NavMesh.AllAreas))
                {
                    m_Agent.SetDestination(hit.position);
                }
                else
                {
                    FinishBehaviour();
                }
            }
        }

        private void UpdateParticipantsList()
        {
            participants.Clear();

            GenericActorInteractionBehaviour[] behaviours;
            for (int i = 0; i < SensedThings.Count; i++)
            {
                // check the other actor has this behaviour
                behaviours = SensedThings[i].GetComponentsInChildren<GenericActorInteractionBehaviour>();
                if (behaviours.Length == 0)
                {
                    continue;
                }

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
