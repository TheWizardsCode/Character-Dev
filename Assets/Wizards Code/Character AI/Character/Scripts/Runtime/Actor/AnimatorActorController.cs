using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Character
{
    public class AnimatorActorController : BaseActorController
    {
        #region InspectorParameters
        [Header("Animation")]
        [SerializeField, Tooltip("Should the character use Root Motion baked into the animations?")]
        bool m_UseRootMotion = false;
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the forward speed of the character.")]
        private string m_SpeedParameterName = "Forward";
        [SerializeField, Tooltip("The name of the parameter in the animator that sets the turn angle of the character.")]
        private string m_TurnParameterName = "Turn";
        [SerializeField, Tooltip("The speed of this character when at a run. It will usually be going slower than this, and for short periods, can go faster (at a spring).")]
        private float m_RunningSpeed = 8;
        [Tooltip("If true then this script will control IK configuration of the character.")]
        public bool isFootIKActive = false;
        #endregion

        Transform m_LeftFootPosition = default;
        Transform m_RightFootPosition = default;

        protected override void Awake()
        {
            base.Awake();
            Animator animator = GetComponentInChildren<Animator>();
            if (animator)
            {
                if (animator.applyRootMotion != m_UseRootMotion)
                {
                    Debug.LogWarning($"`{displayName}` does not have a consistent root motion setting in the AnimatorActorController and the Animator. Overriding to `{m_UseRootMotion}` from the AnimatorActorController.");
                    animator.applyRootMotion = m_UseRootMotion;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            SetForwardAndTurnParameters();
        }

        private void SetForwardAndTurnParameters()
        {
            float magVelocity = m_Agent.velocity.magnitude;
            float speedParam = 0;
            if (!Mathf.Approximately(magVelocity, 0))
            {
                speedParam = magVelocity / m_MaxSpeed;
            }

            Vector3 s = m_Agent.transform.InverseTransformDirection(m_Agent.velocity).normalized;
            float turn = s.x;

            if (m_Animator)
            {
                m_Animator.SetFloat(m_SpeedParameterName, speedParam);
                m_Animator.SetFloat(m_TurnParameterName, turn);
            }

            if (Mathf.Abs(speedParam) > 0.05 || Mathf.Abs(turn) > 0.05)
            {
                state = States.Moving;
            }
        }


        void OnAnimatorIK()
        {
            LookAtIK();
            FeetIK();
        }

        private void FeetIK()
        {
            if (!m_Animator || !isFootIKActive) return;

            if (m_RightFootPosition != null)
            {
                m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, m_RightFootPosition.position);
                m_Animator.SetIKRotation(AvatarIKGoal.RightFoot, m_RightFootPosition.rotation);
            }
            if (m_LeftFootPosition != null)
            {
                m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, m_LeftFootPosition.position);
                m_Animator.SetIKRotation(AvatarIKGoal.LeftFoot, m_LeftFootPosition.rotation);
            }
        }
    }
}