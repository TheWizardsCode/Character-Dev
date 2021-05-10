#if MXM_PRESENT
using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character
{
    public class MxMActorTrajectoryGenerator : MxMTrajectoryGenerator_BasicAI
    {
        public Vector3 desiredVelocity
        {
            get { return m_navAgent.desiredVelocity; }
        }

        public float remainingDistance
        {
            get { return m_navAgent.remainingDistance; }
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