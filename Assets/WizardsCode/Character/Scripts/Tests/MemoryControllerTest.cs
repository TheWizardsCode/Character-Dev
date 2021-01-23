using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WizardsCode.Character;
using WizardsCode.Character.Stats;
using WizardsCode.Stats;

namespace WizardsCode.Personality.Tests
{
    public class MemoryControllerTest
    {
        MemoryController controller;
        
        GameObject shortTermInfluencer, shortTermNegativeInfluencer;
        MemorySO shortTermMemory, shortTermMemoryNegativeInflunce;

        GameObject longTermInfluencer;
        MemorySO longTermMemory;

        private void SetupMemory()
        {
            GameObject go = new GameObject("Memory");
            controller = new GameObject().AddComponent<MemoryController>();

            shortTermInfluencer = new GameObject("ShortTermInfluencer");
            shortTermNegativeInfluencer = new GameObject("ShortTermInfluencer1");

            StatSO shortTermTestStat = ScriptableObject.CreateInstance<StatSO>();
            shortTermTestStat.name = "ShortTermTest";

            shortTermMemory = ScriptableObject.CreateInstance<MemorySO>();
            shortTermMemory.about = shortTermInfluencer;
            shortTermMemory.stat = shortTermTestStat;
            shortTermMemory.influence = 5;
            shortTermMemory.cooldown = 0.1f;

            StatSO shortTermNegativeInfluenceTestStat = ScriptableObject.CreateInstance<StatSO>();
            shortTermNegativeInfluenceTestStat.name = "ShortTermNegativeInfluence";

            shortTermMemoryNegativeInflunce = ScriptableObject.CreateInstance<MemorySO>();
            shortTermMemoryNegativeInflunce.about = shortTermNegativeInfluencer;
            shortTermMemoryNegativeInflunce.stat = shortTermNegativeInfluenceTestStat;
            shortTermMemoryNegativeInflunce.influence = -5;

            longTermInfluencer = new GameObject("LongTermInfluencer");

            StatSO longTermTestStat = ScriptableObject.CreateInstance<StatSO>();
            longTermTestStat.name = "LongTermTest";

            longTermMemory = ScriptableObject.CreateInstance<MemorySO>();
            longTermMemory.about = longTermInfluencer;
            longTermMemory.stat = longTermTestStat;
            longTermMemory.influence = 50;
            longTermMemory.cooldown = 0;

            // Validate setup
            Assert.Zero(controller.GetShortTermMemories().Length, "There are short term memories in a new Memory Controller");
            Assert.Zero(controller.GetLongTermMemories().Length, "There are long term memories in a new Memory Controller");
        }

        [UnityTest]
        public IEnumerator ShortTermMemoryCommit()
        {
            SetupMemory();

            // Add the three short term memories destined to stay in short term
            controller.AddMemory(shortTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemories().Length, "The first short term memory item was not committed to memory.");
            Assert.AreEqual(5, controller.GetShortTermMemoriesAbout(shortTermInfluencer)[0].influence);
            
            controller.AddMemory(shortTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemories().Length, "The second short term memory item should not have been committed as it is a duplicate of an existing short term memory.");

            controller.AddMemory(shortTermMemoryNegativeInflunce);
            Assert.AreEqual(2, controller.GetShortTermMemories().Length, "The short term memory item 1 should have been committed.");
            Assert.AreEqual(1, controller.GetShortTermMemoriesAbout(shortTermInfluencer).Length, "The first short term memory item was not committed to memory.");
            Assert.AreEqual(1, controller.GetShortTermMemoriesAbout(shortTermNegativeInfluencer).Length, "The first short term 1 memory item was not committed to memory.");
            Assert.AreEqual(-5, controller.GetShortTermMemoriesAbout(shortTermNegativeInfluencer)[0].influence);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ShortTermMemoryCommitWithCooldown()
        {
            SetupMemory();

            // Add the three short term memories destined to stay in short term
            controller.AddMemory(shortTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemories().Length, "The first short term memory item was not committed to memory.");

            controller.AddMemory(shortTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemories().Length, "The second short term memory item should not have been committed as it is a duplicate of an existing short term memory.");
            Assert.AreEqual(10, controller.GetSimilarShortTermMemory(shortTermMemory).influence);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LongTermMemoryCommit()
        {
            SetupMemory();

            // Add the first memory destined for long term memory, should go into short term
            controller.AddMemory(longTermMemory);
            Assert.NotZero(controller.GetShortTermMemoriesAbout(longTermInfluencer).Length, "There are no short term memories even after adding a memory");
            Assert.Zero(controller.GetLongTermMemories().Length, "There are long term memories in a new Memory Controller");

            // Add the second memory destined for long term memory, should go into short term
            controller.AddMemory(longTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemoriesAbout(longTermInfluencer).Length);
            Assert.Zero(controller.GetLongTermMemories().Length, "There still shouldn't be any long term memories after adding three short terms.");

            // Add one more short term memory, this should push the two destined for long term into long term to make space for the short term
            controller.AddMemory(shortTermMemory);
            Assert.AreEqual(1, controller.GetShortTermMemoriesAbout(shortTermInfluencer).Length, "There should be 3 memories about " + shortTermInfluencer.name + " short term items in short term memory at this point");
            Assert.AreEqual(1, controller.GetLongTermMemoriesAbout(longTermInfluencer).Length, "There should now be a long term memory.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Cooldown()
        {
            SetupMemory();

            controller.AddMemory(shortTermMemory);
            MemorySO[] about = controller.GetShortTermMemoriesAbout(shortTermMemory.about);
            Assert.False(about[0].readyToReturn);
            yield return new WaitForSeconds(0.11f);
            about = controller.GetShortTermMemoriesAbout(shortTermMemory.about);
            Assert.True(about[0].readyToReturn);

            yield return null;
        }
    }
}
