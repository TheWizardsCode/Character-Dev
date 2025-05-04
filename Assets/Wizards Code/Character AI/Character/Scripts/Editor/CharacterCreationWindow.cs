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
        private int selectedProfileIndex;

        private bool showControllerEditor = false;
        private bool showAnimatorEditor = false;
        private bool showColliderEditor = false;
        private bool showBrainEditor;
        private bool showBehvaioursEditor;
        private bool showCameraAwarenessEditor;
        private AICharacterProfile characterProfile;
        private bool showAppearanceEditor;

        [MenuItem("Tools/Wizards Code/AI/Character Creation")]
        public static void ShowWindow()
        {
            GetWindow<CharacterCreationWindow>("Character Creation");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Creation", EditorStyles.boldLabel);
            
            characterName = EditorGUILayout.TextField("Name", characterName);
            if (string.IsNullOrEmpty(characterName))
            {
                EditorGUILayout.HelpBox("Please enter a name for the character prefab.", MessageType.Warning);
                return;
            }

            characterModel = EditorGUILayout.ObjectField("Character Model", characterModel, typeof(GameObject), true) as GameObject;
            if (characterModel == null)
            {
                EditorGUILayout.HelpBox("Please select a character model.", MessageType.Warning);
                return;
            }

            characterProfile = EditorGUILayout.ObjectField("Character Profile", characterProfile, typeof(AICharacterProfile), true) as AICharacterProfile;
            if (characterProfile == null)
            {
                EditorGUILayout.HelpBox("Select a character profile.", MessageType.Warning);
                return;
            }

            if (configurationScene == null || !configurationScene.isLoaded)
            {
                EditorGUILayout.HelpBox("Configuration scene is not open. To create this AI open the configuration scene by clicking the button below.", MessageType.Warning);

                if (GUILayout.Button("Open Configuration Scene"))
                {
                    OpenConfigurationScene();
                }
                return;
            }

            GUILayout.Label("Steps:", EditorStyles.boldLabel);

            configScrollPos = EditorGUILayout.BeginScrollView(configScrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 200));
            {
                OnAppearanceGUI();
                OnActorControllerGUI();
                OnAnimatorGUI();
                OnCollidersGUI();
                OnBrainGUI();
                OnBehavioursGUI();
                OnCameraAwarenessGUI();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Test Character"))
            {
                EditorApplication.isPlaying = true;
                CameraAwareness cameraAwareness = character.GetComponent<CameraAwareness>();
                cameraAwareness.CanCameraFollowTarget = true;
                cameraAwareness.SetAsFollowTarget();
            }

            GUI.enabled = true;

            if (GUILayout.Button("Save as Prefab"))
            {
                SaveAsPrefab();
            }

            if (GUILayout.Button("Restore Original Scene"))
            {
                RestoreOriginalScene();
            }
        }

        private void OnAppearanceGUI()
        {
            ActorAppearance appearance = character.GetComponent<ActorAppearance>();
            if (appearance == null)
            {
                appearance = character.AddComponent<ActorAppearance>();
            }

            SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length == 0)
            {
                EditorGUILayout.HelpBox("No skinned mesh renderers found on the character.", MessageType.Warning);
                return;
            }

            bool isValid = appearance != null;
            string foldoutLabel =  isValid ? "✔ Appearance" : "✘ Appearance";
            showAppearanceEditor = EditorGUILayout.Foldout(showAppearanceEditor, foldoutLabel, GetFoldoutStyle(isValid));
            if (!showAppearanceEditor)
            {
                return;
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

                Editor editor = Editor.CreateEditor(appearance);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnActorControllerGUI()
        {   
            BaseActorController actorController = character.GetComponent<BaseActorController>();
            if (actorController == null)
            {
                actorController = character.AddComponent<AnimatorActorController>();
                characterProfile.ConfigureActorController(actorController);
            }

            string foldoutLabel = actorController != null ? "✔ Actor Controller" : "✘ Actor Controller";
            showControllerEditor = EditorGUILayout.Foldout(showControllerEditor, foldoutLabel, GetFoldoutStyle(actorController != null));
            if (!showControllerEditor)
            {
                return;
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
        }

        private void OnAnimatorGUI()
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

            isValid &= animator != null;
            string foldoutLabel = isValid ? "✔ Animator" : "✘ Animator";
            showAnimatorEditor = EditorGUILayout.Foldout(showAnimatorEditor, foldoutLabel, GetFoldoutStyle(animator != null));
            if (!showAnimatorEditor)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(animator);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnCollidersGUI()
        {
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

            string foldoutLabel = collider != null ? "✔ Collider" : "✘ Collider";
            showColliderEditor = EditorGUILayout.Foldout(showColliderEditor, foldoutLabel, GetFoldoutStyle(collider != null));
            if (!showColliderEditor)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(collider);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnBrainGUI()
        {
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

            string foldoutLabel = brain != null ? "✔ Brain" : "✘ Brain";
            showBrainEditor = EditorGUILayout.Foldout(showBrainEditor, foldoutLabel, GetFoldoutStyle(brain != null));
            if (!showBrainEditor)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(brain);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnBehavioursGUI()
        {
            AbstractAIBehaviour[] behaviours = character.GetComponentsInChildren<AbstractAIBehaviour>();
            if (behaviours.Length == 0)
            {   
                GameObject behaviourGO = new GameObject("Base Behaviours");
                behaviourGO.transform.SetParent(character.transform);
                behaviourGO.transform.localPosition = Vector3.zero;

                behaviourGO.AddComponent<Wander>();       

                behaviours = character.GetComponentsInChildren<AbstractAIBehaviour>();
            }

            bool isValid = behaviours.Length > 0 && behaviours.All(b => b != null);
            string foldoutLabel = isValid ? "✔ Behaviours" : "✘ Behaviours";
            showBehvaioursEditor = EditorGUILayout.Foldout(showBehvaioursEditor, foldoutLabel, GetFoldoutStyle(isValid));
            if (!showBehvaioursEditor)
            {
                return;
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
        }

        private void OnCameraAwarenessGUI()
        {
            CameraAwareness cameraAwareness = character.GetComponent<CameraAwareness>();
            if (cameraAwareness == null)
            {
                cameraAwareness = character.AddComponent<CameraAwareness>();
            }

            string foldoutLabel = cameraAwareness != null ? "✔ Camera Awareness" : "✘ Camera Awareness";
            showCameraAwarenessEditor = EditorGUILayout.Foldout(showCameraAwarenessEditor, foldoutLabel, GetFoldoutStyle(cameraAwareness != null));
            if (!showCameraAwarenessEditor)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                Editor editor = Editor.CreateEditor(cameraAwareness);
                editor.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();
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
