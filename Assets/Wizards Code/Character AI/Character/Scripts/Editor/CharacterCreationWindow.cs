using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Linq;
using WizardsCode.Stats;
using UnityEditor.SceneTemplate;
using System;

namespace WizardsCode.Character {

    public class CharacterCreationWindow : EditorWindow
    {
        private GameObject characterModel;
        private string characterName;
        private Scene configurationScene;
        private string originalScenePath;
        private GameObject character;
        private Vector2 configScrollPos;

        private bool showControllerEditor = false;
        private bool showAnimatorEditor = false;
        private bool showNavMeshEditor = false;
        private bool showColliderEditor = false;
        private bool showBrainEditor;
        private bool showBehvaioursEditor;
        private bool showCameraAwarenessEditor;
        [Tooltip("A profile for the character that will be used for default settings. This, in conjunction with the template prefab can be used to minimize the amount of manual configuration required. This profile defines defaults for some settings, while the template prefab, if provided, provides precise values to be copied into the character. That is the profile usually defines a class of character while the template defines specific settings for a type of character.")]
        private AICharacterProfile characterProfile;
        [Tooltip("An Actor Controller prefab from which settings can optionally be copied. This, in conjunction with the profile can be used to minimize the amount of manual configuration required. This template defines specific settings that can, optionally, be copied into your character. The profile defines defaults that the character will initially be set to. That is the profile usually defines a class of character while the template defines specific settings for a type of character.")]
        private BaseActorController templatePrefab;
        private bool showAppearanceEditor;

        private string[] appearanceToDoList = new string[]
        {
            "Set main mesh renderer.",
            "Ensure model is centered.",
            "Set material options - if the model has multiple base materials that give a different look, set them in the rendering section.",
            "Set additive mesh renderers - if the model has additive meshes that give a different look when they are turned on, set them in the rendering section.",
        };
        private bool[] appearanceToDoListStatus = new bool[]
        {
            false,
            false
        };

        private string[] navMeshToDoList = new string[]
        {
            "Set height and radius.",
            "Set angular speed.",
            "Set acceleration.",
        };

        private bool[] navMeshToDoListStatus = new bool[]
        {
            false,
            false,
            false
        };

        private string[] animatorToDoList = new string[]
        {
            "Set animator controller.",
            "Set avatar.",
            "Validate whether you want to animate physics.",
        };
        private bool[] animatorToDoListStatus = new bool[]
        {
            false,
            false,
            false
        };

        private string[] colliderToDoList = new string[]
        {
            "Validate automatically generated collider.",
        };
        private bool[] colliderToDoListStatus = new bool[]
        {
            false,
        };

        private string[] actorControllerToDoList = new string[]
        {   
            "Max speed.",
            "Set the min run distance - this is the distance at which the character will start running.",
            "Set the min sprint distance - this is the distance at which the character will start sprinting.",
            "Set the walk speed factor - a % of the max speed that represents normal walk speed.",
            "Set the run speed factor - a % of the max speed that represents normal run speed.",
            "Set arriving distance.",
            "Ensure root motion is set correctly for your animations.",
        };
        private bool[] actorControllerToDoListStatus = new bool[]
        {
        };

        private string[] brainToDoList = new string[]
        {
            "Do you want this character to display a behaviour icon?",
        };
        private bool[] brainToDoListStatus = new bool[]
        {
        };

        private string[] behavioursToDoList = new string[]
        {
            "Add behaviours to the character.",
        };
        private bool[] behavioursToDoListStatus = new bool[]
        {
            false,
        };

        private string[] cameraAwarenessToDoList = new string[]
        {
            "Should the camera be able to follow this character?",
        };
        private bool[] cameraAwarenessToDoListStatus = new bool[]
        {
        };

        [MenuItem("Tools/Wizards Code/AI/Character Creation")]
        public static void ShowWindow()
        {
            GetWindow<CharacterCreationWindow>("Character Creation");
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Character Creation", EditorStyles.boldLabel);

                if (GUILayout.Button("Close and Restore Original Scene"))
                {
                    RestoreOriginalScene();
                }
            }
            GUILayout.EndHorizontal();
            characterName = EditorGUILayout.TextField("Name", characterName);
            if (string.IsNullOrEmpty(characterName))
            {
                EditorGUILayout.HelpBox("Please enter a name for the character prefab.", MessageType.Warning);
                return;
            }

            GameObject originalModel = characterModel;
            characterModel = EditorGUILayout.ObjectField("Character Model", characterModel, typeof(GameObject), true) as GameObject;
            if (characterModel == null)
            {
                EditorGUILayout.HelpBox("Please select a character model.", MessageType.Warning);
                return;
            }
            else if (originalModel != characterModel)
            {
                appearanceToDoListStatus = new bool[appearanceToDoList.Length];
                navMeshToDoListStatus = new bool[navMeshToDoList.Length];
                animatorToDoListStatus = new bool[animatorToDoList.Length];
                colliderToDoListStatus = new bool[colliderToDoList.Length];
                actorControllerToDoListStatus = new bool[actorControllerToDoList.Length];
                brainToDoListStatus = new bool[brainToDoList.Length];
                behavioursToDoListStatus = new bool[behavioursToDoList.Length];
                cameraAwarenessToDoListStatus = new bool[cameraAwarenessToDoList.Length];
                characterName = characterModel.name;
            }

            EditorGUILayout.BeginHorizontal();
            {
                characterProfile = EditorGUILayout.ObjectField("Character Profile", characterProfile, typeof(AICharacterProfile), true) as AICharacterProfile;
                if (characterProfile != null)
                {
                    if (GUILayout.Button("Duplicate Profile"))
                    {
                        AICharacterProfile newProfile = DuplicateProfile();
                        if (newProfile != null)
                        {
                            characterProfile = newProfile;
                        }
                    }
                }
                else if (GUILayout.Button("Create Profile"))
                {
                    characterProfile = CreateInstance<AICharacterProfile>();
                    string path = EditorUtility.SaveFilePanelInProject("Save Character Profile", "New AI Character Profile", "asset", "Specify a location to save the profile.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(characterProfile, path);
                        AssetDatabase.SaveAssets();
                        Selection.activeObject = characterProfile;
                        EditorGUIUtility.PingObject(characterProfile);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a character profile.", MessageType.Warning);
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                templatePrefab = EditorGUILayout.ObjectField("Template Prefab", templatePrefab, typeof(BaseActorController), true) as BaseActorController;
            }
            EditorGUILayout.EndHorizontal();

            if (configurationScene == null || !configurationScene.isLoaded)
            {
                EditorGUILayout.HelpBox("Configuration scene is not open. To create this AI open the configuration scene by clicking the button below.", MessageType.Warning);

                if (GUILayout.Button("Open Configuration Scene"))
                {
                    OpenConfigurationScene();
                }
                return;
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Stop Test"))
                {
                    EditorApplication.isPlaying = false;
                }
            }
            else if (GUILayout.Button("Test Character"))
            {
                EditorApplication.isPlaying = true;
                CameraAwareness cameraAwareness = character.GetComponent<CameraAwareness>();
                cameraAwareness.CanCameraFollowTarget = true;
                cameraAwareness.SetAsFollowTarget();
            }

            GUILayout.Label("Steps:", EditorStyles.boldLabel);

            configScrollPos = EditorGUILayout.BeginScrollView(configScrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 200));
            {
                // Appearance
                bool isValid = OnAppearanceGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the appearance step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Actor Controller
                isValid = OnActorControllerGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Actor Controller step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // NavMeshAgent
                isValid = OnNavMeshGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Nav Mesh Agent step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Animator
                isValid = OnAnimatorGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Animator step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Colliders
                isValid = OnCollidersGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Collider step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Brain
                isValid = OnBrainGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Brain step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Behaviours
                isValid = OnBehavioursGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Behaviours step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }

                // Camera Awareness
                isValid = OnCameraAwarenessGUI();
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("Complete the todo items in the Camera Awareness step.", MessageType.Error);
                    EditorGUILayout.EndScrollView();
                    return;
                }
            }
            EditorGUILayout.EndScrollView();

            GUI.enabled = true;

            if (GUILayout.Button("Save as Prefab"))
            {
                SaveAsPrefab();
            }
        }

        private AICharacterProfile DuplicateProfile()
        {
            AICharacterProfile duplicateProfile = null;
            string currentPath = AssetDatabase.GetAssetPath(characterProfile);
            string defaultFolder = string.IsNullOrEmpty(currentPath) ? "Assets" : System.IO.Path.GetDirectoryName(currentPath);

            string newPath = EditorUtility.SaveFilePanelInProject("Duplicate Character Profile", characterProfile.name + " Copy", "asset", "Specify a location to save the duplicated profile.", defaultFolder);
            if (!string.IsNullOrEmpty(newPath))
            {
                duplicateProfile = Instantiate(characterProfile);
                AssetDatabase.CreateAsset(duplicateProfile, newPath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = duplicateProfile;
                EditorGUIUtility.PingObject(duplicateProfile);
            }

            return duplicateProfile;
        }

        private bool OnAppearanceGUI()
        {
            bool isValid = true;

            ActorAppearance appearance = character.GetComponent<ActorAppearance>();
            if (appearance == null)
            {
                appearance = character.AddComponent<ActorAppearance>();
            }

            SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length == 0)
            {
                EditorGUILayout.HelpBox("No skinned mesh renderers found on the character.", MessageType.Warning);
                return false;
            }

            isValid = ToDoListGUI(appearanceToDoList, ref appearanceToDoListStatus);

            isValid &= appearance != null;
            string foldoutLabel = isValid ? "✔ Appearance" : "✘ Appearance";
            showAppearanceEditor |= !isValid;
            showAppearanceEditor = EditorGUILayout.Foldout(showAppearanceEditor, foldoutLabel, GetFoldoutStyle(isValid));
            if (!showAppearanceEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                SkinnedMeshRenderer selectedRenderer = appearance.MainMeshRenderer;
                string[] rendererNames = renderers.Select(r => r.name).ToArray();
                int selectedIndex = Array.IndexOf(renderers, selectedRenderer);
                selectedIndex = EditorGUILayout.Popup("Main Mesh Renderer", selectedIndex, rendererNames);
                if (selectedIndex >= 0 && selectedIndex < renderers.Length)
                {
                    appearance.MainMeshRenderer = renderers[selectedIndex];
                }
                
                ActorAppearance template = templatePrefab?.GetComponent<ActorAppearance>();
                if (template != null && GUILayout.Button("Copy Body Materials from Template"))
                {
                    if (appearance.BodyMaterials.Length < template.BodyMaterials.Length)
                    {
                        appearance.BodyMaterials = new Material[template.BodyMaterials.Length];
                    }
                    template.BodyMaterials.CopyTo(appearance.BodyMaterials, 0);
                }

                Editor editor = Editor.CreateEditor(appearance);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool ToDoListGUI(string[] todoList, ref bool[] status)
        {
            bool isValid = true;

            if (todoList.Length != status.Length)
            {
                Array.Resize(ref status, todoList.Length);
            }

            for (int i = 0; i < todoList.Length; i++)
            {
                if (!status[i])
                {
                    status[i] = EditorGUILayout.ToggleLeft(todoList[i], status[i]);
                    isValid &= false;
                }
            }

            return isValid;
        }

        private bool OnActorControllerGUI()
        {   
            bool isValid = true;

            BaseActorController actorController = character.GetComponent<BaseActorController>();
            if (actorController == null)
            {
                actorController = character.AddComponent<AnimatorActorController>();
                characterProfile.ConfigureActorController(actorController);
            }

            isValid = ToDoListGUI(actorControllerToDoList, ref actorControllerToDoListStatus);

            string foldoutLabel = actorController != null ? "✔ Actor Controller" : "✘ Actor Controller";
            showControllerEditor |= !isValid;
            showControllerEditor = EditorGUILayout.Foldout(showControllerEditor, foldoutLabel, GetFoldoutStyle(actorController != null));
            if (!showControllerEditor)
            {
                return isValid;
            }
            
            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(actorController);
                editor.OnInspectorGUI();
                if (GUILayout.Button("Reset to Profile Defaults"))
                {
                    characterProfile.ConfigureActorController(actorController);
                }
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnNavMeshGUI()
        {
            bool isValid = true;

            NavMeshAgent navMeshAgent = character.GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                navMeshAgent = character.AddComponent<NavMeshAgent>();
            }

            isValid = ToDoListGUI(navMeshToDoList, ref navMeshToDoListStatus);

            isValid &= navMeshAgent != null;
            string foldoutLabel =  isValid ? "✔ NavMesh Agent" : "✘ NavMesh Agent";
            showNavMeshEditor |= !isValid;
            showNavMeshEditor = EditorGUILayout.Foldout(showNavMeshEditor, foldoutLabel, GetFoldoutStyle(isValid));
            if (!showNavMeshEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(navMeshAgent);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnAnimatorGUI()
        {
            bool isValid = true;

            Animator animator = character.GetComponent<Animator>();
            if (animator == null)
            {
                animator = character.AddComponent<Animator>();
            }

            Animator[] otherAnimators = character.GetComponentsInChildren<Animator>().Where(a => a != animator).ToArray();

            if (animator.runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox("Animator does not have a controller assigned.", MessageType.Warning);
                isValid = false;
            }

            if (animator.avatar == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Animator does not have an avatar assigned.", MessageType.Warning);
                if (otherAnimators.Length > 0)
                {
                    if (GUILayout.Button("Grab Avatar from Other Animator"))
                    {
                        animator.avatar = otherAnimators[0].avatar;
                    }
                }
                EditorGUILayout.EndHorizontal();
                isValid = false;
            }

            if (otherAnimators.Length > 0)
            {
                string otherAnimatorPaths = string.Join(", ", otherAnimators
                    .Where(a => a != animator)
                    .Select(a => a.gameObject.name));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"Unwanted animators found on '{otherAnimatorPaths}'", MessageType.Error);
                if (GUILayout.Button("Ping Unwanted Animator"))
                {
                    EditorGUIUtility.PingObject(otherAnimators[0].gameObject);
                }
                if (GUILayout.Button("Remove Unwanted Animator"))
                {
                    DestroyImmediate(otherAnimators[0], true);
                }
                EditorGUILayout.EndHorizontal();
                isValid = false;
            }

            isValid &= ToDoListGUI(animatorToDoList, ref animatorToDoListStatus);

            isValid &= animator != null;
            string foldoutLabel = isValid ? "✔ Animator" : "✘ Animator";
            showAnimatorEditor |= !isValid;
            showAnimatorEditor = EditorGUILayout.Foldout(showAnimatorEditor, foldoutLabel, GetFoldoutStyle(animator != null));
            if (!showAnimatorEditor)
            {
                return isValid;
            }
                
            Animator template = templatePrefab?.GetComponent<Animator>();
            if (template != null && GUILayout.Button("Copy Controller from Template"))
            {
                animator.runtimeAnimatorController = template.runtimeAnimatorController;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(animator);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnCollidersGUI()
        {
            bool isValid = true;

            Collider collider = character.GetComponent<Collider>();
            if (collider == null)
            {
                Bounds totalBounds = new Bounds(character.transform.position, Vector3.zero);
                SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    totalBounds.Encapsulate(renderer.bounds);
                }

                if (characterProfile.colliderType == AICharacterProfile.ColliderType.Capsule)
                {
                    collider = character.AddComponent<CapsuleCollider>();
                    CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                    if (capsuleCollider != null)
                    {
                        capsuleCollider.center = totalBounds.center - character.transform.position;
                        capsuleCollider.height = totalBounds.size.y;
                        capsuleCollider.radius = Mathf.Max(totalBounds.size.x, totalBounds.size.z) / 2;
                    }
                }
                else if (characterProfile.colliderType == AICharacterProfile.ColliderType.Box)
                {
                    collider = character.AddComponent<BoxCollider>();
                    BoxCollider boxCollider = collider as BoxCollider;
                    if (boxCollider != null)
                    {
                        boxCollider.center = totalBounds.center - character.transform.position;
                        boxCollider.size = totalBounds.size;
                    }
                }
                else if (characterProfile.colliderType == AICharacterProfile.ColliderType.Sphere)
                {
                    collider = character.AddComponent<SphereCollider>();
                    SphereCollider sphereCollider = collider as SphereCollider;
                    if (sphereCollider != null)
                    {
                        sphereCollider.center = totalBounds.center - character.transform.position;
                        sphereCollider.radius = Mathf.Max(totalBounds.size.x, totalBounds.size.y, totalBounds.size.z) / 2;
                    }
                }
            }

            isValid = ToDoListGUI(colliderToDoList, ref colliderToDoListStatus);

            isValid &= collider != null;
            string foldoutLabel = isValid ? "✔ Collider" : "✘ Collider";
            showColliderEditor |= !isValid;
            showColliderEditor = EditorGUILayout.Foldout(showColliderEditor, foldoutLabel, GetFoldoutStyle(collider != null));
            if (!showColliderEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(collider);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnBrainGUI()
        {
            bool isValid = true;

            Brain brain = character.GetComponentInChildren<Brain>();
            if (brain == null)
            {   
                    GameObject brainGO = new GameObject("Brain");
                    brainGO.transform.SetParent(character.transform);
                    brainGO.transform.localPosition = Vector3.zero;

                    brain = brainGO.AddComponent<Brain>();

                    GameObject behaviourIcon = new GameObject("Behaviour Icon");
                    behaviourIcon.transform.SetParent(brain.transform);
                    behaviourIcon.transform.localPosition = new Vector3(0, 2.4f, 0);
                    brain.Icon = behaviourIcon.AddComponent<SpriteRenderer>();
            }

            isValid = ToDoListGUI(brainToDoList, ref brainToDoListStatus);

            isValid &= brain != null;
            string foldoutLabel = isValid ? "✔ Brain" : "✘ Brain";
            showBrainEditor |= !isValid;
            showBrainEditor = EditorGUILayout.Foldout(showBrainEditor, foldoutLabel, GetFoldoutStyle(brain != null));
            if (!showBrainEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(brain);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnBehavioursGUI()
        {
            bool isValid = true;

            AbstractAIBehaviour[] behaviours = character.GetComponentsInChildren<AbstractAIBehaviour>();
            if (behaviours.Length == 0)
            {   
                GameObject behaviourGO = new GameObject("Base Behaviours");
                behaviourGO.transform.SetParent(character.transform);
                behaviourGO.transform.localPosition = Vector3.zero;

                behaviourGO.AddComponent<Wander>();       

                behaviours = character.GetComponentsInChildren<AbstractAIBehaviour>();
            }

            isValid = ToDoListGUI(behavioursToDoList, ref behavioursToDoListStatus);

            isValid &= behaviours.Length > 0 && behaviours.All(b => b != null);
            string foldoutLabel = isValid ? "✔ Behaviours" : "✘ Behaviours";
            showBehvaioursEditor |= !isValid;
            showBehvaioursEditor = EditorGUILayout.Foldout(showBehvaioursEditor, foldoutLabel, GetFoldoutStyle(isValid));
            if (!showBehvaioursEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                foreach (var behaviour in behaviours)
                {
                    if (behaviour != null)
                    {
                        Editor editor = Editor.CreateEditor(behaviour);
                        editor.OnInspectorGUI();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }

        private bool OnCameraAwarenessGUI()
        {
            bool isValid = true;

            CameraAwareness cameraAwareness = character.GetComponent<CameraAwareness>();
            if (cameraAwareness == null)
            {
                cameraAwareness = character.AddComponent<CameraAwareness>();
            }

            isValid = ToDoListGUI(cameraAwarenessToDoList, ref cameraAwarenessToDoListStatus);

            isValid &= cameraAwareness != null;
            string foldoutLabel = isValid ? "✔ Camera Awareness" : "✘ Camera Awareness";
            showCameraAwarenessEditor = EditorGUILayout.Foldout(showCameraAwarenessEditor, foldoutLabel, GetFoldoutStyle(cameraAwareness != null));
            if (!showCameraAwarenessEditor)
            {
                return isValid;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(cameraAwareness);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            return isValid;
        }
        private GUIStyle GetFoldoutStyle(bool isComplete)
        {
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.normal.textColor = isComplete ? Color.green : Color.red;
            foldoutStyle.onNormal.textColor = isComplete ? Color.green : Color.red;
            return foldoutStyle;
        }

        private void SaveAsPrefab()
        {
            if (character != null)
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Character Prefab", characterName, "prefab", "Specify a location to save the prefab.");
                if (!string.IsNullOrEmpty(path))
                {
                    PrefabUtility.SaveAsPrefabAsset(character, path);
                    Debug.Log("Character saved as prefab at: " + path);
                }
            }
            else
            {
                Debug.LogWarning("No character selected to save as prefab.");
            }
        }

        private void OpenConfigurationScene()
        {
            originalScenePath = SceneManager.GetActiveScene().path;
            if (SceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            string templateScenePath = "Assets/Character-Dev/Assets/Wizards Code/Character AI/Scenes/Development Scenes/Character Configuration Scene Template.scenetemplate";
            SceneTemplateAsset templateAsset = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templateScenePath);
            if (templateAsset == null)
            {
                Debug.LogError("Template asset not found at path: " + templateScenePath);
                return;
            }

            var templateInstance = SceneTemplateService.Instantiate(templateAsset, false);
            if (templateInstance == null)
            {
                Debug.LogError("Failed to instantiate the scene template.");
                return;
            }

            configurationScene = templateInstance.scene;
            
            if (characterModel != null)
            {
                character = new GameObject(characterName);
                GameObject model = Instantiate(characterModel, character.transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.name = characterModel.name + " (Model)";

                Camera sceneCamera = Camera.main;
                sceneCamera.transform.position = character.transform.position + new Vector3(1, 2, 5);
                sceneCamera.transform.LookAt(character.transform.position + Vector3.up * 1.5f);

                SceneView.lastActiveSceneView.pivot = sceneCamera.transform.position + sceneCamera.transform.forward * 5;
                SceneView.lastActiveSceneView.rotation = sceneCamera.transform.rotation;
                SceneView.lastActiveSceneView.Repaint();
            }
            else
            {
                Debug.LogWarning("No character selected to instantiate in the temporary scene.");
            }
        }

        private void RestoreOriginalScene()
        {
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                if (character != null && EditorUtility.IsDirty(character))
                {
                    bool savePrefab = EditorUtility.DisplayDialog("Save Prefab", "Do you want to save the character as a prefab before restoring the original scene?", "Yes", "No");
                    if (savePrefab)
                    {
                        SaveAsPrefab();
                    }
                }

                EditorSceneManager.OpenScene(originalScenePath);
                Debug.Log("Original scene restored.");
            }
        }
    }
}
