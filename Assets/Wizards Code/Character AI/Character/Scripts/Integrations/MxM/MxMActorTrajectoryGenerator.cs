#if MXM_PRESENT
using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WizardsCode.Character
{
    public class MxMActorTrajectoryGenerator : MxMTrajectoryGenerator_BasicAI
    {
        NavMeshAgent m_NavAgent;
        void Start()
        {
            m_NavAgent = GetComponentInChildren<NavMeshAgent>();
            StrafeDirection = Vector3.forward;
        }

        public Vector3 desiredVelocity
        {
            get { return m_NavAgent.desiredVelocity; }
        }

        public float remainingDistance
        {
            get { return m_NavAgent.remainingDistance; }
        }

        public void SetTrajectoryConfig(MxMActorTrajectoryGeneratorConfig config)
        {
            if (config == null)
                return;

            MaxSpeed = config.MaxSpeed;
            MoveResponsiveness = config.MoveResponsiveness;
            TurnResponsiveness = config.TurnResponsiveness;
            TrajectoryMode = config.TrajectoryMode;
        }
    }
}
#endif