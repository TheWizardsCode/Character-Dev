#if INK_PRESENT
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
using System.Globalization;
using UnityEngine.Serialization;

using System.Text.RegularExpressions;

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
            //TODO, document this direction
            Music,
            //TODO remove this as a preferred command, way too long, merge with "Music"?
            SetPrimaryBlendedMusicTrack,
            WaitFor,
            Audio
        }

        [Header("Script")]
        [SerializeField, Tooltip("The Ink file to work with.")]
        TextAsset m_InkJSON;
        [SerializeField, Tooltip("The actors that are available in this scene.")]
        BaseActorController[] m_Actors;
        [SerializeField, Tooltip("The cues that will be used in this scene.")]
        ActorCue[] m_Cues;
        [SerializeField, Tooltip("Should the story start as soon as the game starts. If this is set to false the story will not start until a trigger or similar is set.")]
        bool m_PlayOnAwake = true;

        [Header("Camera, Lights and Sound")]
        [SerializeField, Tooltip("The Cinemachine Brain used to control the virtual cameras.")]
        CinemachineBrain cinemachine;
        [SerializeField, Tooltip("The audio source for music playback.")]
        AudioSource m_MusicAudioSource;

        [SerializeField, Tooltip("The audio source for music playback.")]
        GlobalMusicController m_GlobalMusicComp;

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
        [SerializeField, Tooltip("Dialogue and narration bubble controller that will display the text for the player.")]
        [FormerlySerializedAs("m_TextBubbleComp")]
        TextBubbleController m_TextBubble;
        [SerializeField, Tooltip("Should the UI always pause for player input, even when there are no choices to be made?")]
        bool m_AlwaysWaitForPlayer = true;


        Story m_Story;
        bool m_IsUIDirty = false;
        StringBuilder m_NewStoryText = new StringBuilder();
        string m_CurrentSpeakerName = "";

        //TODO REFACTOR managing the waiting for directions has become unwieldly. There are too many combinations of variables with unclear names
        bool wasWaiting = false;
        private BaseActorController m_WaitingForActor;
        private string m_WaitingForState = "";
        private float m_WaitUntilTime = float.NegativeInfinity;

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
        }

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

        private bool IsWaitingFor
        {
            get
            {
                if (m_WaitingForActor == null && m_WaitUntilTime < 0 && m_WaitingForState == "")
                {
                    return false;
                }

                // todo - handle the case where several waiting states are active simultaneously
                switch (m_WaitingForState)
                {
                    case "ReachedTarget":
                        if (m_WaitingForActor.IsMoving)
                        {
                            wasWaiting = false;
                            return true;
                        }
                        else
                        {
                            ExitWaitingState();
                            return false;
                        }
                    case "Time":
                        if (Time.timeSinceLevelLoad >= m_WaitUntilTime)
                        {
                            ExitWaitingState();
                            return false;
                        } else
                        {
                            return true;
                        }
                    case "PlayerInput":
                        return true; // as of writing, pressing space will automatically exit the waiting state
                    default:
                        Debug.LogError("Direction to wait gives a unrecognized state to wait for: '" + m_WaitingForState + "'");
                        return false;
                }
            }
        }

        private void ExitWaitingState()
        {
            m_WaitingForActor = null;
            m_WaitingForState = "";
            m_WaitUntilTime = float.NegativeInfinity;
            wasWaiting = true;
        }

        public void Update()
        {
            if (IsWaitingFor)
            {
                if (m_WaitingForState == "PlayerInput" &&
                        (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0)))
                {
                    ExitWaitingState();
                }
                return;
            }

            if (IsDisplayingUI)
            {
                if (m_IsUIDirty || wasWaiting || !m_AlwaysWaitForPlayer)
                {
                    ProcessStoryChunk();
                    UpdateGUI();
                    wasWaiting = false;
                }
            } else
            {
                m_TextBubble.ShowWidget(false);
                choicesPanel.gameObject.SetActive(false);
            }
        }

        private void EraseUI()
        {
            for (int i = 0; i < choicesPanel.transform.childCount; i++) {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }

            m_TextBubble.ClearText();
        }

        private void UpdateGUI()
        {
            EraseUI();

            m_TextBubble.SetText(m_CurrentSpeakerName, m_NewStoryText.ToString(), true);

            if (m_AlwaysWaitForPlayer || m_Story.currentChoices.Count > 1)
            {
                for (int i = 0; i < m_Story.currentChoices.Count; i++)
                {
                    if (m_WaitingForState == "PlayerInput") m_WaitingForState = "";

                    choicesPanel.gameObject.SetActive(true);
                    Choice choice = m_Story.currentChoices[i];
                    Button choiceButton = Instantiate(m_ChoiceButtonPrefab) as Button;
                    choiceButton.gameObject.SetActive(true);
                    TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                    choiceText.text = m_Story.currentChoices[i].text;
                    choiceButton.transform.SetParent(choicesPanel.transform, false);

                    choiceButton.onClick.AddListener(delegate
                    {
                        ChooseStoryChoice(choice);
                    });
                }
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

            BaseActorController actor = FindActor(args[0].Trim());
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

            BaseActorController actor = FindActor(args[0].Trim());
            if (actor == null) return;

            Transform target = FindTarget(args[1].Trim());
            if (target == null) return;

            actor.MoveTo(target);

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
        /// <param name="args">[ActorName] [ParameterName] [Value] - if Value is missing it is assumed that the parameter is a trigger</param>
        void AnimationParam(string[] args)
        {
            if (!ValidateArgumentCount(Direction.AnimationParam, args, 2, 3))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            string paramName = args[1].Trim();

            if (args.Length == 2)
            {
                actor.Animator.SetTrigger(paramName);
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
                    cinemachine.ActiveVirtualCamera.Priority = 1;
                    newCamera.Priority = 99;
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

        /// <summary>
        /// Play a specified music track. The tracks requested should be saved in
        /// `/Resources/Music/TEMPO_STYLE.mp3`
        /// 
        /// </summary>
        /// <param name="args">[Tempo] [Style]</param>
        void Music(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Music, args, 2))
            {
                return;
            }

            String path = "Music/";
            String track = args[0].Trim() + "_" + args[1].Trim();
            AudioClip audio = Resources.Load<AudioClip>(path + track);
            if (audio)
            {
                m_MusicAudioSource.clip = audio;
                m_MusicAudioSource.Play();
            }
            else
            {
                Debug.LogError("There is a direction to play the music track " + track + " but no file of that name is found in `Resources/Audio`");
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
        /// Play a specified music track, identified by name. The tracks
        /// are blended reactively
        ///
        /// </summary>
        /// <param name="args">[Tempo] [Style]</param>
        void SetPrimaryBlendedMusicTrack(string[] args)
        {
            if (!ValidateArgumentCount(Direction.Music, args, 1))
            {
                return;
            }

            if (m_GlobalMusicComp == null) return;

            string trackAsString = args[0].Trim();
            EMusicTrackName trackNameAsEnum;
            // convert string to enum
            switch (trackAsString)
            {
            case "Main":
                trackNameAsEnum = EMusicTrackName.MTN_Main;
                Debug.LogWarning("main track is always playing. Perhaps you meant another track?");
                break;
            case "Fun":
                trackNameAsEnum = EMusicTrackName.MTN_Fun;
                break;
            case "Investigation":
                trackNameAsEnum = EMusicTrackName.MTN_Investigation;
                break;
            case "Suspense":
                trackNameAsEnum = EMusicTrackName.MTN_Suspense;
                break;
            default:
                Debug.LogError("Direction to play music track cannot be satisfied: " + trackAsString);
                return;
            }

            m_GlobalMusicComp.SetPrimaryTrack(trackNameAsEnum);
        }

        /// <summary>
        /// Wait for a particular game state. Supported states are:
        ///
        /// ReachedTarget - waits for the actor to have reached their move target
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
                m_WaitingForState = "Time";
                m_WaitUntilTime = Time.timeSinceLevelLoad + time;
            }
            else
            {
                m_WaitingForActor = FindActor(param1);
                m_WaitingForState = args[1].Trim();
            }

        }

        void TurnToFace(string[] args)
        {
            if (!ValidateArgumentCount(Direction.TurnToFace, args, 2))
            {
                return;
            }

            BaseActorController actor = FindActor(args[0].Trim());
            string targetName = args[1].Trim();
            Transform target = null;
            if (targetName != "Nothing") {
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
            BaseActorController actor = FindActor(objectName, false);
            if (actor != null)
            {
                return actor.transform.root;
            }

            //OPTIMIZATION Don't use Find at runtime. When initiating the InkManager we should pre-emptively parse all directions and cache the results - or perhaps (since the story may be larger or dynamic) we should do it in a CoRoutine just ahead of execution of the story chunk
            GameObject go = GameObject.Find(objectName);
            if (go)
            {
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
            
            if (!m_Story.canContinue && !IsWaitingFor && !m_AlwaysWaitForPlayer && m_Story.currentChoices.Count > 0)
            {
                m_Story.ChooseChoiceIndex(0);
            }

            while (m_Story.canContinue && !IsWaitingFor)
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
                        case Direction.SetPrimaryBlendedMusicTrack:
                            SetPrimaryBlendedMusicTrack(args);
                            break;
                        case Direction.WaitFor:
                            WaitFor(args);
                            break;
                        case Direction.Audio:
                            Audio(args);
                            break;
                        default:
                            Debug.LogError("Unknown Direction: " + line);
                            break;
                    }
                }
                // is it dialogue?
                else if (Regex.IsMatch(line, "^(\\w*:)|^(\\w*\\s\\w*:)", RegexOptions.IgnoreCase))
                {
                    // regex above will match "Bestie: hi" or "Bus Driver: sit down!", and "techie:nice camera". warning: only allows a single space!
                    int indexOfColon = line.IndexOf(":");
                    m_CurrentSpeakerName = line.Substring(0, indexOfColon).Trim();
                    m_NewStoryText.Clear();
                    m_NewStoryText.Append(line.Substring(indexOfColon + 1).Trim());

                    // require player input to continue, unless a choice pops up
                    if (m_AlwaysWaitForPlayer)
                    {
                        m_WaitingForState = "PlayerInput";
                    }
                }
                // interpret it as narration or descriptive text
                else
                {
                    //m_NewStoryText.Clear();
                    m_CurrentSpeakerName = "";
                    m_NewStoryText.AppendLine(line);
                }

                // Process Tags
                // currently unused
                // List<string> tags = m_Story.currentTags;
                // for (int i = 0; i < tags.Count; i++)
                // {
                // }
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
#endif