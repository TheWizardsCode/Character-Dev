using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Character
{
    [CustomEditor(typeof(EmotionalState))]
    [CanEditMultipleObjects]
    public class EmotionalStateEditor : UnityEditor.Editor
    {
        EmotionalState state;
        SerializedProperty emotionsProp;

        void OnEnable()
        {
            state = serializedObject.targetObject as EmotionalState;
            emotionsProp = serializedObject.FindProperty("emotions");

        }

        private void OnValidate()
        {
            emotionsProp = serializedObject.FindProperty("emotions");
            EmotionalState state = (EmotionalState)serializedObject.targetObject;
            if (state.emotions.Count == 0)
            {
                state.emotions = new List<EmotionMetric>();

                EmotionMetric emotion = new EmotionMetric(EmotionalState.EmotionType.Anger, 0, 1, -0.2f);
                state.emotions.Add(emotion);

                // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
                // Hate - activationMultiplier = 0.8, pleasantnessMultiplier = -0.4
                // Contempt - activationMultiplier = 0.5, pleasantnessMultiplier = -0.5
                // Disgust - activationMultiplier = 0.3, pleasantnessMultiplier = -0.8


                emotion = new EmotionMetric(EmotionalState.EmotionType.Fear, 0, 0.1f, -1f);
                state.emotions.Add(emotion);

                // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
                // Disappointment - activationMultiplier = -0.1, pleasantnessMultiplier = -1
                // Shame - activationMultiplier = -0.3, pleasantnessMultiplier = -0.8
                // Regret - activationMultiplier = -0.5, pleasantnessMultiplier = -0.5
                // Guilt - activationMultiplier = -0.8, pleasantnessMultiplier = -0.3
                // Sadness - activationMultiplier = -1, pleasantnessMultiplier = -0.1

                emotion = new EmotionMetric(EmotionalState.EmotionType.Sadness, 0, -1, -0.1f);
                state.emotions.Add(emotion);

                // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
                // Compassion - activationMultiplier = -1, pleasantnessMultiplier = 0.1
                // Relief - activationMultiplier = -0.8, pleasantnessMultiplier = 0.3
                // Admiration - activationMultiplier = -0.6, pleasantnessMultiplier = 0.5
                // Love - activationMultiplier = -0.3, pleasantnessMultiplier = 0.8
                // Contentment - activationMultiplier = -0.1, pleasantnessMultiplier = 1

                emotion = new EmotionMetric(EmotionalState.EmotionType.Pleasure, 0, 0.1f, 1f);
                state.emotions.Add(emotion);

                // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
                // Joy - activationMultiplier = 0.3, pleasantnessMultiplier = 0.8
                // Pride - activationMultiplier = 0.5, pleasantnessMultiplier = 0.5
                // Amusement - activationMultiplier = 0.8, pleasantnessMultiplier = 0.3

                emotion = new EmotionMetric(EmotionalState.EmotionType.Interest, 0, 0.45f, 0.2f);
                state.emotions.Add(emotion);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Current enjoyment: (" + state.enjoymentValue + ") " + state.enjoymentDescriptor );
            EditorGUILayout.LabelField("Current activation: (" + state.activationValue + ") " + state.activationDescriptor);

            EditorGUILayout.PropertyField(emotionsProp, true);
            
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}