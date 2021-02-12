using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace WizardsCode.Character
{
    public class ClickToSelect : MonoBehaviour
    {
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
                    m_CurrentlySelected = hit.collider.transform.root;
                }
            }
        }
    }
}
