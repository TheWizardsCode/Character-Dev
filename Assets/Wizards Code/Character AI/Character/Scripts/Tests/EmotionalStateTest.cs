using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// Tests for the emotion → derived value pipeline of <see cref="EmotionalState"/>:
    /// <c>activationValue</c> / <c>enjoymentValue</c> sensitivity, achievable range,
    /// value clamping and <see cref="EmotionalState.Noticeability"/> normalization.
    ///
    /// The five emotions and their multipliers mirror the demo character
    /// (<c>Human-Custom.prefab</c>, work item NV-0MSG2R1Z7000KGBZ). Range semantics
    /// are 0..1 for emotion values with 0 = neutral; derived values are 0..1 with
    /// 0.5 = neutral.
    /// </summary>
    public class EmotionalStateTest
    {
        // Multipliers mirrored from Human-Custom.prefab (rebalanced so that every
        // single-slider change is clearly visible, >= 0.1, and the full 0..1
        // derived range is reachable).
        private static readonly (EmotionType Type, float ActivationMultiplier, float EnjoymentMultiplier)[] Multipliers =
        {
            (EmotionType.Anger,    1f,    -0.2f),
            (EmotionType.Interest, 0.45f,  0.2f),
            (EmotionType.Fear,     0.3f,  -1f),
            (EmotionType.Sadness,  -1f,   -0.1f),
            (EmotionType.Pleasure, 0.3f,   1f),
        };

        private const float Epsilon = 0.001f;

        private EmotionalState m_State;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject("EmotionalState Test");
            m_State = go.AddComponent<EmotionalState>();

            foreach ((EmotionType type, float act, float enj) in Multipliers)
            {
                m_State.emotions.Add(new EmotionMetric(type, 0f, act, enj));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (m_State != null)
            {
                Object.DestroyImmediate(m_State.gameObject);
            }
        }

        [Test]
        public void AllFiveEmotionsArePresent()
        {
            Assert.AreEqual(5, m_State.emotions.Count, "Expected exactly 5 emotion metrics.");

            var types = new HashSet<EmotionType>();
            foreach (EmotionMetric metric in m_State.emotions)
            {
                types.Add(metric.type);
            }

            Assert.IsTrue(types.Contains(EmotionType.Anger), "Missing Anger emotion.");
            Assert.IsTrue(types.Contains(EmotionType.Interest), "Missing Interest emotion.");
            Assert.IsTrue(types.Contains(EmotionType.Fear), "Missing Fear emotion.");
            Assert.IsTrue(types.Contains(EmotionType.Sadness), "Missing Sadness emotion.");
            Assert.IsTrue(types.Contains(EmotionType.Pleasure), "Missing Pleasure emotion.");
        }

        [Test]
        public void NeutralStateIsCentred()
        {
            // All emotions at 0 (neutral) must yield the centre of the derived range.
            Assert.AreEqual(0.5f, m_State.activationValue, Epsilon, "activationValue should be 0.5 when all emotions are 0.");
            Assert.AreEqual(0.5f, m_State.enjoymentValue, Epsilon, "enjoymentValue should be 0.5 when all emotions are 0.");
            Assert.AreEqual(0.5f, m_State.Noticeability, Epsilon, "Noticeability should be 0.5 when all emotions are 0.");
        }

        [Test]
        public void SingleEmotionFromZeroToOneChangesActivationByAtLeastPointOne()
        {
            foreach ((EmotionType type, float activationMultiplier, float _) in Multipliers)
            {
                // Ensure a clean slate first.
                SetAllEmotions(0f);

                float neutral = m_State.activationValue;
                m_State.SetEmotionValue(type, 1f);
                float delta = Mathf.Abs(m_State.activationValue - neutral);

                Assert.GreaterOrEqual(
                    delta, 0.1f,
                    $"Dragging {type} 0→1 changed activationValue by only {delta} (multiplier {activationMultiplier}).");

                // Sanity bound: a single slider should not overshoot more than 2x its multiplier.
                Assert.LessOrEqual(
                    delta, 2f * Mathf.Abs(activationMultiplier) + Epsilon,
                    $"Dragging {type} 0→1 changed activationValue by {delta}, more than 2x its multiplier.");
            }
        }

        [Test]
        public void SingleEmotionFromZeroToOneChangesEnjoymentByAtLeastPointOne()
        {
            foreach ((EmotionType type, float _, float enjoymentMultiplier) in Multipliers)
            {
                SetAllEmotions(0f);

                float neutral = m_State.enjoymentValue;
                m_State.SetEmotionValue(type, 1f);
                float delta = Mathf.Abs(m_State.enjoymentValue - neutral);

                Assert.GreaterOrEqual(
                    delta, 0.1f - Epsilon,
                    $"Dragging {type} 0→1 changed enjoymentValue by only {delta} (multiplier {enjoymentMultiplier}).");

                Assert.LessOrEqual(
                    delta, 2f * Mathf.Abs(enjoymentMultiplier) + Epsilon,
                    $"Dragging {type} 0→1 changed enjoymentValue by {delta}, more than 2x its multiplier.");
            }
        }

        [Test]
        public void FullActivationRangeIsReachable()
        {
            // Maximum activation: all emotions at max (1).
            SetAllEmotions(1f);
            Assert.GreaterOrEqual(
                m_State.activationValue, 0.98f,
                $"Expected near-maximum activationValue with all emotions at max, got {m_State.activationValue}.");

            // Minimum activation: the dominant negative-activation emotion (Sadness) at max.
            SetAllEmotions(0f);
            m_State.SetEmotionValue(EmotionType.Sadness, 1f);
            Assert.LessOrEqual(
                m_State.activationValue, 0.02f,
                $"Expected near-zero activationValue with Sadness at max, got {m_State.activationValue}.");

            // Together these prove the achievable activation span covers at least [0.4, 1.0].
            Assert.GreaterOrEqual(m_State.activationValue, 0f);
        }

        [Test]
        public void NoticeabilitySpansTheMajorityOfUnitRange()
        {
            // Max: angry + interested, not fearful or sad.
            SetAllEmotions(0f);
            m_State.SetEmotionValue(EmotionType.Anger, 1f);
            m_State.SetEmotionValue(EmotionType.Interest, 1f);
            Assert.Greater(m_State.Noticeability, 0.8f,
                $"Noticeability maximum should exceed 0.8, got {m_State.Noticeability}.");

            // Min: fearful + sad, not angry or interested.
            SetAllEmotions(0f);
            m_State.SetEmotionValue(EmotionType.Fear, 1f);
            m_State.SetEmotionValue(EmotionType.Sadness, 1f);
            Assert.Less(m_State.Noticeability, 0.2f,
                $"Noticeability minimum should be below 0.2, got {m_State.Noticeability}.");
        }

        [Test]
        public void EmotionValuesAreClampedToUnitRange()
        {
            m_State.SetEmotionValue(EmotionType.Anger, 1.5f);
            Assert.AreEqual(1f, m_State.GetEmotionValue(EmotionType.Anger), Epsilon,
                "Emotion values above 1 must be clamped to 1.");

            m_State.SetEmotionValue(EmotionType.Anger, -0.5f);
            Assert.AreEqual(0f, m_State.GetEmotionValue(EmotionType.Anger), Epsilon,
                "Emotion values below 0 must be clamped to 0.");
        }

        [Test]
        public void SetEmotionValueReplacesRatherThanAccumulates()
        {
            m_State.SetEmotionValue(EmotionType.Pleasure, 0.3f);
            m_State.SetEmotionValue(EmotionType.Pleasure, 0.6f);

            Assert.AreEqual(0.6f, m_State.GetEmotionValue(EmotionType.Pleasure), Epsilon,
                "SetEmotionValue must replace the current value, not add to it.");
        }

        private void SetAllEmotions(float value)
        {
            foreach ((EmotionType type, _, _) in Multipliers)
            {
                m_State.SetEmotionValue(type, value);
            }
        }
    }
}
