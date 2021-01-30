using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WizardsCode.Stats;

namespace WizardsCode.Stats
{
    public class StatsControllerTest
    {
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StatsControllerAddUnknownStat()
        {
            string statName = "Test Unkown Stat";
            StatsController controller = new GameObject().AddComponent<StatsController>();

            StatSO stat = controller.GetOrCreateStat(statName, 50);

            Assert.True(stat.name == statName, "Did not create the unknown stat.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatInstantInfluencer()
        {
            string statName = "Test Immediate Influencer Stat";
            StatsController controller = new GameObject().AddComponent<StatsController>();

            StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
            StatSO stat = controller.GetOrCreateStat(statName);
            influencer.stat = stat;
            influencer.maxChange = 10;
            influencer.duration = 0;

            Assert.True(controller.TryAddInfluencer(influencer), "Was not able to add the Stat influencer");

            yield return null;

            Assert.True(controller.GetOrCreateStat(statName).normalizedValue > 0, "Seems the influencer has had no effect.");

            yield return null;
        }
    }
}
