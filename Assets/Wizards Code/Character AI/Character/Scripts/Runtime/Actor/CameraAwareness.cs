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
        [SerializeField, Tooltip("When this object is enabled should it seek to become the focus of the camera? Note that this does not guarantee it will get focus, other objects may compete."), ShowIf("canCameraFollowTarget"), BoxGroup("Follow Camera Settings")]
        bool m_StartWithFocus = false;
        [SerializeField, Tooltip("The follow camera to use when this object is the camera follow target. If this is not set then the currently active cinemachine camera will be used, if it is a follow camera. If the currently active camera is not a follow camera then the first follow camera found in the scene will be used."), ShowIf("canCameraFollowTarget"), BoxGroup("Follow Camera Settings")]
        CinemachineVirtualCameraBase m_FollowCamera;
        [SerializeField, Tooltip("The Look At target to use for the follow camera. If this is not set then the transform of this object will be used."), ShowIf("canCameraFollowTarget"), BoxGroup("Follow Camera Settings")]
        Transform cameraLookAtTarget;

        public bool CanCameraFollowTarget
        {
            get => canCameraFollowTarget;
            set => canCameraFollowTarget = value;
        }

        public CinemachineVirtualCameraBase FollowCamera
        {
            get {
                if (m_FollowCamera == null && CinemachineBrain.ActiveBrainCount > 0)
                {
                    var activeVCam = CinemachineBrain.GetActiveBrain(0).ActiveVirtualCamera;
                    if (activeVCam is CinemachineClearShot || activeVCam is CinemachineFollow)
                    {
                        m_FollowCamera = activeVCam as CinemachineVirtualCameraBase;
                    }
                }
                if (m_FollowCamera == null)
                {
                    m_FollowCamera = FindFirstObjectByType<CinemachineClearShot>();
                }
                if (m_FollowCamera == null)
                {
                    m_FollowCamera = FindFirstObjectByType<CinemachineVirtualCameraBase>();
                    if (m_FollowCamera != null && m_FollowCamera.GetComponent<CinemachineFollow>() == null)
                    {
                        m_FollowCamera = null;
                    }
                }
                if (m_FollowCamera == null)
                {
                    Debug.LogWarning("No suitable follow VirtualCamera found in the scene. Cannot set follow target.");
                }

                return m_FollowCamera;
            }
            set => m_FollowCamera = value;
        }

        void OnEnable()
        {
            if (m_StartWithFocus && canCameraFollowTarget)
            {
                SetAsFollowTarget();
                FollowCamera.Priority = 100;
            }
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
            FollowCamera.Follow = transform;
            if (cameraLookAtTarget == null)
            {
                cameraLookAtTarget = transform;
            }
            else 
            {
                FollowCamera.LookAt = cameraLookAtTarget;
            }
        }

        #region Editor
#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (canCameraFollowTarget && m_FollowCamera == null)
            {
                _ = FollowCamera; // This will set the follow camera to the first follow camera found in the scene.
            }
            if (cameraLookAtTarget == null)
            {
                cameraLookAtTarget = FindTransform(new string[] { "LookAt", "CameraLookAt", "CameraTarget", "Neck", "Head" });
            }
            if (cameraLookAtTarget == null)
            {
                cameraLookAtTarget = transform;
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