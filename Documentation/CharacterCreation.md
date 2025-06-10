# Character Creation

## Description
The `CharacterCreationWindow` is a custom Unity Editor window for creating, configuring, and saving AI character prefabs for the Wizards Code Character AI system. It provides a guided, step-by-step workflow to ensure all required character components are set up correctly, supporting both new and existing prefabs.

## Usage
- Open the window from the Unity menu: **Tools > Wizards Code > AI > Character Creation**.
- Enter a unique character name 
- Add a model or prefab to use as the base. If you add a model then a new prefab will be created using this model; if you add a prefab then the existing prefab will be edited.
- Assign or create an `AICharacterProfile` to define default settings. Optionally, assign a `BaseActorController` template prefab for copying specific settings.
- Click **Open Configuration Scene** to launch a dedicated setup scene. The selected model and configuration tools will be instantiated here.
- Complete each configuration step in the scrollable interface:
  - **To-Do List Guidance**: Each configuration step includes a to-do list designed to guide you through the required and recommended actions for that step. You must check off each item as you complete it, or if you decide that a particular step is not needed for your character. This ensures that all critical configuration tasks are either completed or consciously skipped, helping you avoid missing important setup details.
  - **Editor Helpers**: Some to-do items include helper buttons or tools in the editor that enable semi-automatic configuration. For example, in the Animator step, the editor can automatically copy the avatar from a duplicate or template if one is found, reducing manual setup. Look for helper buttons or actions next to to-do items to speed up configuration.
  - **Component Editors**: Each configuration step provides a dedicated editor interface for the relevant components, allowing you to easily modify settings. These are, for the most part, the same editors that are in the inspector. You can usually edit values in the inspector, or the window.
  - **Appearance**: Assign main mesh renderer, copy body materials, and configure appearance settings.
  - **Actor Controller**: Set up the main controller and reset to profile defaults if needed.
  - **NavMesh Agent**: Configure navigation agent settings.
  - **Animator**: Assign controllers and avatars, remove unwanted animators, and set up grounders. Use the provided helper to copy the avatar from a duplicate or template if available.
  - **Colliders**: Generate and configure colliders based on the character profile.
  - **Brain**: Add and configure the character's brain and behaviour icon.
  - **Behaviours**: Add and configure AI behaviours (e.g., Wander).
  - **Camera Awareness**: Add and configure camera awareness components.
- Each step must be completed before saving the prefab. The to-do list, along with editor helpers, is intended to help you track your progress and ensure a thorough and efficient configuration process.
- Enter Play mode to test the character in the configuration scene. Use the **Stop Test** button to exit Play mode.
- Click **Save as Prefab** to save the configured character. If editing an existing prefab, changes are applied to the original; otherwise, specify a save location for the new prefab.
- Use **Close and Restore Original Scene** to return to your previous scene. You will be prompted to save the character as a prefab if there are unsaved changes.

## Examples
- Creating a new AI character from a model: Assign a model, create a new profile, follow each configuration step, and save as a prefab.
- Editing an existing prefab: Select a prefab with a `WizardsCodeCharacter` component, update settings, and apply changes.

## Tips
- Use the foldout sections to expand/collapse each configuration step.
- Warnings and errors are displayed if required fields or steps are incomplete.
- The window supports copying settings from templates and profiles to speed up character creation.
- All changes are undoable using Unity's undo system.

## Requirements
- Unity Editor
- Wizards Code Character AI system and dependencies
- Properly set up configuration scene template and required assets

## Troubleshooting
- If you encounter errors or missing components, follow the on-screen help boxes and complete all required steps.
- Ensure all required assets (profiles, templates, models) are available in your project.
- Check the Unity Console for detailed error messages if something does not work as expected.

## See Also
- `AICharacterProfile` documentation
- `BaseActorController` documentation
- Wizards Code Character AI system manual

## Developer Notes
- `CharacterCreationWindow.s` implements the customer Unity Editor window for character creation.
- ToDo items for each step are defined in `CharacterCreatorController.cs`.
