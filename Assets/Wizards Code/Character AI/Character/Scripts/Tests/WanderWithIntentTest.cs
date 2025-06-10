using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;
using WizardsCode.Character;
using WizardsCode.Stats;
using Object = UnityEngine.Object;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// Comprehensive tests for the WanderWithIntent class covering:
    /// - Cooldown mechanisms
    /// - Memory-based navigation
    /// - Stat evaluation logic
    /// - Integration with Brain and MemoryController
    /// - Edge cases and error conditions
    /// </summary>
    public class WanderWithIntentTest
    {
        private GameObject m_TestGameObject;
        private WanderWithIntent m_WanderWithIntent;
        private Brain m_Brain;
        private StateSO unsatisfiedDesiredState;
        private MemoryController m_MemoryController;
        private StatSO m_TestStatTemplate;
        private MemorySO m_TestMemory;
        private GameObject m_TargetObject;

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with required components
            m_TestGameObject = new GameObject("TestCharacter");
            m_TestGameObject.AddComponent<Rigidbody>();
            m_TestGameObject.AddComponent<CapsuleCollider>();
            m_TestGameObject.AddComponent<BaseActorController>();

            // Add Brain with memory 
            m_MemoryController = m_TestGameObject.AddComponent<MemoryController>();
            m_Brain = m_TestGameObject.AddComponent<Brain>();
            
            // Create test stat template
            m_TestStatTemplate = ScriptableObject.CreateInstance<StatSO>();
            m_TestStatTemplate.DisplayName = "TestStat";
            m_TestStatTemplate.MaxValue = 100f;
            m_TestStatTemplate.MinValue = 0f;
            m_TestStatTemplate.Value = 50f;

            // Create a desired state that will be unsatisfied
            unsatisfiedDesiredState = ScriptableObject.CreateInstance<StateSO>();
            unsatisfiedDesiredState.name = "TestUnsatisfiedDesiredState";
            unsatisfiedDesiredState.statTemplate = m_TestStatTemplate;
            unsatisfiedDesiredState.normalizedTargetValue = 0.8f;
            unsatisfiedDesiredState.objective = StateSO.Objective.GreaterThan;
            
            // create a state from the template, add it to the desired state, ensure that it is not satisfied
            StatSO testStat = m_Brain.GetOrCreateStat(m_TestStatTemplate, 0.3f);
            
            // Add the desired state to brain
            var desiredStatesField = typeof(StatsTracker).GetField("m_DesiredStates", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            desiredStatesField.SetValue(m_Brain, new StateSO[] { unsatisfiedDesiredState });

            // Add memories that influences this stat positively
            // Create test memory of a target object that increases the stat
            m_TargetObject = new GameObject("TargetObject0 - increases stat");
            m_TargetObject.transform.position = new Vector3(10f, 0f, 10f);
            m_TestMemory = ScriptableObject.CreateInstance<MemorySO>();
            m_TestMemory.about = m_TargetObject;
            m_TestMemory.influence = 10f;

            m_TestMemory.stat = testStat;
            m_TestMemory.influence = 15f;
            m_MemoryController.AddMemory(m_TestMemory);

            Transform target1 = new GameObject("TargetObject1 - increases stat").transform;
            target1.position = new Vector3(15f, 0f, 1f);
            target1.gameObject.AddComponent<BoxCollider>();
            MemorySO memory1 = ScriptableObject.CreateInstance<MemorySO>();
            memory1.about = target1.gameObject;
            memory1.stat = testStat;
            memory1.influence = 10f;
            memory1.time = Time.time - memory1.cooldown;
            m_MemoryController.AddMemory(memory1);
            
            // Another memory with a different target, increases the stat
            GameObject targetObject2 = new GameObject("TargetObject2 - increases stat");
            targetObject2.transform.position = new Vector3(10f, 0f, 0f);
            targetObject2.AddComponent<BoxCollider>();
            
            MemorySO memory2 = ScriptableObject.CreateInstance<MemorySO>();
            memory2.about = targetObject2;
            memory2.stat = testStat;
            memory2.influence = 12f;
            memory2.time = Time.time - memory2.cooldown;
            m_MemoryController.AddMemory(memory2);

            // Another memory this time with negative influence
            GameObject targetObject3 = new GameObject("TargetObject3 - decreases stat");
            targetObject3.transform.position = new Vector3(1f, 0f, 0f);
            targetObject3.AddComponent<BoxCollider>();
            
            MemorySO memory3 = ScriptableObject.CreateInstance<MemorySO>();
            memory3.about = targetObject3;
            memory3.stat = testStat;
            memory3.influence = -12f;
            memory3.time = Time.time - memory3.cooldown;
            m_MemoryController.AddMemory(memory3);


            // Add WanderWithIntent component
            m_WanderWithIntent = m_TestGameObject.AddComponent<WanderWithIntent>();

            // Create target object for memory tests
            m_TargetObject = new GameObject("TargetObject");
            m_TargetObject.AddComponent<BoxCollider>();
            m_TargetObject.transform.position = new Vector3(10f, 0f, 10f);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TestGameObject != null)
                Object.DestroyImmediate(m_TestGameObject);

            if (m_TargetObject != null)
                Object.DestroyImmediate(m_TargetObject);

            if (m_TestStatTemplate != null)
                Object.DestroyImmediate(m_TestStatTemplate);

            if (m_TestMemory != null)
                Object.DestroyImmediate(m_TestMemory);

            if (unsatisfiedDesiredState != null)
                Object.DestroyImmediate(unsatisfiedDesiredState);

            if (m_Brain != null)
                Object.DestroyImmediate(m_Brain);

            if (m_MemoryController != null)
                Object.DestroyImmediate(m_MemoryController);

            if (m_WanderWithIntent != null)
                Object.DestroyImmediate(m_WanderWithIntent);
        }

        #endregion

        #region Initialization Tests

        [UnityTest]
        public IEnumerator WanderWithIntent_Initialization_ComponentsSetCorrectly()
        {
            yield return null; // Wait one frame for initialization
            
            Assert.IsNotNull(m_WanderWithIntent, "WanderWithIntent component should not be null");
            Assert.IsNotNull(m_Brain, "Brain component should not be null");
            Assert.IsNotNull(m_MemoryController, "MemoryController component should not be null");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_Initialization_DefaultCooldownValues()
        {
            yield return null; // Wait one frame for initialization
            
            // Use reflection to check private fields since they're serialized
            var directionCooldownField = typeof(WanderWithIntent).GetField("m_DirectionChangeCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var turningCooldownField = typeof(WanderWithIntent).GetField("m_TurningModeCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var memoryCooldownField = typeof(WanderWithIntent).GetField("m_MemoryTargetCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var statCooldownField = typeof(WanderWithIntent).GetField("m_StatEvaluationCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            Assert.AreEqual(1f, directionCooldownField.GetValue(m_WanderWithIntent), "Default direction change cooldown should be 1 second");
            Assert.AreEqual(1f, turningCooldownField.GetValue(m_WanderWithIntent), "Default turning mode cooldown should be 1 second");
            Assert.AreEqual(1f, memoryCooldownField.GetValue(m_WanderWithIntent), "Default memory target cooldown should be 1 second");
            Assert.AreEqual(1f, statCooldownField.GetValue(m_WanderWithIntent), "Default stat evaluation cooldown should be 1 second");
        }

        #endregion

        #region Cooldown Mechanism Tests

        [UnityTest]
        public IEnumerator WanderWithIntent_StatEvaluationCooldown_PreventsTooFrequentEvaluation()
        {
            float longCooldown = 2f;

            // Set a longer cooldown for testing
            var statCooldownField = typeof(WanderWithIntent).GetField("m_StatEvaluationCooldown", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            statCooldownField.SetValue(m_WanderWithIntent, longCooldown);
            
            // Access private cooldown check method
            var canEvaluateStatsMethod = typeof(WanderWithIntent).GetMethod("CanEvaluateStats", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            yield return new WaitForSeconds(longCooldown * 1.01f); // Ensure cooldown is reset
            
            bool canEvaluate1 = (bool)canEvaluateStatsMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canEvaluate1, "Stat evaluation should be allowed after cooldown expires");

            // Update the timestamp
            var updateMethod = typeof(WanderWithIntent).GetMethod("UpdateStatEvaluationTime", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMethod.Invoke(m_WanderWithIntent, null);
            
            // Immediate second call should be blocked
            bool canEvaluate2 = (bool)canEvaluateStatsMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsFalse(canEvaluate2, "Immediate second stat evaluation should be blocked by cooldown");
            
            // Wait for cooldown to expire
            yield return new WaitForSeconds(longCooldown * 1.01f);
            
            // Should be allowed again after cooldown
            bool canEvaluate3 = (bool)canEvaluateStatsMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canEvaluate3, "Stat evaluation should be allowed after cooldown expires");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_MemoryTargetCooldown_PreventsRapidTargetSwitching()
        {
            var memoryCooldownField = typeof(WanderWithIntent).GetField("m_MemoryTargetCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            memoryCooldownField.SetValue(m_WanderWithIntent, 1.5f);
            
            var canChangeMemoryTargetMethod = typeof(WanderWithIntent).GetMethod("CanChangeMemoryTarget", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // First change should be allowed
            bool canChange1 = (bool)canChangeMemoryTargetMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canChange1, "First memory target change should be allowed");
            
            // Update timestamp
            var updateMethod = typeof(WanderWithIntent).GetMethod("UpdateMemoryTargetChangeTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMethod.Invoke(m_WanderWithIntent, null);
            
            // Immediate change should be blocked
            bool canChange2 = (bool)canChangeMemoryTargetMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsFalse(canChange2, "Immediate memory target change should be blocked by cooldown");
            
            yield return new WaitForSeconds(1.6f);
            
            // Should be allowed after cooldown
            bool canChange3 = (bool)canChangeMemoryTargetMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canChange3, "Memory target change should be allowed after cooldown expires");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_DirectionChangeCooldown_PreventsOscillation()
        {
            var directionCooldownField = typeof(WanderWithIntent).GetField("m_DirectionChangeCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            directionCooldownField.SetValue(m_WanderWithIntent, 1f);
            
            var canChangeDirectionMethod = typeof(WanderWithIntent).GetMethod("CanChangeDirection", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool canChange1 = (bool)canChangeDirectionMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canChange1, "First direction change should be allowed");
            
            var updateMethod = typeof(WanderWithIntent).GetMethod("UpdateDirectionChangeTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMethod.Invoke(m_WanderWithIntent, null);
            
            bool canChange2 = (bool)canChangeDirectionMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsFalse(canChange2, "Immediate direction change should be blocked");
            
            yield return new WaitForSeconds(1.1f);
            
            bool canChange3 = (bool)canChangeDirectionMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canChange3, "Direction change should be allowed after cooldown");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_TurningModeCooldown_PreventsRapidTurning()
        {
            var turningCooldownField = typeof(WanderWithIntent).GetField("m_TurningModeCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            turningCooldownField.SetValue(m_WanderWithIntent, 0.8f);
            
            var canEnterTurningModeMethod = typeof(WanderWithIntent).GetMethod("CanEnterTurningMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool canEnter1 = (bool)canEnterTurningModeMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canEnter1, "First turning mode entry should be allowed");
            
            var updateMethod = typeof(WanderWithIntent).GetMethod("UpdateTurningModeTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMethod.Invoke(m_WanderWithIntent, null);
            
            bool canEnter2 = (bool)canEnterTurningModeMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsFalse(canEnter2, "Immediate turning mode re-entry should be blocked");
            
            yield return new WaitForSeconds(0.9f);
            
            bool canEnter3 = (bool)canEnterTurningModeMethod.Invoke(m_WanderWithIntent, null);
            Assert.IsTrue(canEnter3, "Turning mode should be allowed after cooldown");
        }

        #endregion

        #region Memory-Based Navigation Tests

        [UnityTest]
        public IEnumerator WanderWithIntent_MemoryNavigation_MovesTowardsTargetWhenStatUnsatisfied()
        {
            // Trigger UpdateMove
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove",
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            
            yield return null; // Wait for one frame to allow movement logic to execute
            
            // Check if the character is moving towards the target
            Vector3 currentTarget = m_WanderWithIntent.currentTarget;

            // The target should be near the memory object
            float distance = Vector3.Distance(currentTarget, m_TargetObject.transform.position);
            Assert.IsTrue(distance < 20f, "Character should be targeting a position near the memory object");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_MemoryNavigation_IgnoresNegativeInfluenceWhenTryingToIncrease()
        {
            yield return null;
            
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            
            // Check that no memory of interest was set (should fall back to base wander)
            var nearestMemoryField = typeof(WanderWithIntent).GetField("nearestMemoryOfInterest", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            MemorySO nearestMemory = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            // Ensure that the targeted memory is not the one with negative influence
            Assert.IsFalse(nearestMemory == null || nearestMemory.influence < 0, "Should not target memory with negative influence when trying to increase stat");
        }
        #endregion

        #region Edge Cases and Error Conditions

        [UnityTest]
        public IEnumerator WanderWithIntent_EdgeCase_NoUnsatisfiedStats()
        {
            // Ensure no desired states are unsatisfied
            var desiredStatesField = GetFieldFromHierarchy(typeof(Brain), "m_DesiredStates", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            desiredStatesField.SetValue(m_Brain, new StateSO[0]);
            
            yield return null;
            
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            
            // Should fall back to base wander behavior
            var nearestMemoryField = typeof(WanderWithIntent).GetField("nearestMemoryOfInterest", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            MemorySO nearestMemory = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            Assert.IsNull(nearestMemory, "Should have no memory target when no stats are unsatisfied");
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_EdgeCase_MemoryWithoutCollider()
        {
            // Create target object without collider
            GameObject targetWithoutCollider = new GameObject("TargetWithoutCollider");
            targetWithoutCollider.transform.position = new Vector3(10f, 0f, 10f);
            
            StateSO desiredState = ScriptableObject.CreateInstance<StateSO>();
            desiredState.statTemplate = m_TestStatTemplate;
            desiredState.normalizedTargetValue = 0.8f;
            desiredState.objective = StateSO.Objective.GreaterThan;

            var desiredStatesField = GetFieldFromHierarchy(typeof(Brain), "m_DesiredStates", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            desiredStatesField.SetValue(m_Brain, new StateSO[] { desiredState });
            
            StatSO testStat = m_Brain.GetOrCreateStat(m_TestStatTemplate, 30f);
            
            MemorySO memoryWithoutCollider = ScriptableObject.CreateInstance<MemorySO>();
            memoryWithoutCollider.about = targetWithoutCollider;
            memoryWithoutCollider.stat = testStat;
            memoryWithoutCollider.influence = 15f;
            memoryWithoutCollider.time = Time.time - memoryWithoutCollider.cooldown;

            m_MemoryController.AddMemory(memoryWithoutCollider);

            yield return null;

            // This should not throw an exception
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.DoesNotThrow(() => updateMoveMethod.Invoke(m_WanderWithIntent, null), 
                "Should handle memories without colliders gracefully");
            
            // Cleanup
            Object.DestroyImmediate(targetWithoutCollider);
            Object.DestroyImmediate(memoryWithoutCollider);
            Object.DestroyImmediate(desiredState);
        }

        [UnityTest]
        public IEnumerator WanderWithIntent_EdgeCase_CharacterInsideTargetCollider()
        {
            // Position character inside the target collider
            m_TestGameObject.transform.position = m_TargetObject.transform.position;
            
            StateSO desiredState = ScriptableObject.CreateInstance<StateSO>();
            desiredState.statTemplate = m_TestStatTemplate;
            desiredState.normalizedTargetValue = 0.8f;
            desiredState.objective = StateSO.Objective.GreaterThan;
            
            var desiredStatesField = GetFieldFromHierarchy(typeof(Brain), "m_DesiredStates", 
                BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var desiredStatesList = new StateSO[] { desiredState };
            desiredStatesField.SetValue(m_Brain, desiredStatesList);

            StatSO testStat = m_Brain.GetOrCreateStat(m_TestStatTemplate, 30f);
            
            m_TestMemory.stat = testStat;
            m_TestMemory.influence = 15f;
            
            m_MemoryController.AddMemory(m_TestMemory);
            
            yield return null;
            
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            
            // Should not target a memory when already inside its bounds
            var nearestMemoryField = typeof(WanderWithIntent).GetField("nearestMemoryOfInterest", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            MemorySO nearestMemory = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            Assert.IsNull(nearestMemory, "Should not target memory when character is already inside its bounds");
            
            Object.DestroyImmediate(desiredState);
        }

        #endregion

        public static FieldInfo GetFieldFromHierarchy(Type type, string fieldName, BindingFlags flags)
        {
            FieldInfo field = null;
            Type currentType = type;
            
            while (currentType != null && field == null)
            {
                field = currentType.GetField(fieldName, flags | BindingFlags.DeclaredOnly);
                currentType = currentType.BaseType;
            }
            
            return field;
        }

        #region Integration Tests

        [UnityTest]
        public IEnumerator WanderWithIntent_Integration_CooldownInteractionWithMemoryTargeting()
        {
            yield return null; // Wait one frame for initialization of the character and brain

            // Set up a scenario where cooldowns interact with memory targeting
            var memoryCooldownField = typeof(WanderWithIntent).GetField("m_MemoryTargetCooldown", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            memoryCooldownField.SetValue(m_WanderWithIntent, 1f);
            
            yield return null;
            
            // First update should target the closer memory
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove",
                BindingFlags.NonPublic | BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);

            var nearestMemoryField = typeof(WanderWithIntent).GetField("nearestMemoryOfInterest",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MemorySO firstTarget = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            Assert.IsNotNull(firstTarget, "Should have selected a memory target");
            
            // Immediate second update should maintain the same target due to cooldown
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            MemorySO secondTarget = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            Assert.AreEqual(firstTarget, secondTarget, "Should maintain same target due to cooldown");
            
            // Wait for cooldown to expire
            yield return new WaitForSeconds(1.1f);
            
            // Now it should be able to switch targets if beneficial
            updateMoveMethod.Invoke(m_WanderWithIntent, null);

            yield return null;

            MemorySO thirdTarget = (MemorySO)nearestMemoryField.GetValue(m_WanderWithIntent);
            
            Assert.IsNotNull(thirdTarget, "Should still have a memory target after cooldown");
        }

        #endregion

        #region Status Text Tests

        [UnityTest]
        public IEnumerator WanderWithIntent_StatusText_ReflectsCurrentIntent()
        {
            yield return null; // Wait one frame for initialization of the character and brain
            
            var updateMoveMethod = typeof(WanderWithIntent).GetMethod("UpdateMove", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMoveMethod.Invoke(m_WanderWithIntent, null);
            
            string statusText = m_WanderWithIntent.StatusText();
            
            Assert.IsTrue(statusText.Contains("Intent"), "Status text should contain intent information");
            Assert.IsTrue(statusText.Contains(m_TargetObject.name), "Status text should contain target object name");
        }

        #endregion
    }
}
