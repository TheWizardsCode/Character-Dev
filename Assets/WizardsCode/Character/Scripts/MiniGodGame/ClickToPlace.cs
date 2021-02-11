using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character.MiniGodGame
{
    public class ClickToPlace : MonoBehaviour
    {
        [SerializeField, Tooltip("Surface to place objects onto.")]
        GameObject ground;
        [SerializeField, Tooltip("The prefab to place when the left mouse is clicked")]
        GameObject prefabToPlace;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (GameObject.ReferenceEquals(hit.collider.gameObject, ground)) {
                        GameObject go = Instantiate(prefabToPlace, hit.point, Quaternion.identity);
                    }
                }
            }
        }
    }
}
