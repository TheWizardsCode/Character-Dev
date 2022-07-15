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
            StatSO template = ScriptableObject.CreateInstance<StatSO>();
            string statName = "Test Unkown Stat";
            template.name = statName;

            Brain controller = new GameObject().AddComponent<Brain>();

            StatSO stat = controller.GetOrCreateStat(template, 50);

            Assert.True(stat.name == statName, "Did not create the unknown stat.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatInstantInfluencer()
        {
            StatSO template = ScriptableObject.CreateInstance<StatSO>();
            string statName = "Test Immediate Influencer Stat";
            template.name = statName;

            Brain controller = new GameObject().AddComponent<Brain>();

            StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
            StatSO stat = controller.GetOrCreateStat(template);
            influencer.stat = stat;
            influencer.maxChange = 10;
            influencer.duration = 0;

            Assert.True(controller.TryAddInfluencer(influencer), "Was not able to add the Stat influencer");

            yield return new WaitForSeconds(0.51f); // ensure the brain has time to apply the influencer

            Assert.True(stat.NormalizedValue > 0, "Seems the influencer has had no effect.");

            yield return null;
        }
    }
}
