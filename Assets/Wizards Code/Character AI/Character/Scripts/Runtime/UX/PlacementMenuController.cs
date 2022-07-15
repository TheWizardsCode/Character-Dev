using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System;

namespace WizardsCode.Character.UX
{
    /// <summary>
    /// The menu to manage placement of interactables by the player.
    /// Right click will bring up this menu then left click on the item to place.
    /// </summary>
    public class PlacementMenuController : MonoBehaviour
    {
        [SerializeField, Tooltip("Surface to place objects onto.")]
        GameObject ground;

        [SerializeField, Tooltip("The Rect Transform containing the menu.")]
        RectTransform m_MenuRect;

        [SerializeField, Tooltip("The button template to use for each item we can spawn.")]
        public RectTransform m_ButtonTemplate;

        public Interactable[] m_PossibleInteractables;
        private Vector3 m_PlacementPosition;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void ShowMenu()
        {
            m_PlacementPosition = Input.mousePosition;
            if (m_PlacementPosition.x + m_MenuRect.rect.width > Screen.width)
            {
                m_PlacementPosition.x -= m_MenuRect.rect.width - 10;
            }
            if (m_PlacementPosition.y < m_MenuRect.rect.height)
            {
                m_PlacementPosition.y += m_MenuRect.rect.height + 10;
            }

            m_MenuRect.position = m_PlacementPosition;

            m_MenuRect.gameObject.SetActive(true);
        }

        private void HideMenu()
        {
            m_MenuRect.gameObject.SetActive(false);
        }

        private void CreateMenu()
        {
            // Remove any previous menu elements
            GameObject go;
            for (int i = 0; i < m_MenuRect.transform.childCount; i++)
            {
                go = m_MenuRect.transform.GetChild(i).gameObject;
                go.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                Destroy(go);
            }
            m_MenuRect.DetachChildren();

            float buttonWidth = 160;
            float buttonHeight = 30;
            float buttonSpacing = 10;
            m_MenuRect.sizeDelta = new Vector2(buttonWidth + 20, (buttonHeight + buttonSpacing) * m_PossibleInteractables.Length);

            // Add all menu elements
            for (int i = 0; i < m_PossibleInteractables.Length; i++)
            {
                Interactable interactable = m_PossibleInteractables[i];
                go = Instantiate(m_ButtonTemplate.gameObject, m_MenuRect.transform);
                go.GetComponentInChildren<TextMeshProUGUI>().text = m_PossibleInteractables[i].DisplayName;
                go.GetComponentInChildren<Button>().onClick.AddListener(delegate {SpawnPrefab(interactable); });
                go.SetActive(true);
            }
        }

        private void SpawnPrefab(Interactable interactable)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(m_PlacementPosition), out hit, 100))
            {
                GameObject go = Instantiate(interactable.gameObject, hit.point, Quaternion.identity);
            }
            HideMenu();
        }

        void Update()
        {
            if (m_MenuRect.transform.childCount != m_PossibleInteractables.Length)
            {
                CreateMenu();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (GameObject.ReferenceEquals(hit.collider.gameObject, ground))
                    {
                        ShowMenu();
                    } else
                    {
                        HideMenu();
                    }
                }
            }
        }
    }
}
