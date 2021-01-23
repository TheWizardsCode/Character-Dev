using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    /// <summary>
    /// Provides some Editor only features for working with stats in the development environment.
    /// </summary>
    public class StatInfluencerController : MonoBehaviour
    {
        public StatInfluencerSO influencer;

        /// <summary>
        /// If the currently selected object has a StatsController attached then apply\
        /// the indicated influencer.
        /// </summary>
        public void ApplyInfluencerToSelectedObject()
        {
            Transform selected = Selection.activeTransform;
            StatsController stats = selected.GetComponent<StatsController>();
            if (stats)
            {
                stats.TryAddInfluencer(influencer);
            }
        }
    }
}
