using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Utility
{
    /// <summary>
    /// Drop this on any object you want to always be facing the camera. Discovers the main camera on Start,
    /// if the main camera cahnges during gameplay this component will no longer work.
    /// </summary>
    [ExecuteInEditMode]
    public class FaceCamera : MonoBehaviour
    {
        Camera m_Camera;

        private void Start()
        {
            m_Camera = Camera.main;
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
                m_Camera.transform.rotation * Vector3.up);
        }
    }
}
