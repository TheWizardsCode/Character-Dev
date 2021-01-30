using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeDTBD.Navigation
{
    /// <summary>
    /// A PointOfInterest marks a place in a scene that Avatars will be attracted to when nearby.
    /// </summary>
    public class PointOfInterest : MonoBehaviour
    {
        [SerializeField, Tooltip("A list of all related PointsOfInterest. If empty at startup this will be populated with PointsOfInterest in the scene.")]
        List<MonoBehaviour> m_RelatedPointsOfInterest;

        public List<MonoBehaviour> RelatedPointsOfInterest
        {
            get { return m_RelatedPointsOfInterest; }
        }

        private void Awake()
        {
            m_RelatedPointsOfInterest = new List<MonoBehaviour>(GameObject.FindObjectsOfType<PointOfInterest>());
        }
    }
}