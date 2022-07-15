using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

namespace WizardsCode.AnimationControl
{
    [RequireComponent(typeof(Animator)),
        RequireComponent(typeof(NavMeshAgent)),
        RequireComponent(typeof(NavMeshConnector))]
    public class HumanoidController : MonoBehaviour
    {
        [Header("IK")]
        [Tooltip("If true then this script will control IK configuration of the character.")]
        public bool isIKActive = false;

        [Header("Interaction Offsets")]
        [SerializeField, Tooltip("An offset applied to the position of the character when they sit.")]
        float sittingOffset = 0.25f;

        public Transform leftFootPosition = default;
        public Transform rightFootPosition = default;

        Quaternion desiredRotation = default;
        bool isRotating = false;

        Animator animator;
        NavMeshAgent agent;
        NavMeshConnector connector;

        private void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            connector = GetComponent<NavMeshConnector>();
        }

        /// <summary>
        /// Instruct the character to move to a defined positiion and, optionally, 
        /// make callbacks at various points in the process.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="arrivingCallback">Called as the character is arriving at the destination. This is called when the character is entering the ArrivingDistance defined in the NavMeshController.</param>
        /// <param name="arrivedCallback">Called as the character arrives at the destination. Arrival is defined by the NavMeshAgent.</param>
        /// <param name="stationaryCallback">Called once the character has stopped moving.</param>
        public void MoveTo(Vector3 position, Action arrivingCallback, Action arrivedCallback, Action stationaryCallback)
        {
            connector.onArriving = arrivingCallback;
            connector.onArrived = arrivedCallback;
            connector.onStationary = stationaryCallback;
            agent.SetDestination(position);
        }

        public void Sit(Transform sitPosition)
        {
            MoveTo(sitPosition.position, () =>
            {
                TurnTo(sitPosition.rotation);
            },
            () =>
            {
                Vector3 pos = sitPosition.position;
                pos.z -= sittingOffset; // slide back in the chair a little
                MoveTo(pos, null, null, () =>
                {
                    isIKActive = true;
                    animator.SetBool("Sitting", true);
                });
            },
            null);
        }

        internal void Stand()
        {
            Vector3 pos = transform.position;
            pos.z += sittingOffset;
            MoveTo(pos, null,
            () =>
            {
                isIKActive = false;
                animator.SetBool("Sitting", false);
            },
            null);
        }

        private void TurnTo(Quaternion rotation)
        {
            desiredRotation = rotation;
            isRotating = true;
        }

        private void Update()
        {
            if (isRotating && transform.rotation != desiredRotation)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 0.05f);
            }
        }

        void OnAnimatorIK()
        {
            if (!isIKActive) return;

            if (rightFootPosition != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition.position);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootPosition.rotation);
            }
            if (leftFootPosition != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition.position);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootPosition.rotation);
            }
        }
    }

}
