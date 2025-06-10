using System;
using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode.Character
{
    /// <summary>
    /// CharacterCreatorController is not used at runtime, when running in the Player the component will delete itself.
    /// It is used to flag a character as having been created by the Character Creator.
    /// This allows the prefab to be opened in the Character Creator and edited.
    /// </summary>
    public class CharacterCreatorController : MonoBehaviour
    {
        [SerializeField, Tooltip("The grounder parent, this is the transform that contains the grounder IK objects.")]
        public Transform grounderParent;

        public CharacterCreatorToDoItem[] appearanceToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Set main mesh renderer.", isComplete = false },
            new CharacterCreatorToDoItem { description = "If there are optional meshes that you don't want visible on start then disable them now. Note, if these same meshes are added to the additive or unique items list they may still be enabled by the system on start.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Ensure model is centered.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set material options - if the model has multiple base materials that give a different look, set them in the rendering section.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set additive mesh renderers - if the model has additive meshes (can all be enabled at the same time) that give a different look when they are turned on, set them in the rendering section.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set unique item mesh renderers - if the model has unique meshes (only one can be enabled at any one time) that give a different look when they are turned on, set them in the rendering section.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] navMeshToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Set height and radius.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set angular speed.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set acceleration.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set stopping distance.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set Quality of navigation. Generally, this should be set to High for player characters and characters, Good for NPCs and Medium for animals.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set priority. Higher is less important. Player characters should be 1. NPCs should be between 10 and 50, animals between 51 and 99.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] animatorToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Set animator controller.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set avatar.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Validate whether you want to animate physics. Generally, for background characters, this will be false. For players and possibly Hero characters this might be true.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Enable and configure Grounder if required. Note the grounder component needs careful setup, every model is different. Even the same model at a different scale may require customization. In addition, grounding is a relatively expensive operation. The Grounder will usually only be applied to Hero characters.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] colliderToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Generate and Validate collider, or skip if collider(s) are already present on the model.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] actorControllerToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Max speed.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set the min run distance - this is the distance at which the character will start running.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set the min sprint distance - this is the distance at which the character will start sprinting.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set the walk speed factor - a % of the max speed that represents normal walk speed.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set the run speed factor - a % of the max speed that represents normal run speed.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Set arriving distance.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Ensure root motion is set correctly for your animations.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] brainToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Configure the display, or not, of the behaviour icon", isComplete = false },
        };

        public CharacterCreatorToDoItem[] behavioursToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Review added behaviours and add any additional ones required.", isComplete = false },
            new CharacterCreatorToDoItem { description = "All behaviours must have a Display Name and a Description.", isComplete = false },
        };

        public CharacterCreatorToDoItem[] cameraAwarenessToDoList = new CharacterCreatorToDoItem[]
        {
            new CharacterCreatorToDoItem { description = "Set the distance the animal can be from a camera without triggering the camera awareness routines. Generally you want this to be longer for larger or interesting characters and shorter for background characters.", isComplete = false },
            new CharacterCreatorToDoItem { description = "Should the camera be able to follow this character?", isComplete = false },
        };

        void Awake()
        {
    #if !UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(this);
            }
    #endif
        }
    }

    [Serializable]
    public struct CharacterCreatorToDoItem
    {
        public string description;
        public bool isComplete;
    }
}