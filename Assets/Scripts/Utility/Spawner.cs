using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Editor.Utility
{
    /// <summary>
    /// A really simple spawner that will create a number of a given prefab within a defined area.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to spawn.")]
        GameObject m_Prefab;
        [SerializeField, Tooltip("The number of the prefab to create.")]
        int m_Number = 10;
        [SerializeField, Tooltip("The radius within which to spawn")]
        float m_Radius = 10;

        private void Start()
        {
            for (int i = 0; i < m_Number; i++)
            {
                Vector2 pos2D = Random.insideUnitCircle * m_Radius;
                Vector3 position = transform.position + new Vector3(pos2D.x, 0, pos2D.y);
                Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 359.9f), 0));
                GameObject go = Instantiate(m_Prefab, position, rotation);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, m_Radius);
        }

    }
}
