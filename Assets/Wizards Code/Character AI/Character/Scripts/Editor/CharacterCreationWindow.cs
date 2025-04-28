using UnityEngine;

namespace WizardsCode.Character {
    using UnityEditor;

    public class CharacterCreationWindow : EditorWindow
    {
        private GameObject character;
        private string characterName;

        [MenuItem("Tools/Wizards Code/AI/Character Creation")]
        public static void ShowWindow()
        {
            GetWindow<CharacterCreationWindow>("Character Creation");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Creation", EditorStyles.boldLabel);

            characterName = EditorGUILayout.TextField("Name", characterName);
            character = (GameObject)EditorGUILayout.ObjectField("Character", character, typeof(GameObject), true);

            GUILayout.Label("Steps:", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Actor Controller"))
            {
                Debug.Log("Add Actor Controller button clicked");
            }

            if (GUILayout.Button("Configure Animator"))
            {
                // Copy Assets/Character-Dev/Assets/Wizards Code/Character AI/Animation/Animations/Controllers/Humanoid Controller (Override This).controller into the character directory using the name "characterName + "Animation Controller.controller"
                // Set Animator Controller to this controller
            }

            if (GUILayout.Button("Configure NavMesh Agent"))
            {
                // Radius 0.5, Height 2, Priority 10, Base Offset -0.8, Speed 3, Angular Speed 120, Acceleration 8, stopping distance 0.2, Auto braking true, Auto Traverse Off Mesh Link true, Auto Repath = true
            }

            if (GUILayout.Button("Add Capsule Collider"))
            {
                // Center 0, 0.8, 0
                // Radius 0.4, Height 1.8
            }

            if (GUILayout.Button("Add Rigid Body"))
            {
                // Mass 62
                // is Kinematic true
            }

            if (GUILayout.Button("Add Look At Target as Child Object"))
            {
                // Add a child to the character called `{name} Look At Target`
                // set position to character 0, height - 0.3f, 1
            }

            if (GUILayout.Button("Add Brain as Child Object"))
            {
                // Add Child object called brain
                // Add Brain component to child object
                // Add Behaviour Icon as child of brain, position 0, 2.4, 0
                // Add Sprite Renderer component to Behaviour Icon
                // Set Brain.IconUI to Behaviour Icon
            }

            if (GUILayout.Button("Create Basic Behaviours"))
            {
                // Create Basic Behaviours child object under brain
                // Add Wander with Intents component to Basic Behaviours
                // Set FallbackBehaviour in Brain to the Wander component
                // Set Icon to Assets/Character-Dev/Assets/Wizards Code/Character AI/Character/Sprites/twemoji/Wandering - Person Walking Dark Skin Tone - 1f6b6-1f3ff.png
            }

            if (GUILayout.Button("Test Character"))
            {
                Debug.Log("Run the scene check character is wandering");
            }

            if (GUILayout.Button("Save as Prefab"))
            {
                if (character != null)
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Character Prefab", character.name, "prefab", "Specify a location to save the prefab.");
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
        }
    }
}
