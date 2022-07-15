[Ink](https://www.inklestudios.com/ink/) is a narrative scripting language. This code uses Ink stories within Unity games to control the development of a narrative.

We add a number of `Directions` that can be embedded into an Ink script, these instruct the engine to do something specific at that point. For example, we can instuct an actor to move to a specific place in the scene, change the camera angle or play a specified piece of music. For a full list of directions see below.

# Quick Start

We assume that you have a script already and want to integrate it into your game environment. 

The system supports speaker names in the form of `Speaker: Spoken words.` Although it is not required it is a good idea to have your speaker names be the same as the
actor Game Objects in the scene. This allows the Ink manager to discover those objects and control them, such as preventing the actors brain from choosing an alternative action until after a narrative section has passed.

## Unity Setup

1. Switch on the new Input System `Edit -> Project Settings -> Player -> Active Input Handling`
2. Install Text Mesh pro
3. Add an `AudioSource` component to your main camera
4. Install The [Ink Unity integration](https://github.com/inkle/ink-unity-integration).
5. Add 'INK_PRESENT' to your script defines (`Edit -> Project Settings -> Player -> Scripting Defines Symbols`)
5. [If you want to control the camera from Ink] Install Cinemachine and add at least one virtual camera `GameObject => Create -> Cinemahchine -> Virtual Camera`

## Scene Setup for Ink

1. Add `Packages/Wizards Code - Character/Ink/Prefabs/Ink Canvas` prefab
2. Add `Packages/Wizards Code - Character/Ink/Prefabs/Ink manager` component to a suitable manager object
3. Drag your script JSON into the `Ink JSON` field\
4. Drag your Cinemachine camera brain into the `Cinemachine` parameter
5. Drag your camera (with audio source) into the `Music Audio Source` parameter

# Directions

Directions are embedded instructions in the Ink story that tell the game engine to perform some action. For example, we can instuct an actor to move to a specific place in the scene, change the camera angle or play a specified piece of music. This section contains details of each of the directions.

Directions are inserted with the following syntax:

```
>>> DIRECTION_NAME: PARAM1, PARAM2, ...
```

Note that actors and cues that are to be used need to be setup in the `InkManager` before the game starts.

## Cue

Prompt an actor with a specific cue. Note that cues must be known to the InkManager by adding them to the Cues collection in the inspector.

```
>>> Cue: ACTOR_NAME CUE_NAME
```

## PlayerControl

Sets the player control to either On or Off. If on then the Ink story will not progress and no UI will be shown until it is renabled
with a call to `SetPlayerControl(false)` in the API. Typically this would happen when a player takes a particular action in the game.

```
>>> PlayerControl: [ON_OR_OFF]
```

## Action

Tell an actor to prioritize a particular behaviour. Under normal circumstances
this behaviour will be executed as soon as possible, as long as the necessary
preconditions have been met and no higher priority item exists.

```
>>> Action: ACTOR_NAME, BEHAVIOUR_NAME
```

## AI

Places an actor under, or removes an actor from being under AI control. When an actor with an AI brain is under AI control the brain will be able to influence the actors actions. If AI control is OFF then Ink scripts (or another script) have full control over the actor. If AI is on you can still influence what the actor will do with directions, but once a direction is completed the AI brain will take over immediately.

That is, when AI control is ON directions in the script will take precedence over the AI Brain, but when AI control is off the AI brain can take actions.

```
>>> AI: ACTOR_NAME, [ON|OFF]
```

## AnimationParam

Set an animation parameter on an actor. 

```
>>> AnimationParam: ACTOR_NAME PARAMETER_NAME VALUE_IF_NOT_TRIGGER
```

## Audio

## Camera

Switch to a specific camera and optionally look at a named object.

```
>>> Camera: CAMERA_NAME[, LOOK_AT_TARGET_NAME]
```

## MoveTo 

Move and actor to specific location. It is up to the ActorController to decide how they should get there.

By default the story will wait for the actor to reach their mark before continuing.
Add a 'No Wait' parameter in position 3 to allow the story to continue without waiting.

```
>>> MoveTo: ACTOR_NAME, LOCATION_Name [, Wait|No Wait]
```

### Examples of MoveTo

Move the Actor named `Pat` to a location named `Mark1`, and pause the story until they arrive.

```
>>> MoveTo: Pat, Mark1
```

Identical to above, but with an explicit wait instruction. This serves no technical purpose but can add readability.

```
>>> MoveTo: Pat, Mark1, Wait
```

Move the Actor named `Pat` to a location named `Mark1`, but allow the story to continue while they move.

```
>>> MoveTo: Pay, Mark1, NoWait
```

## Music

## SetEmotion

## StopMoving

## TurnToFace

Turn to face, and continue to look at, a target or, if no target is provided, stop looking at a specific target.

```
>>> TurnToDace: ACTOR_NAME, [TARGET_NAME | Nothing]
```

## WaitFor

Wait for a particular game state. The actor will decide on how to get to the location.

Supported wait states are:
  * ReachedTarget - wait until an actor has finished moving to a target
  * Time - wait a specified amount of time

```
>>> WaitFor: ACTOR_NAME, LOCATION_Name [, Wait|No Wait]
```

### Examples of WaitFor command

#### ReachedTarget
Waits for the actor to have reached their current move target. 
If the target is changed during this wait cycle this wait will end when reaching the new target.

```
>>> WaitFor: ACTOR_NAME, ReachedTarget
```

#### Time
Waits for a duration (in seconds)

```
>>> WaitFor: 8
```


