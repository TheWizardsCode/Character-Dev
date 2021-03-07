Welcome to the Wizards Code Character System.

The Character System helps game authors create Non Player Characters (Actors) in their game worlds. These characters can be interacted with or they can simply populate the world as background characters.

When interacting they will talk to the player and potentially carry out actions. Conversations are driven by Ink and open source scripting language for writing interactive narrative. In fact this text is an Ink narrative. As such you can navigate through it as if it were a <i>choose your own adventure</i> game.

What would you like to learn more about?

-> Top_Knot

== Top_Knot

* Talking with Actors -> Talking_To_Actors
* Telling Actors to take actions -> Actor_Actions
* Quit -> DONE
    
== Talking_To_Actors

Dialog is provided to Actors via an Ink script. This is made available to the character system via an API. Using the data in the script the Character System can present appropriate lines of text in any way that fits the game, such as text on the screen, voice actor audio and graphics or text to speech.

After each piece of dialog the player can be presented with a series of choices.

-> Top_Knot

== Actor_Actions

Actions are handled by tags in the narrative. An action tag takes the following form:

`>>> Cue: [ActorName], [CueName]`

`Cue` is a keyword that tells the character system that it should cue an actor
`[ActorName]` must be replaced by the name of the actor that will recieve the Cue. The actor must be known to the `InkManager`
`[CueName]` must be replaced by the name of the Cue that the actor should carry out. The cue must be known to the Ink manager in the current scene.

* [Cue Joe] -> Joes_Entrance
* Home -> Top_Knot

== Joes_Entrance
>>> Cue: Joe, WalkToMark1
The Ink manager has cued Joe to enter the scene and move to Mark 1 which is an invisible game object in the scene. The Actor Controller on the Joe actor model will instruct the AI to move Joe to that mark. You will see him walk onto set any moment now.

-> Top_Knot

TODO: Document >>> TurnToFace: ActorName, Object
TODO: Document >>> PlayerControl: On | Off