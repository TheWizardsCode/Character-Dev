#if INK_PRESENT
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using WizardsCode.Character;

namespace WizardsCode.Utility
{
    /// <summary>
    /// This script, is used to a third person camera in relation to the player.
    /// 
    /// Based on the script used in the tutorial at https://youtu.be/537B1kJp9YQ
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField, Tooltip("The ActorController converts movement into animations and actions.")]
        BaseActorController m_Actor;
        [SerializeField, Tooltip("Invert the y axis so that moving the mouse up will result in the view moving up?")]
        bool m_InvertY = false;

        public float rotationPower = 3f;
        public float rotationLerp = 0.5f;
        public float speed = 1f;

        private bool _lookEnbled;
        Vector2 _look;
        Vector3 nextPosition;
        Quaternion nextRotation;

        private void Awake()
        {
            m_Actor = GetComponent<BaseActorController>();
        }
        public void OnLookMove(InputValue value)
        {
            _look = value.Get<Vector2>();
        }

        public GameObject followTransform;
      
        private void Update()
        {
            if (!Mouse.current.rightButton.isPressed)
            {
                return;
            }

            transform.rotation *= Quaternion.AngleAxis(_look.x * rotationPower, Vector3.up);
            followTransform.transform.rotation *= Quaternion.AngleAxis(_look.x * rotationPower, Vector3.up);

            #region Vertical Rotation
            float rotateY = m_InvertY ? -_look.y * rotationPower : _look.y * rotationPower;
            followTransform.transform.rotation *= Quaternion.AngleAxis(rotateY, Vector3.right);

            var angles = followTransform.transform.localEulerAngles;
            angles.z = 0;

            var angle = followTransform.transform.localEulerAngles.x;

            if (angle > 180 && angle < 340)
            {
                angles.x = 340;
            }
            else if (angle < 180 && angle > 40)
            {
                angles.x = 40;
            }


            followTransform.transform.localEulerAngles = angles;
            #endregion


            nextRotation = Quaternion.Lerp(followTransform.transform.rotation, nextRotation, Time.deltaTime * rotationLerp);

            transform.rotation = Quaternion.Euler(0, followTransform.transform.rotation.eulerAngles.y, 0);
            followTransform.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }
    }
}
#endif