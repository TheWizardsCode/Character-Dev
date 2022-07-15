Welcome to the Wizards Code Character System: Ink Integration

The Character System helps game authors create Non Player Characters (Actors) in their game worlds. These characters can be interacted with or they can simply populate the world as background characters.

When interacting they will talk to the player and potentially carry out actions. Conversations are driven by Ink and open source scripting language for writing interactive narrative. In fact this text is an Ink narrative. As such you can navigate through it as if it were a <i>choose your own adventure</i> game.

What would you like to learn more about?

-> Top_Knot

== Top_Knot

* Talking with Actors -> Talking_To_Actors
* Directing Actors to take actions -> Actor_Direction
* Quit -> DONE
    
== Talking_To_Actors

Dialog is provided to Actors via an Ink script. This is made available to the character system via an API. Using the data in the script the Character System can present appropriate lines of text in any way that fits the game, such as text on the screen, voice actor audio and graphics or text to speech.

After each piece of dialog the player can be presented with a series of choices.

-> Top_Knot

== Actor_Direction

Actions are handled by direction in the narrative. An direction takes the following form:

`>>> Keyword: [Parameter1], [Parameter2]`

`Keyword` is a keyword that tells the character system what direction this is.
`[Paramameter1] [Paramameter2]` are 1, 2 or more parameters that the direction needs. What these parameters are varies with different Directions. See the documentation for each Direction to learn more.

* SetEmotion -> SetEmotion
* Cue -> Cue
* MoveTo -> MoveTo
* Camera -> Camera
* Action -> Action
* WaitFor -> WaitFor
* Home -> Top_Knot

= Cue

A `Cue` direction instructs the character system to load an `ActorCue` and prompt an actor with it. This reuses the `ActorCue` system from with the Character System.

`>>> Cue: [ActorName], [CueName]`

`Cue` is a keyword that tells the character system that it should cue an actor
`[ActorName]` must be replaced by the name of the actor that will recieve the Cue. The actor must be known to the `InkManager`
`[CueName]` must be replaced by the name of the Cue that the actor should carry out. The cue must be known to the Ink manager in the current scene.

Cues are `ActorCues` in the Character System and as such provide a way of defining a number of items in a single Scriptable Object (e.g. movement, sound, animation and more), or even a chain of actions. They afford a significant amount of control over how the actor interprets the direction from the script. An alternative approach is to use a direction that allows the actor more control over how they interpret the direction. For example, you can use the `MoveTo` direction instead of a `Cue` direction.

* [Cue Joe - who will run onto set] -> Joes_Entrance_Cue
* Home -> Top_Knot

= MoveTo

The `MoveTo` direction tell an actor to move to a specific place in the environment. How the character gets there is managed by the character itself. The `MoveTo` direction takes the following form:

`>>> MoveTo: [ActorName], [MarkName]

`MoveTo` is a keyword that tells the character system that it should tell an actor to move
`[ActorName]` must be replaced by the name of the actor that will recieve the Direction. The actor must be known to the `InkManager`
`[MarkName]` Is the name of a mark in the scene. This is the name of any Game Object that can be reached by the actor.

* [Have Joe move to Mark2] -> Joes_MoveTo
* Home -> Top_Knot

= Joes_MoveTo
>>> MoveTo: Joe, Mark2

If you previously cued Joe they will move from Mark1 to Mark2. If you haven't yet brought Joe into the scene with the `Cue` direction then hw will move directly from offset to Mark2.

* Home -> Top_Knot

= Joes_Entrance_Cue
>>> Cue: Joe, WalkToMark1

The Ink manager has cued Joe to enter the scene and move to Mark 1 which is an invisible game object in the scene. The Actor Controller on the Joe actor model will instruct the AI to move Joe to that mark. You will see him walk onto set any moment now.

-> Top_Knot

= SetEmotion

The `SetEmotion` direction will set the value of an emotion being tracked on a character. The emotional values are used to influence how an Actor carries out other directions. For example, if Fear is high and the character is not in combat then they may crouch when moving in order to avoid being seen by whatever they are scared of.

The SetEmotion direction takes the following form:

>>> SetEmotion: [ActorName], [EmotionName], [Float]

-> Top_Knot

= Camera

`>>> Camera: [CameraName], [ObjectParent], [ChildObjectName optional]`

Switch to the named camera and set the camera Follow and LookAt parameters to the  ChildObjectName found under the ObjectParent. If ChildObjectName is not present then the Object Parent is used. If the `ObjectParent` is not present then no change to the Target parameter of the camera will be made. 

-> Top_Knot

= AnimationParam

Set animator parameter on a given actor to a given value. If the parameter is a trigger then there will be no value.

`>>> AnimationParam: [ActorName] [ParameterName] [Value]` 

-> Top_Knot

= WaitFor

    Wait for a given state to occur.
    
    `>>> WaitFor: [Actor], ReachedTarget` Wait for the actor to reach its current destingation before proceeding.

    `>>> WaitFor: [Seconds]` Wait for the indicated number of seconds (expressed as a float) to pass.

-> Top_Knot

= Action

Fire a behaviour on the actor with the name and an optional interactable the behaviour will be carried out with.

`>>> Action: [ActorName], [BehaviourName], [InteractableName]

-> Top_Knot

TODO: Document >>> TurnToFace: ActorName, Object - Turn the actor to face the target and place the LookAt transform on the objeect. If Object is set to "Nothing" then the actor will look directly ahead of themselves.


TODO: Document >>> PlayerControl: On | Off (Gives the player control over the main character, prevents the story proceeding until the player trips a trigger or interacts with something)

TODO: Document `>>> StopMoving: ActorName` Stop all movement.

TODO: Document EXTERNAL GetPartyNoticability()

TODO: Document `>>> Music: [Tempo], [Style]` - play a music track. The track should be stored in `Resources/Music/Tempo_Style.mp3` (or other accepted Unity audio format)