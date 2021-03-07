using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine.UI;
using WizardsCode.Character;
using WizardsCode.Utility;
using System.Text;
using System;

namespace WizardsCode.Ink
{
    public class InkManager : AbstractSingleton<InkManager>
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

        Story m_Story;
        bool m_IsUIDirty = false;
        StringBuilder m_NewStoryText = new StringBuilder();
        enum Direction { Cue, TurnToFace }

        private bool m_IsDisplayingUI = false;
        internal bool IsDisplayingUI
        {
            get { return m_IsDisplayingUI; } 
            set { 
                m_IsDisplayingUI = value;
                m_IsUIDirty = value;
            }
        }

        private void Awake()
        {
            m_Story = new Story(m_InkJSON.text);
        }

        public void ChoosePath(string knotName, string stitchName)
        {
            string path = knotName;
            if (!string.IsNullOrEmpty(stitchName))
            {
                path = string.IsNullOrEmpty(path) ? stitchName : "." + stitchName;
            }

            if (!string.IsNullOrEmpty(path))
            {
                m_Story.ChoosePathString(path);
            }
        }

        public void Update()
        {
            if (IsDisplayingUI)
            {
                if (m_IsUIDirty)
                {
                    ProcessStoryChunk();
                    UpdateGUI();
                }
            } else
            {
                textPanel.gameObject.SetActive(false);
                choicesPanel.gameObject.SetActive(false);
            }
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

        private void UpdateGUI()
        {
            EraseUI();

            textPanel.gameObject.SetActive(true);
            TextMeshProUGUI chunkText = Instantiate(m_StoryChunkPrefab) as TextMeshProUGUI;
            chunkText.text = m_NewStoryText.ToString();
            chunkText.transform.SetParent(textPanel.transform, false);

            for (int i = 0; i < m_Story.currentChoices.Count; i++)
            {
                choicesPanel.gameObject.SetActive(true);
                Choice choice = m_Story.currentChoices[i];
                Button choiceButton = Instantiate(m_ChoiceButtonPrefab) as Button;
                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                choiceText.text = m_Story.currentChoices[i].text;
                choiceButton.transform.SetParent(choicesPanel.transform, false);

                choiceButton.onClick.AddListener(delegate
                {
                    ChooseStoryChoice(choice);
                });
            }

            m_IsUIDirty = false;
        }

        /// <summary>
        /// Called whenever the story needs to progress.
        /// </summary>
        /// <param name="choice">The choice made to progress the story.</param>
        void ChooseStoryChoice(Choice choice)
        {
            m_Story.ChooseChoiceIndex(choice.index);
            m_IsUIDirty = true;
        }

        void PromptCue(string[]args)
        {
            if (!ValidateArgumentCount(args, 2))
            {
                return;
            }


            ActorController actor = FindActor(args[0].Trim());
            ActorCue cue = FindCue(args[1].Trim());

            actor.Prompt(cue);
        }

        void TurnToFace(string[] args)
        {
            if (!ValidateArgumentCount(args, 2))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            Transform target = FindTarget(args[1].Trim());

            if (target != null)
            {
                actor.LookAtTarget = target;
            }
        }

        Transform FindTarget(string objectName)
        {
            ActorController actor = FindActor(objectName);
            if (actor != null)
            {
                return actor.transform.root;
            }

            Debug.LogError("There is a direction that requires an object to be found in the scene, however, only actors are supported at this time. The Manager needs to be made aware of props in the scene that will be used.");
            return null;
        }

        private ActorController FindActor(string actorName)
        {
            ActorController actor = null;
            for (int i = 0; i < m_Actors.Length; i++)
            {
                if (m_Actors[i].name == actorName)
                {
                    actor = m_Actors[i];
                    break;
                }
            }

            if (actor == null)
            {
                Debug.LogError("Script contains a direction for " + actorName + ". However, the actor cannot be found.");
            }

            return actor;
        }

        private ActorCue FindCue(string cueName)
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

            if (cue == null)
            {
                Debug.LogError("Script contains a Cue direction but the cue called `" + cueName + "` cannot be found.");
            }

            return cue;
        }

        /// <summary>
        /// Grab the current story chunk and parse it for processing.
        /// </summary>
        void ProcessStoryChunk()
        {
            string line;
            m_NewStoryText.Clear();

            while (m_Story.canContinue)
            {
                line = m_Story.Continue();

                // Process Directions;
                if (line.StartsWith(">>>"))
                {
                    int startIdx = line.IndexOf(' ');
                    int endIdx = line.IndexOf(':') - startIdx;
                    Enum.TryParse(line.Substring(startIdx, endIdx).Trim(), out Direction cmd);
                    string[] args = line.Substring(endIdx + startIdx + 1).Split(',');

                    switch (cmd) {
                        case Direction.Cue:
                            PromptCue(args);
                            break;
                        case Direction.TurnToFace:
                            TurnToFace(args);
                            break;
                        default:
                            Debug.LogError("Unknown Direction: " + line);
                            break;
                    }
                } else
                {
                    m_NewStoryText.AppendLine(line);
                }

                // Process Tags
                List<string> tags = m_Story.currentTags;
                for (int i = 0; i < tags.Count; i++)
                {
                }

            }

            m_IsUIDirty = true;
        }

        bool ValidateArgumentCount(string[] args, int requiredCount)
        {
            if (args.Length < 2)
            {
                Debug.LogError("Cue direction has too few arguments. There should be (actorName, cueName). Ignoring direction.");
                return false;
            }
            else if (args.Length > 2)
            {
                Debug.LogWarning("Cue direction has too many arguments. There should be (actorName, cueName). Ignoring the additional arguments.");
                return true;
            }

            return true;
        }
    }
}
