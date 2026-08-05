using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WizardsCode.AnimationControl;
using WizardsCode.Character.UI;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character.Tests
{
    /// <summary>
    /// End-to-end tests proving that emotion values (as driven by the demo UI
    /// sliders) have the intended effect on the actual animator parameters
    /// (work item NV-0MSGM4XQP007V6UM).
    ///
    /// The chain under test is:
    ///   slider → EmotionalStateUIController.SetEmotionValue(name, value)
    ///          → EmotionalState.SetEmotionValue(type, value)
    ///          → activationValue / enjoymentValue
    ///          → EmotionalStateMachineBehavior.OnStateUpdate
    ///          → Animator.SetFloat("Activation"/"Enjoyment")
    ///
    /// m_PoseChangeTime is forced to 0 so the interpolation applies the target
    /// immediately, making the animator parameters deterministic functions of the
    /// emotional state.
    /// </summary>
    public class EmotionalStateToAnimatorTest
    {
        private const string ActivationParam = "Activation";
        private const string EnjoymentParam = "Enjoyment";
        // The submodule is nested inside the main project's Assets, so Unity sees
        // these tests at Assets/Wizards Code/Character-Dev/Assets/...
        private const string TestFolderPath =
            "Assets/Wizards Code/Character-Dev/Assets/Wizards Code/Character AI/Character/Scripts/Tests/Generated";
        private const string TestControllerPath = TestFolderPath + "/TestEmotionAnimatorController.controller";

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

        private GameObject m_Root;
        private Animator m_Animator;
        private EmotionalState m_State;
        private EmotionalStateMachineBehavior m_Behavior;

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.CreateFolder(
                    "Assets/Wizards Code/Character-Dev/Assets/Wizards Code/Character AI/Character/Scripts/Tests", "Generated");
            }

            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(TestControllerPath);
            controller.AddParameter(ActivationParam, AnimatorControllerParameterType.Float);
            controller.AddParameter(EnjoymentParam, AnimatorControllerParameterType.Float);

            m_Root = new GameObject("EmotionalState To Animator Test");
            m_Animator = m_Root.AddComponent<Animator>();
            m_Animator.runtimeAnimatorController = controller;

            m_State = m_Root.AddComponent<EmotionalState>();
            foreach ((EmotionType type, float act, float enj) in Multipliers)
            {
                m_State.emotions.Add(new EmotionMetric(type, 0f, act, enj));
            }

            m_Behavior = ScriptableObject.CreateInstance<EmotionalStateMachineBehavior>();

            // Force immediate pose application for determinism.
            SerializedObject so = new SerializedObject(m_Behavior);
            so.FindProperty("m_PoseChangeTime").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Behavior != null)
            {
                Object.DestroyImmediate(m_Behavior);
            }
            if (m_Root != null)
            {
                Object.DestroyImmediate(m_Root);
            }

            AssetDatabase.DeleteAsset(TestControllerPath);
            if (AssetDatabase.IsValidFolder(TestFolderPath))
            {
                AssetDatabase.DeleteAsset(TestFolderPath);
            }
        }

        [Test]
        public void SweepingEachEmotionAcrossFullRange_DrivesAnimatorParametersToDerivedValues()
        {
            m_Behavior.OnStateEnter(m_Animator, new AnimatorStateInfo(), 0);

            foreach ((EmotionType type, _, _) in Multipliers)
            {
                SetAllEmotions(0f);

                foreach (float v in Sweep)
                {
                    m_State.SetEmotionValue(type, v);
                    m_Behavior.OnStateUpdate(m_Animator, new AnimatorStateInfo(), 0);

                    Assert.AreEqual(
                        m_State.activationValue,
                        m_Animator.GetFloat(ActivationParam),
                        Epsilon,
                        $"{type} slider at {v}: animator Activation parameter must equal activationValue.");
                    Assert.AreEqual(
                        m_State.enjoymentValue,
                        m_Animator.GetFloat(EnjoymentParam),
                        Epsilon,
                        $"{type} slider at {v}: animator Enjoyment parameter must equal enjoymentValue.");
                }
            }
        }

        [Test]
        public void UiControllerSetEmotionValue_WiresSliderValueToEmotionalState()
        {
            // SetEmotionValue(string, float) is the exact target of the slider's
            // onValueChanged delegate wired up in EmotionalStateUIController.Start().
            GameObject go = new GameObject("UIController Test");
            go.SetActive(false); // defer Awake until the state reference is assigned

            EmotionalStateUIController controller = go.AddComponent<EmotionalStateUIController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("m_EmotionalState").objectReferenceValue = m_State;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(true);

            controller.SetEmotionValue("Anger", 0.75f);
            Assert.AreEqual(0.75f, m_State.GetEmotionValue(EmotionType.Anger), Epsilon,
                "Slider handler for Anger must set the Anger emotion value.");

            controller.SetEmotionValue("Pleasure", 0.25f);
            Assert.AreEqual(0.25f, m_State.GetEmotionValue(EmotionType.Pleasure), Epsilon,
                "Slider handler for Pleasure must set the Pleasure emotion value.");

            Object.DestroyImmediate(go);
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
