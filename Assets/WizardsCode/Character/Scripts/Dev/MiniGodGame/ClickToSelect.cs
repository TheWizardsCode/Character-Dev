using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    public class ClickToSelect : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the camera attach to the selected object?")]
        bool m_IsFollowCamera = true;
        [SerializeField, Tooltip("If the camera is to follow the selected object then offset it by this vector.")]
        Vector3 m_CameraOffset = new Vector3(0, 3, -7);

        Transform m_CurrentlySelected;

        public GameObject CurrentlySelected
        {
            get
            {
                if (m_CurrentlySelected == null)
                {
                    return null;
                }
                else
                {
                    return m_CurrentlySelected.gameObject;
                }
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (hit.collider.transform.root.GetComponent<Brain>())
                    {
                        m_CurrentlySelected = hit.collider.transform.root;
                    } else
                    {
                        m_CurrentlySelected = null;
                    }
                }
            }

            if (m_IsFollowCamera && m_CurrentlySelected != null)
            {
                //TODO don't use camera.main
                Camera.main.transform.position = m_CurrentlySelected.position + m_CameraOffset;
                Camera.main.transform.LookAt(m_CurrentlySelected);
            }
        }
    }
}
