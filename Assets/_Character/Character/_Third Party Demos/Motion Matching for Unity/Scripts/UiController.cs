using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using WizardsCode.Stats;

namespace WizardsCode.Character.MxM
{
    public class UiController : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI Canvas to control.")]
        RectTransform m_UI;
        [SerializeField, Tooltip("The Cinemachine virtual camera to use.")]
        CinemachineFreeLook m_Cinemachine;
        [SerializeField, Tooltip("The health stat template used to track character health.")]
        StatSO m_HealthStatTemplate;

        private int m_CharacterIndex;
        private List<StatsTracker> m_Characters = new List<StatsTracker>();

        private void Start()
        {
            m_UI.gameObject.SetActive(true);
            StatsTracker[] allActors = GameObject.FindObjectsOfType<StatsTracker>();
            for (int i = 0; i < allActors.Length; i++)
            {
                m_Characters.Add(allActors[i]);
            }

            m_CharacterIndex = m_Characters.Count - 1;
            m_Cinemachine.Follow = m_Characters[m_CharacterIndex].transform.root;
            m_Cinemachine.LookAt = m_Characters[m_CharacterIndex].transform.root;
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
                m_Cinemachine.Follow = m_Characters[m_CharacterIndex].transform.root;
                m_Cinemachine.LookAt = m_Characters[m_CharacterIndex].transform.root;
                Debug.Log("Target changed to " + m_Characters[m_CharacterIndex].transform.root.name);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                StatSO health = m_Characters[m_CharacterIndex].GetOrCreateStat(m_HealthStatTemplate);
                health.Value = 0;
            }
        }
    }
}
