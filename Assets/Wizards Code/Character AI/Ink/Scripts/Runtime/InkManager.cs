#if INK_PRESENT
using UnityEngine;
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
using UnityEngine.Serialization;

using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace WizardsCode.Ink
{
    /// <summary>
    /// The InkManager parses the Ink file and instructs the actors, camera, sound etc. what to do.
    /// 
    ///TODO this is getting unwiedly at this point. Separate out the implementations of directions into their own
    ///class.
    /// </summary>
    public class InkManager : AbstractSingleton<InkManager>
    {
        enum Direction {
            Unkown,
            Cue,
            TurnToFace,
            PlayerControl,
            MoveTo,
            SetEmotion,
            Action,
            StopMoving,
            AnimationParam,
            Camera,
            Music,
            WaitFor,
            Audio,
            AI,
            Teleport,
            Enable,
            SoundFX
        }

        #region Inspector Fields
        [Header("Script")]
        [SerializeField, Tooltip("The Ink file to work with.")]
        TextAsset m_InkJSON;
        [SerializeField, Tooltip("The actors that are available in this scene.")]
        BaseActorController[] m_Actors;
        [SerializeField, Tooltip("The cached objects that are available in this scene. Add any objects to this collection that you need to act upon in your ink script. This has two advantages, firstly it is a faster search (not search will fall back to the Find if cache hit is not detected). Secondly, it will work even if the object is inactive.")]
        List<Transform> m_CachedObjects = new List<Transform>();
        [SerializeField, Tooltip("The cues that will be used in this scene.")]
        ActorCue[] m_Cues;
        [SerializeField, Tooltip("Should the story start as soon as the game starts. If this is set to false the story will not start until a trigger or similar is set.")]
        bool m_PlayOnAwake = true;

        [Header("Standard Cues")]
        [SerializeField, Tooltip("The cue to send to an actor when they start talking.")]
        ActorCueAnimator m_startTalkingCue;
        [SerializeField, Tooltip("The cue to send to an actor when they hav finished talking.")]
        ActorCueAnimator m_stopTalkingCue;

        [Header("Camera, Lights and Sound")]
        [SerializeField, Tooltip("The Cinemachine Brain used to control the virtual cameras.")]
        CinemachineBrain cinemachine;
        [SerializeField, Tooltip("Camera to use as a fade to black/white or other when transitioning betweeen cameras. If not null then this camera will be inserted between all camera changes. The actuable fade properties will be set in the Cinemachine Brain.")]
        CinemachineVirtualCamera m_FadeCamera;
        [SerializeField, Tooltip("The audio source for music playback.")]
        AudioSource m_MusicAudioSource;
        [SerializeField, Tooltip("The audio source for sound effects.")]
        AudioSource m_FXAudioSource;

        [Header("Actor Setup")]
        [SerializeField, Tooltip("The name of the player object.")]
        string m_PlayerName = "Player";
        [SerializeField, Tooltip("The layer on which all party members will be found.")]
        LayerMask m_PartyLayerMask;

        [Header("UI")]
        [SerializeField, Tooltip("The panel on which to display the choice buttons in the story.")]
        RectTransform choicesPanel;
        [SerializeField, Tooltip("Story choice button")]
        Button m_ChoiceButtonPrefab;
        [SerializeField, Tooltip("X offset for the buttons position on screen.")]
        float m_ButtonXOffset = 150;
        [SerializeField, Tooltip("Y offset for the buttons position. Note that the actual offset will be the button number multiplied by this amount, meaning the buttons will be stacked on top of one another.")]
        float m_ButtonYOffset = 100;
        [SerializeField, Tooltip("The time it takes for a button to move from its start position to its target position when spawned in.")]
        float m_ButtonAnimationTime = 0.6f;
        [SerializeField, Tooltip("Dialogue and narration bubble controller that will display the text for the player.")]
        [FormerlySerializedAs("m_TextBubbleComp")]
        TextBubbleController m_TextBubble;
        [SerializeField, Tooltip("When the Ink story calls for an actor to tall how longer, per character in the text, should they be kept in an active state. The actor will not carry out any other actions until this time has elepased. Set to 0 to not have the speaker wait.")]
        float m_ActiveTimePerCharacter = 0.01f;
        [SerializeField, Tooltip("If there is only one option available in the story should it automatically be chosen? If set to false the story will wait for the player to select the choice.")]
        bool m_autoAdvanceSingleChoice = true;

        [Header("Debug")]
        [SerializeField, Tooltip("The starting path to use when running in the editor.")]
        string m_StartingPath = "";
        #endregion
        
        #region Variables
        Story m_Story;
        bool isUIDirty = false;
        StringBuilder m_NewTextToDisplay = new StringBuilder();
        
        List<WaitForState> waitForStates = new List<WaitForState>();
        bool wasWaiting = false;

        private bool m_IsDisplayingUI = false;
        private BaseActorController m_activeSpeaker;
        private bool isAIActive = true;
        #endregion

        #region Properties
        internal bool IsDisplayingUI
        {
            get { return m_IsDisplayingUI; }
            set {
                m_IsDisplayingUI = value;
                isUIDirty = value;
            }
        }
        #endregion

        #region Lifecycle Events
        private void Awake()
        {
            m_Story = new Story(m_InkJSON.text);
            IsDisplayingUI = m_PlayOnAwake;

            m_Story.BindExternalFunction("GetPartyNoticability", () =>
            {
                return GetPartyNoticability();
            });
        }

        private void Start()
        {
            if (cinemachine == null)
            {
                Debug.LogWarning("Cinemachine brain is not set in the inspector. Auto discovering. You should set this in the inspectr.");
                cinemachine = GameObject.FindObjectOfType<CinemachineBrain>();
            }

            if (!string.IsNullOrEmpty(m_StartingPath))
            {
                m_Story.ChoosePathString(m_StartingPath);
            }

            BindExternalFunctions();
        }

        private void OnDestroy()
        {
            string path = Application.persistentDataPath + $"/StoryLog_{DateTime.Now.ToFileTime()}.log";
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine("Story Log");
            writer.WriteLine("");
            writer.WriteLine(m_StoryDebugLog.ToString());
            writer.Close();

            Debug.Log($"Story Log written to {path}");
        }
        #endregion

        /// <summary>
        /// Return a float value between 0 and 1 indicating how likely the party is to be noticed.
        /// 0 means will not be noticed, 1 means will be noticed.
        /// </summary>
        /// <returns>a % chance of being noticed</returns>
        float GetPartyNoticability()
        {
            List<BaseActorController> members = GetNearbyPartyMembers();
            float noticability = 0.5f;
            for (int i = 0; i < members.Count; i++)
            {
                noticability += members[i].Noticability;
            }

            return Mathf.Clamp01(noticability / members.Count);
        }

        /// <summary>
        /// Check for party members nearby and return a list of all such actors.
        /// </summary>
        /// <returns>All actors allied to the player that are nearby.</returns>
        List<BaseActorController> GetNearbyPartyMembers()
        {
            List<BaseActorController> result = new List<BaseActorController>();

            BaseActorController player = FindActor(m_PlayerName);
            Collider[] all = Physics.OverlapSphere(player.transform.position, 10, m_PartyLayerMask);
            BaseActorController current;
            for (int i = 0; i < all.Length; i++)
            {
                current = all[i].GetComponentInParent<BaseActorController>();
                if (current)
                {
                    result.Add(current);
                }
            }

            return result;
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

        private bool isWaiting
        {
            get
            {
                for (int i = waitForStates.Count - 1; i >= 0; i--)
                {
                    switch (waitForStates[i].waitType)
                    {
                        case WaitForState.WaitType.ReachTarget:
                            if (waitForStates[i].actor.IsMoving) {
                                //Debug.Log($"Waiting for {waitForStates[i].actor} to reach destination.");
                                return true;
                            } else
                            {
                                wasWaiting = true;
                                waitForStates.RemoveAt(i);
                                if (waitForStates.Count == 0) return false;
                            }
                            break;
                        case WaitForState.WaitType.Time:
                            if (Time.timeSinceLevelLoad < waitForStates[i].endTime)
                            {
                                //Debug.Log($"Waiting until {waitForStates[i].m_WaitUntilTime} currently {Time.timeSinceLevelLoad}");
                                return true;
                            }
                            else
                            {
                                wasWaiting = true;
                                waitForStates.RemoveAt(i);
                                if (waitForStates.Count == 0) return false;
                            }
                            break;
                        default:
                            Debug.LogError("Direction to wait gives a unrecognized state to wait for: '" + waitForStates[i].waitType + "'");
                            break;
                    }
                }
                return false;
            }
        }

        public void Update()
        {
            if (isWaiting || !m_TextBubble.isFinished) return;

            if (IsDisplayingUI)
            {
                if (isUIDirty || wasWaiting)
                {
                    ProcessStoryChunk();
                    wasWaiting = false;
                }
                if (isUIDirty)
                {
                    UpdateTextGUI();
                    UpdateChoicesGUI();
                }
            } else
            {
                m_TextBubble.ShowWidget(false);
                choicesPanel.gameObject.SetActive(false);
            }
        }

        private void EraseChoices()
        {
            for (int i = 0; i < choicesPanel.transform.childCount; i++) {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }
        }

        private void UpdateTextGUI()
        {
            string text = m_NewTextToDisplay.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                if (!text.EndsWith("\n\n"))
                {
                    if (text.EndsWith("\n"))
                    {
                        text += "\n";
                    }
                }
                m_TextBubble.AddText(m_activeSpeaker, text);

                m_NewTextToDisplay.Clear();
            }
        }

        private void UpdateChoicesGUI() {
            if (!m_TextBubble.isFinished) return;

            if (m_Story.currentChoices.Count >= 1)
            {
                m_StoryDebugLog.AppendLine($"Available Choices:");
                for (int i = m_Story.currentChoices.Count -1; i >= 0; i--)
                {
                    m_StoryDebugLog.AppendLine($"\tOption {i}:  {m_Story.currentChoices[i].text}");

                    choicesPanel.gameObject.SetActive(true);
                    Choice choice = m_Story.currentChoices[i];
                    Button choiceButton = Instantiate(m_ChoiceButtonPrefab) as Button;
                    choiceButton.gameObject.transform.position = new Vector3(175, 900, 0);
                    choiceButton.gameObject.SetActive(true);
                    TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                    choiceText.text = m_Story.currentChoices[i].text;
                    choiceButton.transform.SetParent(choicesPanel.transform, false);

                    choiceButton.onClick.AddListener(delegate
                    {
                        m_StoryDebugLog.AppendLine($"Choice: {choice.text}");
                        ChooseStoryChoice(choice);
                    });

                    Vector3 pos = new Vector3(m_ButtonXOffset, m_ButtonYOffset * (i + 1), 0);
                    StartCoroutine(AnimateButtonPlacement(choiceButton.GetComponent<RectTransform>(), pos));
                }
            }

            isUIDirty = false;
        }

        IEnumerator AnimateButtonPlacement(RectTransform rect, Vector3 targetPos)
        {
            yield return new WaitForEndOfFrame();

            float time = 0;
            Vector3 startPos = rect.anchoredPosition;

            while (time < m_ButtonAnimationTime)
            {
                time += Time.deltaTime;
                rect.anchoredPosition =  Vector3.Lerp(startPos, targetPos, time / m_ButtonAnimationTime);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Set a actor to talk for a number of seconds.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="seconds"></param>
        void TalkFor(BaseActorController actor, float seconds)
        {
            //if (seconds < m_stopTalkingCue.layerWeightChangeTime) return;

            m_activeSpeaker.Prompt(m_startTalkingCue);
            m_activeSpeaker.brain.active = false;
            Invoke("StopTalking", seconds);
        }

        void StopTalking()
        {
            if (m_activeSpeaker == null) return;
            m_activeSpeaker.Prompt(m_stopTalkingCue);
            m_activeSpeaker.brain.active = isAIActive;
        }

        /// <summary>
        /// Called whenever the story needs to progress.
        /// </summary>
        /// <param name="choice">The choice made to progress the story.</param>
        void ChooseStoryChoice(Choice choice)
        {
            EraseChoices();
            m_Story.ChooseChoiceIndex(choice.index);
            m_NewTextToDisplay.Clear();
            isUIDirty = true;
        }

        /// <summary>
        /// Prompt an actor with a specific cue. Note that cues must be known to the InkManager by adding them to the Cues collection in the inspector.
        /// </summary>
        /// <param name="args">ACTOR_NAME CUE_NAME</param>
        void PromptCue(string[]args)
        {
            if (!ValidateArgumentCount(Direction.Cue, args, 2))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            ActorCue cue = FindCue(args[1].Trim());

            if (actor != null)
            {
                actor.Prompt(cue);
            }
        }

        /// <summary>
        /// The MoveTo direction instructs an actor to move to a specific location. It is up to the ActorController
        /// to decide how they should move. By default the story
        /// will wait for the actor to reach their mark before continuing. Add a NoWait parameter to allow the story to continue without waiting.
        /// </summary>
        /// <param name="args">ACTOR, LOCATION [, Wait|No Wait]</param>
        void MoveTo(string[] args)
        {
            if (!ValidateArgumentCount(Direction.MoveTo, args, 2, 3))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            if (actor == null) return;

            Transform target = FindTarget(args[1].Trim());
            if (target == null) return;

            actor.MoveTo(target);

            if (args.Length == 3)
            {
                string waitArg = args[2].ToLower().Trim();
                if (waitArg == "no wait")
                {
                    return;
                } else if (waitArg != "wait")
                {
                    Debug.LogError($"MoveTo instruction with arguments {string.Join(",", args)} has an invalid argument in posision 3. Valid values are 'Wait' and 'NoWait'. Falling back to the default of 'Wait'. Please correct the Ink Script.");
                }
            }
            WaitFor(new string[] { args[0], "ReachedTarget" });
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

            BaseActorController actor = FindActor(args[0].Trim());
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

            BaseActorController actor = FindActor(args[0].Trim());
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

            BaseActorController actor = FindActor(args[0].Trim());
            actor.StopMoving();
        }

        /// <summary>
        /// Set an animation parameter on an actor.
        ///
        /// </summary>
        /// <param name="args">[ActorName], [ParameterName], [Value] - if Value is missing it is assumed that the parameter is a trigger</param>
        void AnimationParam(string[] args)
        {
            Animator animator = null;

            if (!ValidateArgumentCount(Direction.AnimationParam, args, 2, 3))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim(), false);
            if (actor)
            {
                animator = actor.Animator;
            } else
            {
                Transform target = FindTarget(args[0].Trim());
                if (target)
                {
                    //OPTIMIZATION: Cache GetComponent Results
                    animator = target.GetComponent<Animator>();
                }
            }

            if (!animator)
            {
                Debug.LogError($"Got direction to set an AnimationParm ('{string.Join(", ", args)}') but could not find an animator on an object with the name '{args[0].Trim()}'.");
            }

            string paramName = args[1].Trim();

            if (args.Length == 2)
            {
                animator.SetTrigger(paramName);
                return;
            }

            string value = args[2].Trim();

            if (value == "False")
            {
                animator.SetBool(paramName, false);
                return;
            } else if (value == "True")
            {
                animator.SetBool(paramName, true);
                return;
            }

            float floatValue;
            if (float.TryParse(args[2].Trim(), out floatValue))
            {
                animator.SetFloat(paramName, floatValue);
                return;
            }

            Debug.LogError("Direction to set an animator value that is not a boolean or a float. Other types are not supported right now.");
        }

        /// <summary>
        /// Teleport immediately transfers a character from wherever they are to a specified 
        /// mark in the world. Their AI will be turned off under the assumption that they will
        /// be part of the current scene. If they are to act as background AI in the scene 
        /// then set the optional AI status parameter to "on".
        ///
        /// </summary>
        /// <param name="args">ACTOR_NAME, MARK_NAME[, AI_ON_OR_OFF]/param>
        void Teleport(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Camera, args, 1, 2))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            if (actor)
            {
                Transform mark = FindTarget(args[1].Trim());
                if (mark)
                {
                    bool active = false;
                    if (args.Length > 2) { 
                        if (args[2].Trim().ToLower() == "on")
                        {
                            active = true;
                        } else if (args[2].Trim().ToLower() == "off")
                        {
                            active = false;
                        } else
                        {
                            Debug.LogError($"Recieved direction `Teleport: {string.Join(", ", args)}`, however, the AI state flag of `{args[2].Trim()}` is unrecognized (should be `on` or `off`). Proceeding with AI off.");
                        }
                    }

                    actor.Teleport(mark, active);
                    actor.Prompt(m_stopTalkingCue);
                }
                else
                {
                    Debug.LogError($"Recieved direction `Teleport: {string.Join(", ", args)}`, however, no mark with the name `{args[1].Trim()}` was found.");
                }
            }
            else
            {
                Debug.LogError($"Recieved direction `Teleport: {string.Join(", ", args)}`, however, no actor with the name `{args[0].Trim()}` was found.");
            }
        }

        /// <summary>
        /// Enable or Disable an object
        ///
        /// </summary>
        /// <param name="args">OBJECT_NAME, TRUE_OR_FALSE/param>
        void Enable(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Camera, args, 2))
            {
                return;
            }
            
            Transform transform = FindTarget(args[0].Trim());
            if (transform)
            {
                if (args[1].Trim().ToLower() == "true")
                {
                    transform.gameObject.SetActive(true);
                } else if (args[1].Trim().ToLower() == "false")
                {
                    transform.gameObject.SetActive(false);
                } else
                {
                    Debug.LogError($"Recieved direction `Enable: {string.Join(", ", args)}`, however, only `true` or `false` values are allowed. `{args[1].Trim()}` was supplied. If this object is disabled on start you need to ensure that it is referenced in the `Cached Objects` section of the Ink Manager so that it can be discovered.");
                }
            }
            else
            {
                Debug.LogError($"Recieved direction `Enable: {string.Join(", ", args)}`, however, no object with the name `{args[0].Trim()}` was found.");
            }
        }

        /// <summary>
        /// Switch to a specific camera and optionally look at a named object.
        ///
        /// </summary>
        /// <param name="args">[CameraName] [FollowTargetName] [LookAtTargetName]</param>
        void Camera(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Camera, args, 1, 3))
            {
                return;
            }

            CinemachineVirtualCamera newCamera;
            Transform camera = FindTarget(args[0].Trim());
            if (camera)
            {
                newCamera = camera.gameObject.GetComponent<CinemachineVirtualCamera>();
                if (cinemachine.ActiveVirtualCamera != (ICinemachineCamera)newCamera) {
                    cinemachine.ActiveVirtualCamera.Priority = 10;


                    if (m_FadeCamera)
                    {
                        StartCoroutine(CrossFadeCamerasCo(newCamera));
                    }
                    else
                    {
                        newCamera.Priority = 99;
                    }
                }

                Transform objectName;
                if (args.Length >= 2)
                {
                    objectName = FindTarget(args[1].Trim());
                    if (objectName)
                    {
                        if (args.Length == 2)
                        {
                            newCamera.Follow = objectName;
                            newCamera.LookAt = objectName;
                        }
                        else
                        {
                            Transform childObject = FindChild(objectName, args[2].Trim());
                            if (childObject)
                            {
                                newCamera.Follow = childObject;
                                newCamera.LookAt = childObject;
                            } else
                            {
                                newCamera.Follow = objectName;
                                newCamera.LookAt = objectName;
                            }
                        }
                    }
                }
            } else
            {
                Debug.LogError($"Recieved direction to switch to camera called {args[0].Trim()}, however, no such camera could not be found: ");
            }
        }

        IEnumerator CrossFadeCamerasCo(CinemachineVirtualCamera newCamera)
        {
            m_FadeCamera.Priority = 99;
            yield return new WaitForSeconds(0.2f);

            while (cinemachine.IsBlending)
            {
                yield return new WaitForEndOfFrame();
            }

            m_FadeCamera.Priority = 10;
            newCamera.Priority = 99;
            yield return new WaitForSeconds(0.2f);
            while (cinemachine.IsBlending)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Play a specified music track. The tracks requested should be saved in
        /// `Resources/Music/MOODE_NAME.mp3`
        /// 
        /// </summary>
        /// <param name="args">MOOD, NAME[, LOOP_TRUE_OR_FALSE_DEFAULT_TRUE]</param>
        void Music(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Music, args, 2, 3))
            {
                return;
            }

            String path = "Music";
            String track = args[0].Trim() + "_" + args[1].Trim();
            AudioClip audio = Resources.Load<AudioClip>($"{path}/{track}");
            if (audio)
            {
                bool isLooping = true;
                if (args.Length == 3 && args[2].ToLower().Trim() == "false") {
                    isLooping = false;
                }

                if (m_MusicAudioSource.clip != audio)
                {
                    m_MusicAudioSource.clip = audio;
                    m_MusicAudioSource.loop = isLooping;
                    m_MusicAudioSource.Play();
                }
            }
            else
            {
                Debug.LogError($"There is a direction to play the music track '{path}/{track}' but no resource file of that name.");
            }
        }

        /// <summary>
        /// Play a specified sound effect. Effects requested should be saved in
        /// `Resources/Audio/TYPE/NAME`
        /// 
        /// SOURCE: is the game object from which the sound will be played (must have an AudioSource)
        /// TYPE: is an arbitrary FX type name
        /// NAME: is thename of the actual clip file
        /// LOOP: is 'true' if you want the sound to loop, this defaults to false. Any other value will be interpreted as false.
        /// 
        /// </summary>
        /// <param name="args">SOURCE, TYPE, NAME[, LOOP_TRUE_OR_FALSE_DEFAULT_FALSE]</param>
        void SoundFX(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Music, args, 3, 4))
            {
                return;
            }

            AudioSource source;
            Transform obj = FindTarget(args[0].Trim());
            if (!obj)
            {
                Debug.LogError($"Direction to play SoundFX with the arguments {string.Join(", ", args)} but no source object with the name {args[0].Trim()} can be found");
                return;
            } else
            {
                //OPTIMIZATION: cache audio source
                source = obj.GetComponentInChildren<AudioSource>();
                if (!source)
                {
                    Debug.LogError($"Direction to play SoundFX with the arguments {string.Join(", ", args)} but no audio source was found on the the object with the name name {args[0].Trim()}.");
                    return;
                }
            }

            String path = "Audio";
            String clip = args[1].Trim() + "/" + args[2].Trim();
            AudioClip audio = Resources.Load<AudioClip>($"{path}/{clip}");
            if (audio)
            {
                bool isLooping = false;
                if (args.Length == 4 && args[3].ToLower().Trim() == "true")
                {
                    isLooping = true;
                }

                if (source.clip != audio)
                {
                    source.clip = audio;
                    source.loop = isLooping;
                    source.Play();
                }
            }
            else
            {
                Debug.LogError($"There is a direction to play the soundFX '{path}/{clip}' but no resource file of that name.");
            }
        }

        /// <summary>
        /// Play an audio file from an identified AudioSource.
        /// Files should be stored in `Resources/Audio/...`.
        /// The audio will be played from an actor if specified, if none specified it will be assumed there is an audio source on the camera
        /// 
        /// </summary>
        /// <param name="args">
        /// An array of arguments as follows:
        /// 0: The filename, inclusing extenstion, of the audio file
        /// 1: [Optional] The name of the actor or object that contains the AudioSource from which to play this audio
        /// </param>
        void Audio(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Music, args, 1, 2))
            {
                return;
            }

            String path = "Audio/";
            String filename = args[0].Trim();
            AudioClip audio = Resources.Load<AudioClip>(path + filename);

            if (!audio)
            {
                Debug.LogError("There is a direction to play an audio file " + filename + " but there is no audio file of that name in `Resources/Audio/`.");
                return;
            }

            AudioSource source = null;
            if (args.Length == 1)
            {
                //TODO don't use Camera.main cache the result for performance. See Camera(...)
                source = UnityEngine.Camera.main.GetComponentInChildren<AudioSource>();
                if (source == null)
                {
                    Debug.LogError("There is a direction to play an audio file from the default audio source but there is no audio source on the main camera.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(args[1]))
            {
                BaseActorController actor = FindActor(args[1].Trim());
                if (actor != null)
                {
                    source = actor.GetComponentInChildren<AudioSource>();
                    if (source == null)
                    {
                        Debug.LogError("There is a direction to play an audio file from an actor " + actor.name + " but there is no AudioSource attached to it or its children.");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("There is a direction to play an audio file from an actor " + actor.name + " but there is no such actor.");
                    return;
                }
            }

            if (source == null)
            {
                Debug.LogError("There is a direction to play an audio file called " + filename + " but no audio source was specified or found.");
                return;
            }
            source.clip = audio;
            source.Play();
        }

        /// <summary>
        /// Wait for a particular game state. Supported states are:
        ///
        /// ReachedTarget - waits for the actor to have reached their move target
        /// [a float] - waits for a duration (in seconds)
        ///
        /// </summary>
        /// <param name="args">[Actor] [State]</param>
        void WaitFor(string[] args)
        {
            if (!ValidateArgumentCount(Direction.WaitFor, args, 1, 2))
            {
                return;
            }

            string param1 = args[0].Trim();
            bool isFloat = float.TryParse(param1, out float time);
            if (isFloat)
            {
                waitForStates.Add(new WaitForState(time));
            }
            else
            {
                waitForStates.Add(new WaitForState(FindActor(param1), args[1].Trim()));
            }
        }

        /// <summary>
        /// Turn to face, and continue to look at, a target or, if no target is provided, stop looking at a specific target.
        /// </summary>
        /// <param name="args">ACTOR_NAME, [TARGET_NAME | Nothing]</param>
        void TurnToFace(string[] args)
        {
            if (!ValidateArgumentCount(Direction.TurnToFace, args, 2))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            string targetName = args[1].Trim();
            Transform target = null;
            if (targetName.ToLower() != "nothing") {
                target = FindTarget(targetName);
            }

            if (target != null)
            {
                actor.gameObject.transform.LookAt(target.position);
                actor.LookAtTarget = target.transform;
            } else {
                actor.ResetLookAt();
            }
        }

        EmotionalState FindEmotionalState(BaseActorController actor)
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
            Transform obj = null;
            for (int i = 0; i < m_CachedObjects.Count; i++)
            {
                if (m_CachedObjects[i].name == objectName.Trim())
                {
                    return m_CachedObjects[i];
                }
            }

            BaseActorController actor = FindActor(objectName, false);
            if (actor != null)
            {
                return actor.transform.root;
            }

            //OPTIMIZATION Don't use Find at runtime. When initiating the InkManager we should pre-emptively parse all directions and cache the results in m_CachedObjects - or perhaps (since the story may be larger or dynamic) we should do it in a Coroutine just ahead of execution of the story chunk
            GameObject go = GameObject.Find(objectName);
            if (go)
            {
                m_CachedObjects.Add(go.transform);
                return go.transform;
            }
            else
            {
                Debug.LogError($"There is a direction that needs to operate on {objectName}, but the object cannot be found.");
                return null;
            }
        }

        /// <summary>
        /// Look through the known actors to see if we have one with the given name.
        /// </summary>
        /// <param name="actorName">The name of the actor we want.</param>
        /// <param name="logError">If true (the default) an error will be logged to the console if the actor is not found.</param>
        /// <returns>The actor with the given name or null if they cannot be found.</returns>
        private BaseActorController FindActor(string actorName, bool logError = true)
        {
            BaseActorController actor = null;
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
                Debug.LogError($"Ink script contains a direction for actor called '{actorName}`. However, the actor cannot be found.");
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


        StringBuilder m_StoryDebugLog = new StringBuilder();

        /// <summary>
        /// Grab the current story chunk and parse it for processing.
        /// </summary>
        void ProcessStoryChunk()
        {
            if (!m_Story.canContinue && !isWaiting)
            {
                if (m_Story.currentChoices.Count == 1)
                {
                    if (m_autoAdvanceSingleChoice)
                    {
                        m_StoryDebugLog.AppendLine($"Auto Choice: {m_Story.currentChoices[0]}");
                        m_Story.ChooseChoiceIndex(0);
                    }
                }
            }

            string line;
            while (m_TextBubble.isFinished && m_Story.canContinue && !isWaiting)
            {
                line = m_Story.Continue();

                // Process Directions;
                int cmdIdx = line.IndexOf(">>>");
                if (cmdIdx >= 0)
                {
                    m_NewTextToDisplay.Clear();

                    int startIdx = line.IndexOf(' ', cmdIdx);
                    int endIdx = line.IndexOf(':') - startIdx;
                    Enum.TryParse(line.Substring(startIdx, endIdx).Trim(), out Direction cmd);
                    string[] args = line.Substring(endIdx + startIdx + 1).Split(',');

                    m_StoryDebugLog.AppendLine($"Direction: {cmd}: {string.Join(", ", args)}");

                    switch (cmd)
                    {
                        case Direction.Unkown:
                            Debug.LogError("Unknown Direction: " + line);
                            break;
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
                        case Direction.Music:
                            Music(args);
                            break;
                        case Direction.SoundFX:
                            SoundFX(args);
                            break;
                        case Direction.WaitFor:
                            WaitFor(args);
                            break;
                        case Direction.Audio:
                            Audio(args);
                            break;
                        case Direction.AI:
                            AI(args);
                            break;
                        case Direction.Teleport:
                            Teleport(args);
                            break;
                        case Direction.Enable:
                            Enable(args);
                            break;
                        default:
                            Debug.LogError("Unknown Direction: " + line);
                            break;
                    }
                }
                // is it dialogue?
                else if (Regex.IsMatch(line, "^(\\w*:)|^(\\w*\\s\\w*:)", RegexOptions.IgnoreCase)) // we have an actors name
                {
                    int indexOfColon = line.IndexOf(":");
                    string speaker = line.Substring(0, indexOfColon).Trim();
                    string speech = line.Substring(indexOfColon + 1).Trim();

                    m_StoryDebugLog.AppendLine($"{speaker}: \"{speech}\"");

                    m_NewTextToDisplay.Clear();

                    m_activeSpeaker = FindActor(speaker);
                    
                    if (m_activeSpeaker)
                    {
                        TalkFor(m_activeSpeaker, speech.Length * m_ActiveTimePerCharacter);
                    }
                                        
                    m_NewTextToDisplay.Append(speech);
                    if (m_ActiveTimePerCharacter > 0)
                    {
                        WaitFor(new string[1] { $"{m_ActiveTimePerCharacter * speech.Length}" });
                    }

                    isUIDirty = true;
                }
                else // No named actor, so interpret it as narration or descriptive text
                {
                    m_StoryDebugLog.AppendLine($"Narration: {line}");

                    m_NewTextToDisplay.Clear();

                    m_activeSpeaker = null;
                    m_NewTextToDisplay.AppendLine(line);
                    if (m_ActiveTimePerCharacter > 0)
                    {
                        WaitFor(new string[1] { $"{m_ActiveTimePerCharacter * line.Length}" });
                    }

                    isUIDirty = true;
                }
            }
        }

        /// <summary>
        /// Places an actor under, or removes an actor from being under AI control. When an actor with an AI brain is under AI control the brain will be able to influence the actors actions. If AI control is OFF then Ink scripts (or another script) have full control over the actor. If AI is on you can still influence what the actor will do with directions, but once a direction is completed the AI brain will take over immediately.
        /// </summary>
        /// <param name="args">ACTOR_NAME On|Off</param>
        void AI(string[] args)
        {
            ValidateArgumentCount(Direction.AI, args, 2);
            
            BaseActorController actor = FindActor(args[0].Trim());
            if (!actor.brain)
            {
                Debug.LogError($"There is directive to turn {actor}'s AI {args[1]}. However there no brain was found on that actor.");
                return;
            }


            if (args[1].Trim().ToLower() == "on")
            {
                isAIActive = true;
                actor.brain.active = true;
            }
            else if (args[1].Trim().ToLower() == "off")
            {
                isAIActive = false;
                actor.brain.active = false;
            } else
            {
                Debug.LogError($"AI direction has incorrect parameters of {string.Join(", ", args)}");
            }

        }

        /// <summary>
        /// Sets the player control to either On or Off. If on then the story will not progress until it is renabled
        /// with a call to `SetPlayerControl(false)`
        /// </summary>
        /// <param name="args">On or Off</param>
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

        /// <summary>
        /// Searches recursively for a child.
        /// </summary>
        /// <param name="name">The name of the child to search for.</param>
        /// <returns>The transform of the child object with the given name, if it exists, otherwise null.</returns>
        public Transform FindChild(Transform parent, string name)
        {
            Transform[] transforms = parent.gameObject.GetComponentsInChildren<Transform>();

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].gameObject.name == name)
                {
                    return transforms[i];
                }
            }

            return null;
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
                if (!string.IsNullOrEmpty(warning))
                {
                    msg = warning + msg;
                    Debug.LogWarning(msg);
                }
                return false;
            } else if (!string.IsNullOrEmpty(warning))
            {
                msg = warning + msg;
                Debug.LogWarning(msg);
            }

            return true;
        }

        #region External Utility Functions
        void BindExternalFunctions()
        {
            m_Story.BindExternalFunction("ConvertToSpaced", (string value) =>
            {
                return value.Replace('_', ' ');
            });
        }
        #endregion
    }

class WaitForState
    {
        public enum WaitType { ReachTarget, Time }
        public BaseActorController actor;
        // TODO: make this an enum
        public WaitType waitType;
        public float endTime = float.NegativeInfinity;

        public WaitForState(float duration)
        {
            waitType = WaitType.Time;
            this.endTime = Time.timeSinceLevelLoad + duration;
        }

        public WaitForState(BaseActorController actor, string waitForState)
        {
            this.actor = actor;
            waitType = WaitType.ReachTarget;
        }
    }

}
#endif