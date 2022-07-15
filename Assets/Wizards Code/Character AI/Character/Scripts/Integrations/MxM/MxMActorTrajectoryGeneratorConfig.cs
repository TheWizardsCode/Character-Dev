#if MXM_PRESENT
using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character
{

    [CreateAssetMenu(fileName = "New MxMActorTrajectoryGeneratorConfig", menuName = "Wizards Code/Actor/MxM Trajectory Generator Configuration")]
    public class MxMActorTrajectoryGeneratorConfig : ScriptableObject
    {
        [Header("Motion Settings")]
        [Tooltip("The maximum speed of the trajectory with full stick")]
        public float MaxSpeed = 4.3f;

        [Tooltip("How responsive the trajectory movement is. Higher numbers make the trajectory move faster")]
        public float MoveResponsiveness = 15f;

        [Tooltip("How responsive the trajectory direction is. Higher numbers make the trajectory direction rotate faster")]
        public float TurnResponsiveness = 10f;

        [Tooltip("The mode that the trajectory is in. Changes the behaviour of the trajectory")]
        public ETrajectoryMoveMode TrajectoryMode = ETrajectoryMoveMode.Normal;
    }
}
#endif