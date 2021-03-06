using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine.UI;
using WizardsCode.Character;

namespace WizardsCode.Ink
{
    public class InkManager : MonoBehaviour
    {
        [Header("Script")]
        [SerializeField, Tooltip("The Ink file to work with.")]
        TextAsset m_InkJSON;
        [SerializeField, Tooltip("The actors that are available in this scene.")]
        ActorController[] m_Actors;
        [SerializeField, Tooltip("The cues that will be used in this scene.")]
        ActorCue[] m_Cues;

        [Header("UI")]
        [SerializeField, Tooltip("The panel on which to display the text in the story.")]
        RectTransform textPanel;
        [SerializeField, Tooltip("The panel on which to display the choice buttons in the story.")]
        RectTransform choicesPanel;
        [SerializeField, Tooltip("Story chunk prefab for creation when we want to display a story chunk.")]
        TextMeshProUGUI m_StoryChunkPrefab;
        [SerializeField, Tooltip("Story choice button")]
        Button m_ChoiceButtonPrefab;

        Story story;

        private void Awake()
        {
            story = new Story(m_InkJSON.text);
            RefreshUI();
        }

        private void EraseUI()
        {
            for (int i = 0; i < textPanel.transform.childCount; i++)
            {
                Destroy(textPanel.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < choicesPanel.transform.childCount; i++) {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }
        }

        private void RefreshUI()
        {
            EraseUI();

            TextMeshProUGUI chunkText = Instantiate(m_StoryChunkPrefab) as TextMeshProUGUI;
            chunkText.text = ProcessStoryChunk();
            chunkText.transform.SetParent(textPanel.transform, false);

            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                Button choiceButton = Instantiate(m_ChoiceButtonPrefab) as Button;
                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                choiceText.text = story.currentChoices[i].text;
                choiceButton.transform.SetParent(choicesPanel.transform, false);

                choiceButton.onClick.AddListener(delegate
                {
                    ChooseStoryChoice(choice);
                });
            }
        }

        void ChooseStoryChoice(Choice choice)
        {
            story.ChooseChoiceIndex(choice.index);
            RefreshUI();
        }

        void ProcessCue(string actorName, string cueName)
        {
            ActorCue cue = null;
            for (int i = 0; i < m_Cues.Length; i++)
            {
                if (m_Cues[i].name == cueName)
                {
                    cue = m_Cues[i];
                    break;
                }
            }

            if (cue == null) {
                Debug.LogError("Script contains # Cue - " + actorName + " - " + cueName + ". However, the cue cannot be found.");
                return;
            }

            for (int i = 0; i < m_Actors.Length; i++)
            {
                if (m_Actors[i].name == actorName)
                {
                    m_Actors[i].Prompt(cue);
                    return;
                }
            }

            Debug.LogError("Script contains # Cue - " + actorName + " - " + cueName + ". However, the actor cannot be found.");
        }

        string ProcessStoryChunk()
        {
            string text = "";
            if (story.canContinue)
            {
                text = story.ContinueMaximally();
            }

            List<string> tags = story.currentTags;
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].StartsWith("Cue"))
                {
                    string[] split = tags[i].Split('-');
                    string actorName = split[1].Trim();
                    string cueName = split[2].Trim();
                    ProcessCue(actorName, cueName);
                }
            }

            return text;
        }
    }
}
