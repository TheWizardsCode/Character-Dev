So you want to use Ink to drive dialog with your actors. No problem. The steps are simple:

-> Top_Knot

= Top_Knot

* [How should I set up the scene?] -> Configure_Scene
* [What does the Ink file look like?] -> Ink_File
* [Is that it?] -> Play_Scene

= Configure_Scene

Actor: Your scene must have at least one Wizard Character within it. It's outside the scope of this part of the tutorials to show you how. If you don't know how to do this then go check that out first.

Actor: You will also need to add an `InkManage` component and a UI for displaying the dialog. There is an `Ink Manager` prefab in the integration package that will allow you to quickly add a functioning setup to your scene. Or just copy this scene to get started.

-> Top_Knot

= Ink_File

Actor: The ink file you use is a standard Ink file. Any text you have in that file will display on the speech interface. If you want a specific character to speak the words then add `CharacterName:` before the text they will speak.

Actor: The `CharacterName` needs to be the same as the name given to the character game object in your scene.

Actor: When you have created your Ink file ensure that it is configured in your `InkManager`.

-> Top_Knot

= Play_Scene

Actor: That's it, just hit play.

-> END