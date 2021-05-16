using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WizardsCode.Character.MxM
{
    public class UiController : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI Canvas to control.")]
        RectTransform m_UI;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                m_UI.gameObject.SetActive(!m_UI.gameObject.activeSelf);
            }
        }
    }
}
