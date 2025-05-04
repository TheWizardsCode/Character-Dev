using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

namespace WizardsCode 
{
    public class CameraAwareness : MonoBehaviour
    {
        // Camera Awareness Settings
        [SerializeField, Tooltip("The maximum distance the object can be from the camera to be considered in view."), BoxGroup("Camera Awareness Settings")]
        float maxDistanceFromCamera = 75f;

        [Space]
        // Camera Follow Settings
        [SerializeField, Tooltip("Is this object a potential camera follow target?"), BoxGroup("Follow Camera Settings")]
        bool canCameraFollowTarget = false;
        [SerializeField, Tooltip("The follow camera to use when this object is the camera follow target. If this is not set then the currently active cinemachine camera will be used, if it is a follow camera. If the currently active camera is not a follow camera then the first follow camera found in the scene will be used."), ShowIf("canCameraFollowTarget"), BoxGroup("Follow Camera Settings")]
        CinemachineCamera followCamera;
        [SerializeField, Tooltip("The Look At target to use for the follow camera. If this is not set then the transform of this object will be used."), ShowIf("canCameraFollowTarget"), BoxGroup("Follow Camera Settings")]
        Transform cameraLookAtTarget;

        public bool CanCameraFollowTarget
        {
            get => canCameraFollowTarget;
            set => canCameraFollowTarget = value;
        }

        void OnBecameVisible()
        {
            if (Vector3.Distance(transform.position, Camera.main.transform.position) > maxDistanceFromCamera)
            {
            return;
            }

            Vector3 directionToTransform = transform.position - Camera.main.transform.position;
            Ray ray = new Ray(Camera.main.transform.position, directionToTransform.normalized);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxDistanceFromCamera))
            {
                if (hitInfo.collider.gameObject != gameObject)
                {
                    Debug.Log($"{gameObject.name} is not in view of the camera, obstructed by {hitInfo.collider.gameObject.name}!");
                }
                else
                {
                    Debug.Log($"{gameObject.name} is in view of the camera!");
                }
            }
        }

        [Button("Set as Follow Target"), ShowIf("canCameraFollowTarget")]
        /// <summary>
        /// Calling this will cause the Cinemachine camera to follow this object.
        /// </summary>
        public void SetAsFollowTarget() 
        {
            if (!canCameraFollowTarget)
            {
                Debug.LogWarning("This object is not a camera follow target. Cannot set follow target.");
                return;
            }

            if (followCamera == null)
            {
                followCamera = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera as CinemachineCamera;
                if (followCamera != null && followCamera.GetComponent<CinemachineFollow>() == null)
                {
                    followCamera = null; // the active camera is not a follow camera
                }
            }
            if (followCamera == null)
            {
                followCamera = FindFirstObjectByType<CinemachineFollow>()?.GetComponent<CinemachineCamera>();
            }
            if (followCamera == null)
            {
                Debug.LogWarning("No follow camera found in the scene. Cannot set follow target.");
                return;
            }

            followCamera.Follow = transform;
            if (cameraLookAtTarget == null)
            {
                cameraLookAtTarget = transform;
            }
            else 
            {
                followCamera.LookAt = cameraLookAtTarget;
            }
        }

        #region Editor
#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (CanCameraFollowTarget) 
            {
                if (cameraLookAtTarget == null)
                {
                    cameraLookAtTarget = FindTransform(new string[] { "LookAt", "CameraLookAt", "CameraTarget", "Neck", "Head" });
                }
                if (cameraLookAtTarget == null)
                {
                    cameraLookAtTarget = transform;
                }
            }
        }

        /// <summary>
        /// Search the children in the hierarchy of this game object for a transform with one of the given names.
        /// All names will be searched in the order they are given.
        /// All names will be searched in the provided case and all lower case.
        /// The first transform found will be returned.
        /// If no transform is found then null will be returned.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        private Transform FindTransform(string[] names)
        {
            foreach (string name in names)
            {
                Transform transform = FindTransformRecursive(base.transform, name);
                if (transform != null)
                {
                    return transform;
                }
                transform =FindTransformRecursive(base.transform, name.ToLower());
                if (transform != null)
                {
                    return transform;
                }
            }
            return null;
        }

        private Transform FindTransformRecursive(Transform parent, string name)
        {
            if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                Transform result = FindTransformRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
#endif
        #endregion
    }
}