#if MXM_PRESENT
using MxM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.MxMExtensions;

namespace WizardsCode.Character
{
    public class MxMActorController : BaseActorController
    {
        public enum LocomotionType { Walk, Run, Sprint }

        [Header("MxM Locomotion")]
        [SerializeField] private float m_speedDowngradeTime = 0.3f;
        [SerializeField] private float m_favourMultiplier = 0.6f;

        private LocomotionType m_currentLocomotionType = LocomotionType.Walk; 
        private float m_speedTimer = -1f;

        private MxMAnimator m_mxmAnimator;
        private MxMActorTrajectoryGenerator m_trajectoryGenerator;

        private ETags m_runTagHandle;
        private ETags m_sprintTagHandle;

        protected void Start()
        {
            m_mxmAnimator = GetComponentInChildren<MxMAnimator>();
            m_trajectoryGenerator = GetComponentInChildren<MxMActorTrajectoryGenerator>();

            m_runTagHandle = m_mxmAnimator.CurrentAnimData.FavourTagFromName(LocomotionType.Run.ToString());
            m_sprintTagHandle = m_mxmAnimator.CurrentAnimData.FavourTagFromName(LocomotionType.Sprint.ToString());

            m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
        }

        public override void Prompt(ActorCue cue)
        {
            if (cue is ActorCueMxM)
            {
                Vector3 pos = LookAtTarget.position;
                ((ActorCueMxM)cue).contactPoints = new EventContact[] { new EventContact(pos, 0) };
            }

            base.Prompt(cue);
        }

#region Locomotion Style
        public void BeginSprint()
        {
            if (m_currentLocomotionType == LocomotionType.Run)
                m_mxmAnimator.RemoveFavourTags(m_runTagHandle);

            m_currentLocomotionType = LocomotionType.Sprint;

            m_mxmAnimator.AddFavourTags(m_sprintTagHandle);
        }

        public void ResetFromSprint()
        {
            if (m_currentLocomotionType != LocomotionType.Sprint)
                return;

            m_currentLocomotionType = LocomotionType.Run;
            m_speedTimer = 0f;

            float inputMag = m_trajectoryGenerator.InputVector.sqrMagnitude;

            if (inputMag > m_runSqrMagnitude)
            {
                m_mxmAnimator.AddFavourTags(m_runTagHandle);
            }

            m_mxmAnimator.RemoveFavourTags(m_sprintTagHandle);
        }

        protected override void Update()
        {
            base.Update();

            UpdateSpeedRamp();
        }

        public void UpdateSpeedRamp()
        {
            float inputMag = m_trajectoryGenerator.InputVector.sqrMagnitude;

            switch (m_currentLocomotionType)
            {
                case LocomotionType.Walk:
                {
                    if (inputMag > m_runSqrMagnitude)
                    {
                        m_currentLocomotionType = LocomotionType.Run;
                        m_mxmAnimator.AddFavourTags(m_runTagHandle);
                        m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
                    }
                    break;
                }
                case LocomotionType.Run:
                {
                    if (inputMag < m_runSqrMagnitude)
                    {
                        if (m_speedTimer < -Mathf.Epsilon)
                        {
                            m_speedTimer = 0f;
                        }

                        m_speedTimer += Time.deltaTime;

                        if (m_speedTimer > m_speedDowngradeTime)
                        {
                            m_mxmAnimator.RemoveFavourTags(m_runTagHandle);
                            m_mxmAnimator.SetFavourMultiplier(m_favourMultiplier);
                            m_currentLocomotionType = LocomotionType.Walk;
                            m_speedTimer = -1f;
                        }
                    }
                }
                break;
            }
        }
#endregion
    }
}
#endif