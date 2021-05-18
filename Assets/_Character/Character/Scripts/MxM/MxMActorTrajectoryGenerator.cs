#if MXM_PRESENT
using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character
{
    public class MxMActorTrajectoryGenerator : MxMTrajectoryGenerator_BasicAI
    {
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