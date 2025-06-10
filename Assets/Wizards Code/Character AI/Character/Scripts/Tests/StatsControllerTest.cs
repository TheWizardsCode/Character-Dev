using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WizardsCode.Character;
using WizardsCode.Stats;

namespace WizardsCode.AI.Tests
{
    public class StatsControllerTest
    {
        private Brain brain;
        private GameObject testGameObject;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with required components
            testGameObject = new GameObject("TestCharacter");
            testGameObject.AddComponent<Rigidbody>();
            testGameObject.AddComponent<CapsuleCollider>();
            testGameObject.AddComponent<BaseActorController>();

            // Add Brain component
            brain = testGameObject.AddComponent<Brain>();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StatsControllerAddUnknownStat()
        {
            StatSO template = ScriptableObject.CreateInstance<StatSO>();
            string statName = "Test Unknown Stat";
            template.name = statName;

            StatSO stat = brain.GetOrCreateStat(template, 50);

            Assert.True(stat.name == statName, "Did not create the unknown stat.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatInstantInfluencer()
        {
            StatSO template = ScriptableObject.CreateInstance<StatSO>();
            string statName = "Test Immediate Influencer Stat";
            template.name = statName;

            StatInfluencerSO influencer = ScriptableObject.CreateInstance<StatInfluencerSO>();
            StatSO stat = brain.GetOrCreateStat(template);
            influencer.stat = stat;
            influencer.maxChange = 10;
            influencer.duration = 0;

            Assert.True(brain.TryAddInfluencer(influencer), "Was not able to add the Stat influencer");

            yield return new WaitForSeconds(0.51f); // ensure the brain has time to apply the influencer

            Assert.True(stat.NormalizedValue > 0, "Seems the influencer has had no effect.");

            yield return null;
        }
    }
}
