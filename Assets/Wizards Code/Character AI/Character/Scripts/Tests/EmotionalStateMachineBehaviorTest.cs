using NUnit.Framework;
using WizardsCode.AnimationControl;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// Tests for the pose-change interpolation used by
    /// <see cref="EmotionalStateMachineBehavior"/> to drive the animator
    /// <c>Activation</c>/<c>Enjoyment</c> parameters toward the current
    /// <see cref="EmotionalState"/> values.
    ///
    /// The interpolation is frame-rate independent: progress is measured in
    /// elapsed time divided by the configured <c>m_PoseChangeTime</c>, so the
    /// animator parameters reach the target within (approximately) the pose
    /// change time regardless of frame rate (work item NV-0MSG2R1Z7000KGBZ).
    /// </summary>
    public class EmotionalStateMachineBehaviorTest
    {
        private const float Epsilon = 0.001f;

        [Test]
        public void ParameterReachesTargetWithinPoseChangeTime_AtMultipleFrameRates()
        {
            const float poseChangeTime = 1.04f; // value configured on the demo character
            const float start = 0.2f;
            const float end = 0.85f;

            foreach (float deltaTime in new[] { 1f / 30f, 1f / 60f, 1f / 120f })
            {
                float elapsed = 0f;

                // Simulate frames until twice the pose change time has elapsed.
                while (elapsed < poseChangeTime * 2f)
                {
                    elapsed += deltaTime;
                    EmotionalStateMachineBehavior.PoseChangeValues values =
                        EmotionalStateMachineBehavior.ComputePoseValues(
                            start, end, start, end, elapsed, poseChangeTime);

                    Assert.LessOrEqual(values.activation, end + Epsilon,
                        $"Interpolated value overshot the target at {deltaTime * 1000f:0.#}ms frames.");
                }

                Assert.AreEqual(end, EmotionalStateMachineBehavior.ComputePoseValues(
                        start, end, start, end, elapsed, poseChangeTime).activation,
                    end * 0.01f,
                    $"Activation parameter did not reach the target within 2x pose change time at {deltaTime * 1000f:0.#}ms frames.");
            }
        }

        [Test]
        public void InterpolationIsIndependentOfFrameRate_AtSameElapsedTime()
        {
            const float poseChangeTime = 2f;
            const float start = 0f;
            const float end = 1f;
            const float checkTime = poseChangeTime / 2f;

            float value30 = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, checkTime, poseChangeTime).activation;
            float value60 = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, checkTime, poseChangeTime).activation;
            float value120 = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, checkTime, poseChangeTime).activation;

            Assert.AreEqual(value30, value60, Epsilon);
            Assert.AreEqual(value60, value120, Epsilon);
        }

        [Test]
        public void InterpolationReachesExactlyTheTargetAfterPoseChangeTime()
        {
            const float poseChangeTime = 1f;
            const float start = 0.1f;
            const float end = 0.9f;

            var values = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, poseChangeTime, poseChangeTime);

            Assert.AreEqual(end, values.activation, Epsilon,
                "Activation should equal the target exactly once the pose change time has elapsed.");
            Assert.AreEqual(end, values.enjoyment, Epsilon,
                "Enjoyment should equal the target exactly once the pose change time has elapsed.");
        }

        [Test]
        public void TargetChangeMidInterpolationConvergesToNewTarget()
        {
            // Covers the randomization path: when the end value is changed
            // (e.g. by m_RandomizeActivation / m_RandomizeEnjoyment, or a new
            // emotional state), the interpolation must converge to the new target.
            const float poseChangeTime = 1f;
            const float start = 0f;
            const float end = 0.5f;

            // Half-way through the first transition.
            float elapsed = poseChangeTime / 2f;
            var mid = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, elapsed, poseChangeTime);
            Assert.Greater(mid.activation, start);
            Assert.Less(mid.activation, end);

            // The target changes now; a fresh transition begins from the mid value.
            const float newEnd = 1f;
            float localElapsed = 0f;
            float value = mid.activation;
            while (localElapsed < poseChangeTime * 2f)
            {
                localElapsed += 1f / 60f;
                value = EmotionalStateMachineBehavior.ComputePoseValues(
                    mid.activation, newEnd, mid.activation, newEnd, localElapsed, poseChangeTime).activation;
            }

            Assert.AreEqual(newEnd, value, 0.01f,
                "After a mid-interpolation target change the parameter must converge to the new target.");
        }

        [Test]
        public void ZeroPoseChangeTimeJumpsImmediatelyToTarget()
        {
            const float start = 0.2f;
            const float end = 0.9f;

            var values = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, 0f, 0f);

            Assert.AreEqual(end, values.activation, Epsilon);
            Assert.AreEqual(end, values.enjoyment, Epsilon);
        }

        [Test]
        public void ValuesNeverMoveBeforeProgressStarts()
        {
            const float poseChangeTime = 1f;
            const float start = 0.3f;
            const float end = 0.8f;

            var values = EmotionalStateMachineBehavior.ComputePoseValues(
                start, end, start, end, 0f, poseChangeTime);

            Assert.AreEqual(start, values.activation, Epsilon);
            Assert.AreEqual(start, values.enjoyment, Epsilon);
        }
    }
}
