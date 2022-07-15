#if MXM_PRESENT
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

namespace WizardsCode.Character.MxM
{
    public class UiController : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI Canvas to control.")]
        RectTransform m_UI;
        [SerializeField, Tooltip("The Cinemachine virtual camera to use.")]
        CinemachineFreeLook m_Cinemachine;

        private int m_CharacterIndex;
        private List<Transform> m_Characters = new List<Transform>();

        private void Start()
        {
            MxMActorController[] allActors = GameObject.FindObjectsOfType<MxMActorController>();
            for (int i = 0; i < allActors.Length; i++)
            {
                m_Characters.Add(allActors[i].transform);
            }

            m_Characters.Add(GameObject.FindObjectOfType<CharacterController>().transform);

            m_CharacterIndex = m_Characters.Count - 1;
            m_Cinemachine.Follow = m_Characters[m_CharacterIndex];
            m_Cinemachine.LookAt = m_Characters[m_CharacterIndex];
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                m_UI.gameObject.SetActive(!m_UI.gameObject.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                m_CharacterIndex++;
                if (m_CharacterIndex >= m_Characters.Count)
                {
                    m_CharacterIndex = 0;
                }
                m_Cinemachine.Follow = m_Characters[m_CharacterIndex];
                m_Cinemachine.LookAt = m_Characters[m_CharacterIndex];
            }
        }
    }
}
#endif