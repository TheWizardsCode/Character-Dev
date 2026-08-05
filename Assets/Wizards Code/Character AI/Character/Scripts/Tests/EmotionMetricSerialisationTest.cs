using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// Editor tests verifying that <see cref="EmotionMetric.value"/> persists
    /// through Unity serialisation round-trips (prefab save → reload).
    ///
    /// Rationale (work item NV-0MSG2R1Z7000KGBZ): emotion values must persist so
    /// that editor/prefab configured emotion values survive play sessions. The
    /// serialised field name is <c>value</c> (auto-property with
    /// <c>[field: SerializeField]</c>), which also matches the historical
    /// <c>value:</c> keys present in the "Male UMA with Emotions" and
    /// "Neo with Emotions" prefabs.
    /// </summary>
    public class EmotionMetricSerialisationTest
    {
        private const string TestPrefabPath = "Assets/__EmotionalStateSerialisationTest.prefab";

        [UnityTest]
        public IEnumerator EmotionValuesSurvivePrefabRoundTrip()
        {
            GameObject source = new GameObject("EmotionalState Serialisation Test");
            EmotionalState state = source.AddComponent<EmotionalState>();
            state.emotions.Add(new EmotionMetric(EmotionType.Anger, 0.7f, 1f, -0.2f));

            PrefabUtility.SaveAsPrefabAsset(source, TestPrefabPath);
            Object.DestroyImmediate(source);
            yield return null;

            GameObject loaded = AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
            Assert.IsNotNull(loaded, "Failed to reload the test prefab.");

            EmotionalState loadedState = loaded.GetComponent<EmotionalState>();
            Assert.IsNotNull(loadedState, "Reloaded prefab lost its EmotionalState component.");
            Assert.AreEqual(1, loadedState.emotions.Count);
            Assert.AreEqual(0.7f, loadedState.emotions[0].value, 0.001f,
                "EmotionMetric.value was not preserved through serialisation.");

            AssetDatabase.DeleteAsset(TestPrefabPath);
        }

        [Test]
        public void SerializedObjectExposesValuePropertyOnEmotionMetric()
        {
            GameObject source = new GameObject("EmotionalState Serialised Object Test");
            EmotionalState state = source.AddComponent<EmotionalState>();
            state.emotions.Add(new EmotionMetric(EmotionType.Interest, 0.4f, 0.45f, 0.2f));

            // Inspect the serialised representation via the prefab round-trip so
            // the property path matches what Unity writes to disk for the
            // auto-property backing field ([field: SerializeField] => key 'value').
            PrefabUtility.SaveAsPrefabAsset(source, TestPrefabPath);
            Object.DestroyImmediate(source);

            GameObject loaded = AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
            SerializedObject serializedObject = new SerializedObject(loaded);
            SerializedProperty valueProperty =
                serializedObject.FindProperty("emotions.Array.data[0].value");

            Assert.IsNotNull(valueProperty,
                "Expected a serialised 'value' property on EmotionMetric. " +
                "If EmotionMetric.value is intentionally excluded from persistence this test documents that decision explicitly.");
            Assert.AreEqual(0.4f, valueProperty.floatValue, 0.001f);

            AssetDatabase.DeleteAsset(TestPrefabPath);
            Object.DestroyImmediate(loaded);
        }
    }
}
