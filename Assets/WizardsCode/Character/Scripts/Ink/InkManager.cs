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
using static WizardsCode.Character.EmotionalState;
using WizardsCode.Stats;
using Cinemachine;

namespace WizardsCode.Ink
{
    public class InkManager : AbstractSingleton<InkManager>
    {
        enum Direction { 
            Cue, 
            TurnToFace, 
            PlayerControl, 
            MoveTo, 
            SetEmotion, 
            Action, 
            StopMoving, 
            AnimationParam,
            Camera
        }

        [Header("Script")]
        [SerializeField, Tooltip("The Ink file to work with.")]
        TextAsset m_InkJSON;
        [SerializeField, Tooltip("The actors that are available in this scene.")]
        ActorController[] m_Actors;
        [SerializeField, Tooltip("The cues that will be used in this scene.")]
        ActorCue[] m_Cues;
        [SerializeField, Tooltip("Should the story start as soon as the game starts. If this is set to false the story will not start until a trigger or similar is set.")]
        bool m_PlayOnAwake = true;

        [Header("Camera")]
        [SerializeField, Tooltip("The Cinemachine Brain used to control the virtual cameras.")]
        CinemachineBrain cinemachine;

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
            IsDisplayingUI = m_PlayOnAwake;
        }

        /// <summary>
        /// Jump to a specific point in the story.
        /// </summary>
        /// <param name="knot">The name of the knot to jump to.</param>
        /// <param name="stitch">The name of the stitch within the named not.</param>
        internal void JumpToPath(string knot, string stitch = "")
        {
            if (!string.IsNullOrEmpty(stitch))
            {
                m_Story.ChoosePathString(knot + "." + stitch);
            }
            else
            {
                m_Story.ChoosePathString(knot);
            }
        }

        public void ChoosePath(string knotName, string stitchName = null)
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
            m_NewStoryText.Clear();
            m_IsUIDirty = true;
        }

        void PromptCue(string[]args)
        {
            if (!ValidateArgumentCount(Direction.Cue, args, 2))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            ActorCue cue = FindCue(args[1].Trim());

            if (actor != null)
            {
                actor.Prompt(cue);
            }
        }

        /// <summary>
        /// The MoveTo direction instructs an actor to move to a specific location. It is up to the ActorController
        /// to decide how they should move.
        /// </summary>
        /// <param name="args"></param>
        void MoveTo(string[] args)
        {
            if (!ValidateArgumentCount(Direction.MoveTo, args, 2))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            Transform target = FindTarget(args[1].Trim());

            if (actor != null)
            {
                actor.MoveTo(target);
            }
        }

        /// <summary>
        /// The SetEmotion direction looks for a defined emotion on an character and sets it if found.
        /// 
        /// </summary>
        /// <param name="args">[ActorName], [EmotionName], [Float]</param>
        void SetEmotion(string[] args)
        {
            if (!ValidateArgumentCount(Direction.SetEmotion, args, 3))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            if (actor)
            {
                EmotionalState emotions = FindEmotionalState(actor);
                EmotionType emotion = (EmotionType)Enum.Parse(typeof(EmotionType), args[1].Trim());
                float value = float.Parse(args[2].Trim());

                emotions.SetEmotionValue(emotion, value);
            }
        }


        /// <summary>
        /// Tell an actor to prioritize a particular behaviour. Under normal circumstances
        /// this behaviour will be executed as soon as possible, as long as the necessary
        /// preconditions have been met and no higher priority item exists.
        /// 
        /// </summary>
        /// <param name="args">[ActorName], [BehaviourName]</param>
        void Action(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Action, args, 2, 3))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            Brain brain = actor.GetComponentInChildren<Brain>();
            brain.PrioritizeBehaviour(args[1].Trim());
        }

        /// <summary>
        /// Tell an actor to stop moving immediately.
        /// 
        /// </summary>
        /// <param name="args">[ActorName]</param>
        void StopMoving(string[] args)
        {
            if (!ValidateArgumentCount(Direction.StopMoving, args, 1))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            actor.StopMoving();
        }

        /// <summary>
        /// Set an animation parameter on an actor.
        /// 
        /// </summary>
        /// <param name="args">[ActorName] [ParameterName] [Value] - if Value is missing it is assumed that the parameter is a trigger</param>
        void AnimationParam(string[] args)
        {
            if (!ValidateArgumentCount(Direction.AnimationParam, args, 2, 3))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            string paramName = args[1].Trim();

            if (args.Length == 2)
            {
                actor.Animator.SetTrigger(name);
                return;
            }

            string value = args[2].Trim();

            if (value == "False")
            {
                actor.Animator.SetBool(paramName, false);
                return;
            } else if (value == "True")
            {
                actor.Animator.SetBool(paramName, true);
                return;
            }

            Debug.LogError("Direction to set an animator value that is a string, float or int. These are not supported right now.");
        }

        /// <summary>
        /// Switch to a specific camera and optionally look at a named object.
        /// 
        /// </summary>
        /// <param name="args">[CameraName] [TargetName] - if TargetName is missing it is assumed that the camera is already setup correctly</param>
        void Camera(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Camera, args, 1, 2))
            {
                return;
            }

            CinemachineVirtualCamera newCamera;
            Transform t = FindTarget(args[0].Trim());
            if (t)
            {
                newCamera = t.gameObject.GetComponent<CinemachineVirtualCamera>();
                cinemachine.ActiveVirtualCamera.Priority = 1;
                newCamera.Priority = 99;

                if (args.Length == 2)
                {
                    t = FindTarget(args[1].Trim());
                    if (t)
                    {
                        newCamera.LookAt = t;
                    }
                }
            }
        }

        void TurnToFace(string[] args)
        {
            if (!ValidateArgumentCount(Direction.TurnToFace, args, 2))
            {
                return;
            }

            ActorController actor = FindActor(args[0].Trim());
            Transform target = FindTarget(args[1].Trim());

            if (target != null)
            {
                actor.gameObject.transform.LookAt(target.position);
                actor.LookAtTarget = target.transform;
            }
        }

        EmotionalState FindEmotionalState(ActorController actor)
        {
            EmotionalState emotions = actor.GetComponent<EmotionalState>();
            if (!emotions)
            {
                Debug.LogError("There is a direction to set an emotion value on " + actor + " but there is no EmotionalState component on that actor.");
            }
            return emotions;
        }

        Transform FindTarget(string objectName)
        {
            ActorController actor = FindActor(objectName, false);
            if (actor != null)
            {
                return actor.transform.root;
            }

            //TODO Don't use Find at runtime. When initiating the InkManager we should pre-emptively parse all directions and cache the results - or perhaps (since the story may be larger or dynamic) we should do it in a CoRoutine just ahead of execution of the story chunk
            GameObject go = GameObject.Find(objectName);
            if (go)
            {
                return go.transform;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Look through the known actors to see if we have one with the given name.
        /// </summary>
        /// <param name="actorName">The name of the actor we want.</param>
        /// <param name="logError">If true (the default) an error will be logged to the console if the actor is not found.</param>
        /// <returns>The actor with the given name or null if they cannot be found.</returns>
        private ActorController FindActor(string actorName, bool logError = true)
        {
            ActorController actor = null;
            for (int i = 0; i < m_Actors.Length; i++)
            {
                if (m_Actors[i].name == actorName.Trim())
                {
                    actor = m_Actors[i];
                    break;
                }
            }

            if (logError && actor == null)
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

            while (m_Story.canContinue)
            {
                line = m_Story.Continue();

                // Process Directions;
                int cmdIdx = line.IndexOf(">>>");
                if (cmdIdx >= 0)
                {
                    int startIdx = line.IndexOf(' ', cmdIdx);
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
                        case Direction.PlayerControl:
                            SetPlayerControl(args);
                            string resp = m_Story.ContinueMaximally();
                            break;
                        case Direction.MoveTo:
                            MoveTo(args);
                            break;
                        case Direction.SetEmotion:
                            SetEmotion(args);
                            break;
                        case Direction.Action:
                            Action(args);
                            break;
                        case Direction.StopMoving:
                            StopMoving(args);
                            break;
                        case Direction.AnimationParam:
                            AnimationParam(args);
                            break;
                        case Direction.Camera:
                            Camera(args);
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

        void SetPlayerControl(string[] args)
        {
            ValidateArgumentCount(Direction.PlayerControl, args, 1);

            if (args[0].Trim().ToLower() == "on")
            {
                SetPlayerControl(true);
                //TODO At present there we need to set a DONE divert in the story which is less than ideal since it means the writers can't use the Inky test tools: asked for guidance at https://discordapp.com/channels/329929050866843648/329929390358265857/818370835177275392
                
            }
            else
            {
                SetPlayerControl(false);
            }
        }

        internal void SetPlayerControl(bool value)
        {
            IsDisplayingUI = !value;
        }

        bool ValidateArgumentCount(Direction direction, string[] args, int minRequiredCount, int maxRequiredCount = 0)
        {
            string error = "";
            string warning = "";

            if (args.Length < minRequiredCount)
            {
                error = "Too few arguments in Direction. There should be at least " + minRequiredCount + ". Ignoring direction: ";
            }
            else if (maxRequiredCount > 0)
            {
                if (args.Length > maxRequiredCount)
                {
                    warning = "Incorrect number of arguments in Direction. There should be between " + minRequiredCount + " and " + maxRequiredCount + " Ignoring the additional arguments: ";
                }
            } else
            {
                if (args.Length > minRequiredCount)
                {
                    warning = "Incorrect number of arguments in Direction. There should " + minRequiredCount + ". Ignoring the additional arguments: ";
                }
            }

            string msg = "";
            msg += "`>>> " + direction + ": ";
            for (int i = 0; i < args.Length; i++)
            {
                msg += args[i].ToString();
                if (i < args.Length - 1)
                {
                    msg += ", ";
                }
            }
            msg += "`";

            if (!string.IsNullOrEmpty(error))
            {
                msg = error + msg;
                Debug.LogError(msg);
                return false;
            } else if (!string.IsNullOrEmpty(warning))
            {
                msg = warning + msg;
                Debug.LogWarning(msg);
            }

            return true;
        }
    }
}
