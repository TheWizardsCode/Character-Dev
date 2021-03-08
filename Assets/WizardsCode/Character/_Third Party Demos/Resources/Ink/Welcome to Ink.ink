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

* [Cue] -> Cue
* [MoveTo] -> MoveTo
* Home -> Top_Knot

= Cue

A `Cue` direction instructs the character system to load an `ActorCue` and prompt an actor with it. This reuses the `ActorCue` system from with the Character System.

`>>> Cue: [ActorName], [CueName]`

`Cue` is a keyword that tells the character system that it should cue an actor
`[ActorName]` must be replaced by the name of the actor that will recieve the Cue. The actor must be known to the `InkManager`
`[CueName]` must be replaced by the name of the Cue that the actor should carry out. The cue must be known to the Ink manager in the current scene.

Cues are `ActorCues` in the Character System and as such provide a way of defining a number of items in a single Scriptable Object (e.g. movement, sound, animation and more), or even a chain of actions. They afford a significant amount of control over how the actor interprets the direction from the script. An alternative approach is to use a direction that allows the actor more control over how they interpret the direction. For example, you can use the `MoveTo` direction instead of a `Cue` direction.

* [Cue Joe - who will run onto set] -> Joes_Entrance
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

= Joes_Entrance
>>> Cue: Joe, WalkToMark1

The Ink manager has cued Joe to enter the scene and move to Mark 1 which is an invisible game object in the scene. The Actor Controller on the Joe actor model will instruct the AI to move Joe to that mark. You will see him walk onto set any moment now.

-> Top_Knot

TODO: Document >>> TurnToFace: ActorName, Object
TODO: Document >>> PlayerControl: On | Off (Gives the player control over the main character, prevents the story proceeding until the player trips a trigger or interacts with something)