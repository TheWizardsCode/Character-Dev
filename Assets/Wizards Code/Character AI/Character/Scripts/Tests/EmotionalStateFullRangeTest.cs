using NUnit.Framework;
using UnityEngine;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// Full-range sweep tests for the emotion → derived value pipeline of
    /// <see cref="EmotionalState"/> (work item NV-0MSGM4XQP007V6UM).
    ///
    /// Where <see cref="EmotionalStateTest"/> verifies the endpoints (0 and 1),
    /// these tests drag each emotion slider through every intermediate value and
    /// verify the derived values (<c>activationValue</c> / <c>enjoymentValue</c>)
    /// respond monotonically in the direction of the multiplier, never overshoot
    /// past the endpoint (with the linear aggregation each axis is a straight
    /// line, so every intermediate point lies between the neutral value and the
    /// endpoint), and stay clamped in [0, 1].
    ///
    /// The five emotions and their multipliers mirror the demo character
    /// (<c>Human-Custom.prefab</c>, work item NV-0MSG2R1Z7000KGBZ).
    /// </summary>
    public class EmotionalStateFullRangeTest
    {
        // Multipliers mirrored from Human-Custom.prefab (see EmotionalStateTest).
        private static readonly (EmotionType Type, float ActivationMultiplier, float EnjoymentMultiplier)[] Multipliers =
        {
            (EmotionType.Anger,    1f,    -0.2f),
            (EmotionType.Interest, 0.45f,  0.2f),
            (EmotionType.Fear,     0.3f,  -1f),
            (EmotionType.Sadness,  -1f,   -0.1f),
            (EmotionType.Pleasure, 0.3f,   1f),
        };

        // Slider positions covering the full 0..1 range, including intermediates.
        private static readonly float[] Sweep = { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 0.9f, 1f };

        private const float Epsilon = 0.001f;

        private EmotionalState m_State;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject("EmotionalState Full Range Test");
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
        public void SweepingEachEmotionAcrossFullRange_MovesDerivedValuesMonotonically()
        {
            foreach ((EmotionType type, float activationMultiplier, float enjoymentMultiplier) in Multipliers)
            {
                SetAllEmotions(0f);
                float neutralActivation = m_State.activationValue;
                float neutralEnjoyment = m_State.enjoymentValue;

                float prevActivation = neutralActivation;
                float prevEnjoyment = neutralEnjoyment;

                foreach (float v in Sweep)
                {
                    m_State.SetEmotionValue(type, v);

                    float activation = m_State.activationValue;
                    float enjoyment = m_State.enjoymentValue;

                    if (activationMultiplier > 0f)
                    {
                        Assert.GreaterOrEqual(activation, prevActivation - Epsilon,
                            $"{type} slider at {v}: activationValue must not decrease for a positive multiplier.");
                    }
                    else if (activationMultiplier < 0f)
                    {
                        Assert.LessOrEqual(activation, prevActivation + Epsilon,
                            $"{type} slider at {v}: activationValue must not increase for a negative multiplier.");
                    }

                    if (enjoymentMultiplier > 0f)
                    {
                        Assert.GreaterOrEqual(enjoyment, prevEnjoyment - Epsilon,
                            $"{type} slider at {v}: enjoymentValue must not decrease for a positive multiplier.");
                    }
                    else if (enjoymentMultiplier < 0f)
                    {
                        Assert.LessOrEqual(enjoyment, prevEnjoyment + Epsilon,
                            $"{type} slider at {v}: enjoymentValue must not increase for a negative multiplier.");
                    }

                    prevActivation = activation;
                    prevEnjoyment = enjoyment;
                }
            }
        }

        [Test]
        public void SweepingEachEmotionAcrossFullRange_NeverOvershootsPastEndpoint()
        {
            foreach ((EmotionType type, float activationMultiplier, float enjoymentMultiplier) in Multipliers)
            {
                SetAllEmotions(0f);
                float neutralActivation = m_State.activationValue;
                float neutralEnjoyment = m_State.enjoymentValue;

                m_State.SetEmotionValue(type, 1f);
                float endActivation = m_State.activationValue;
                float endEnjoyment = m_State.enjoymentValue;

                // With the linear aggregation (0.5 + value * multiplier, clamped),
                // every intermediate value must lie between the neutral value and
                // the endpoint — no overshoot or undershoot.
                float loActivation = Mathf.Min(neutralActivation, endActivation);
                float hiActivation = Mathf.Max(neutralActivation, endActivation);
                float loEnjoyment = Mathf.Min(neutralEnjoyment, endEnjoyment);
                float hiEnjoyment = Mathf.Max(neutralEnjoyment, endEnjoyment);

                foreach (float v in Sweep)
                {
                    SetAllEmotions(0f);
                    m_State.SetEmotionValue(type, v);

                    float activation = m_State.activationValue;
                    float enjoyment = m_State.enjoymentValue;

                    Assert.IsTrue(activation >= loActivation - Epsilon && activation <= hiActivation + Epsilon,
                        $"{type} slider at {v}: activationValue {activation} must lie within " +
                        $"[{loActivation}, {hiActivation}] (between neutral and endpoint).");
                    Assert.IsTrue(enjoyment >= loEnjoyment - Epsilon && enjoyment <= hiEnjoyment + Epsilon,
                        $"{type} slider at {v}: enjoymentValue {enjoyment} must lie within " +
                        $"[{loEnjoyment}, {hiEnjoyment}] (between neutral and endpoint).");
                }
            }
        }

        [Test]
        public void SweepingEachEmotionAcrossFullRange_TotalSpanRespectsMultiplier()
        {
            foreach ((EmotionType type, float activationMultiplier, float enjoymentMultiplier) in Multipliers)
            {
                SetAllEmotions(0f);
                float neutralActivation = m_State.activationValue;
                float neutralEnjoyment = m_State.enjoymentValue;

                m_State.SetEmotionValue(type, 1f);

                float activationSpan = Mathf.Abs(m_State.activationValue - neutralActivation);
                float enjoymentSpan = Mathf.Abs(m_State.enjoymentValue - neutralEnjoyment);

                Assert.LessOrEqual(activationSpan, Mathf.Abs(activationMultiplier) + Epsilon,
                    $"{type} 0→1 moved activationValue by {activationSpan}, more than its multiplier {activationMultiplier}.");
                Assert.LessOrEqual(enjoymentSpan, Mathf.Abs(enjoymentMultiplier) + Epsilon,
                    $"{type} 0→1 moved enjoymentValue by {enjoymentSpan}, more than its multiplier {enjoymentMultiplier}.");
            }
        }

        [Test]
        public void SweepingEachEmotionAcrossFullRange_DerivedValuesStayClamped()
        {
            foreach ((EmotionType type, _, _) in Multipliers)
            {
                foreach (float v in Sweep)
                {
                    SetAllEmotions(0f);
                    m_State.SetEmotionValue(type, v);

                    Assert.IsTrue(m_State.activationValue >= 0f && m_State.activationValue <= 1f,
                        $"{type} slider at {v}: activationValue {m_State.activationValue} out of [0,1].");
                    Assert.IsTrue(m_State.enjoymentValue >= 0f && m_State.enjoymentValue <= 1f,
                        $"{type} slider at {v}: enjoymentValue {m_State.enjoymentValue} out of [0,1].");
                }
            }
        }

        [Test]
        public void SomeSliderCombinationReachesExactlyZeroAndOne_OnEachAxis()
        {
            // The axes must actually REACH the extremes, not merely stay within
            // [0, 1]. Scan every combination of sliders at 0 or 1 (2^5 = 32) and
            // assert the full range is attainable on both axes. (This would fail
            // if, e.g., the multipliers were weakened so the best achievable
            // activation were only 0.75.)
            float maxActivation = ComputeAxisExtreme(true, maximize: true);
            float minActivation = ComputeAxisExtreme(true, maximize: false);
            Assert.AreEqual(1f, maxActivation, Epsilon,
                "Some slider combination must drive activationValue to exactly 1.0; " +
                $"best achievable was {maxActivation}.");
            Assert.AreEqual(0f, minActivation, Epsilon,
                "Some slider combination must drive activationValue to exactly 0.0; " +
                $"best achievable was {minActivation}.");

            float maxEnjoyment = ComputeAxisExtreme(false, maximize: true);
            float minEnjoyment = ComputeAxisExtreme(false, maximize: false);
            Assert.AreEqual(1f, maxEnjoyment, Epsilon,
                "Some slider combination must drive enjoymentValue to exactly 1.0; " +
                $"best achievable was {maxEnjoyment}.");
            Assert.AreEqual(0f, minEnjoyment, Epsilon,
                "Some slider combination must drive enjoymentValue to exactly 0.0; " +
                $"best achievable was {minEnjoyment}.");
        }

        [Test]
        public void DominantSliderSweepsCoverEveryValue_OnEachAxis()
        {
            // For each axis, sweeping the single strongest positive and strongest
            // negative slider from 0 to 1 must cover every value in between with no
            // gaps: monotonic, consecutive-step delta bounded by step*|multiplier|,
            // starting at neutral (0.5) and reaching the extreme exactly.
            foreach (bool activation in new[] { true, false })
            {
                float bestPositive = float.NegativeInfinity;
                float bestNegative = float.PositiveInfinity;
                EmotionType positiveType = Multipliers[0].Type;
                EmotionType negativeType = Multipliers[0].Type;

                foreach ((EmotionType type, float a, float e) in Multipliers)
                {
                    float m = activation ? a : e;
                    if (m > bestPositive) { bestPositive = m; positiveType = type; }
                    if (m < bestNegative) { bestNegative = m; negativeType = type; }
                }

                string axisName = activation ? "activationValue" : "enjoymentValue";
                const int steps = 100;
                float stepSize = 1f / steps;

                // Positive sweep: non-decreasing, no gaps, reaches exactly 1.0.
                float prev = float.NaN;
                float first = 0f;
                for (int s = 0; s <= steps; s++)
                {
                    SetAllEmotions(0f);
                    m_State.SetEmotionValue(positiveType, s * stepSize);
                    float v = activation ? m_State.activationValue : m_State.enjoymentValue;
                    if (s == 0) { first = v; }
                    else
                    {
                        Assert.GreaterOrEqual(v, prev - Epsilon,
                            $"{positiveType} sweep on {axisName} must be non-decreasing at step {s}.");
                        Assert.LessOrEqual(Mathf.Abs(v - prev), bestPositive * stepSize + Epsilon,
                            $"{positiveType} sweep on {axisName} has a gap of {Mathf.Abs(v - prev)} at step {s} " +
                            "— not every value in between is reachable.");
                    }
                    prev = v;
                }
                Assert.AreEqual(0.5f, first, Epsilon,
                    $"{positiveType} sweep must start at neutral 0.5 on {axisName}.");
                Assert.AreEqual(1f, prev, Epsilon,
                    $"{positiveType} sweep must reach exactly 1.0 on {axisName} (got {prev}).");

                // Negative sweep: non-increasing, no gaps, reaches exactly 0.0.
                prev = float.NaN;
                for (int s = 0; s <= steps; s++)
                {
                    SetAllEmotions(0f);
                    m_State.SetEmotionValue(negativeType, s * stepSize);
                    float v = activation ? m_State.activationValue : m_State.enjoymentValue;
                    if (s == 0) { first = v; }
                    else
                    {
                        Assert.LessOrEqual(v, prev + Epsilon,
                            $"{negativeType} sweep on {axisName} must be non-increasing at step {s}.");
                        Assert.LessOrEqual(Mathf.Abs(v - prev), Mathf.Abs(bestNegative) * stepSize + Epsilon,
                            $"{negativeType} sweep on {axisName} has a gap of {Mathf.Abs(v - prev)} at step {s} " +
                            "— not every value in between is reachable.");
                    }
                    prev = v;
                }
                Assert.AreEqual(0.5f, first, Epsilon,
                    $"{negativeType} sweep must start at neutral 0.5 on {axisName}.");
                Assert.AreEqual(0f, prev, Epsilon,
                    $"{negativeType} sweep must reach exactly 0.0 on {axisName} (got {prev}).");
            }
        }

        /// <summary>
        /// Returns the best (max or min) derived value achievable over every
        /// combination of sliders at 0 or 1 (2^5 = 32 combinations).
        /// </summary>
        private float ComputeAxisExtreme(bool activation, bool maximize)
        {
            float best = maximize ? float.NegativeInfinity : float.PositiveInfinity;

            int combinationCount = 1 << Multipliers.Length;
            for (int mask = 0; mask < combinationCount; mask++)
            {
                SetAllEmotions(0f);
                for (int i = 0; i < Multipliers.Length; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        m_State.SetEmotionValue(Multipliers[i].Type, 1f);
                    }
                }

                float value = activation ? m_State.activationValue : m_State.enjoymentValue;
                best = maximize ? Mathf.Max(best, value) : Mathf.Min(best, value);
            }

            return best;
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
