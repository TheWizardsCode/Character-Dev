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
    /// serialised field name is the historical <c>value:</c> key (a public field,
    /// matching the "Male UMA with Emotions", "Neo with Emotions" and
    /// "Human-Custom" prefabs) rather than the mangled
    /// <c>&lt;value&gt;k__BackingField</c> that an auto-property backing field would produce.
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
        public void SerialisedEmotionMetricUsesFriendlyValueKey()
        {
            GameObject source = new GameObject("EmotionalState Serialised Object Test");
            EmotionalState state = source.AddComponent<EmotionalState>();
            state.emotions.Add(new EmotionMetric(EmotionType.Interest, 0.4f, 0.45f, 0.2f));

            // Verify the on-disk serialised key for EmotionMetric.value is the
            // historical 'value:' key rather than the mangled
            // '<value>k__BackingField' that an auto-property would produce.
            PrefabUtility.SaveAsPrefabAsset(source, TestPrefabPath);
            Object.DestroyImmediate(source);

            string yaml = System.IO.File.ReadAllText(TestPrefabPath);
            StringAssert.Contains("emotions:", yaml,
                "Prefab must contain the serialised emotions list.");
            StringAssert.Contains("value:", yaml,
                "EmotionMetric.value must be serialised under the friendly key 'value:' " +
                "(historical prefab format, work item NV-0MSG2R1Z7000KGBZ).");
            StringAssert.DoesNotContain("k__BackingField", yaml,
                "The mangled auto-property backing field name must not leak into the serialised YAML.");

            AssetDatabase.DeleteAsset(TestPrefabPath);
        }
    }
}
