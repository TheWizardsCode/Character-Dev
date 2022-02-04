# Wizards Code Character System

The Wizards Code character system provides an ambient AI system for your game characters. Using this system your characters will come to life. This document will get you going quickly. For more in depth tutorials see the demo scenese in `Characters/Scenes`.

# Basic Setup

1. Add `Animator Actor Controller`
2. Drag the `Prefabs/AI/Brain` prefab onto the character root
3. Create an `Animator Override Controller` that overrides `Animations/Controllers/Humanoid Controller (Override This)` and assign it to your models animator.
4. Add a `Rigidbody`
5. Add an appropriate `Capsule Collider` to cover the characters body
6. Add an `Audio Source`
7. Bake the `NavMesh` in your scene
8. Hit play - your character should idle

See the demo scene in `Characters/Scenes/Behvaiours/101 Minimal Character` and [Documentation](Behaviours/101 Minimal Character.md).

# Behaviours

Your character will either be controlled by the AI brain or by directions sent from some kind of director. We will first add an AI behaviour to wander. Then we will add a story based director that will allow us to control the way the character moves based on player input to the story.

## Adding a Wander Behaviour

This section will add a basic wander AI behaviour to your character. It's not a very interesting behaviour, but it is a start.

1. Create a child of your brain called `Wander`
2. Add the `Wander` or `Wander with Intent` component to this object. the difference is only in the way the character wanders. Wander is entirely random, wheras Wander with Intent will continue in roughly the same direction until they run out of space, then they will pick a new direction.
3. The character will be moving around so lets add a Cinemachine camera to make it easier to follow them `GameObject -> Cinemachine -> Virtual Camera`
4. Setup the free look camera to follow and look at your character.
5. Hit play - your character should wander, although they will run everywhere

## Directing the Character

Sometimes you will want to instruct the character to perform specific actions. This can be done via the API, but here we will use the available Ink integration. Ink is a narrative story language. The character system extends it to allow the character to be given instructions. This is great for storytelling games but is also very useful for testing new characters.

1. Create an object called `Ink Manager` and add the `Ink Manager` components to it.
2. Drag the `Ink/Prefabs/Ink Manager` into the scene
3. Create a new Ink story using `Create -> Ink`
4. Add the content below to the story file
5. Drag the compiled JSON file (stored in the same folder as you Ink file) into the `Ink Manager` parameter `Ink JSON`
6. Add an Actor to the list of Actors in the `Ink Manager` and drag your Character into the slot.
7. Somewhere in your scene add a cube called `Mark1`, it must be on the NavMesh, but other than that it can be anywhere. Remove the collider.
8. Hit play, your character will wander as before. However, you will now have a narrative dialog.
9. Click the "Go to Mark 1" button, the character will navigate to the mark you set.
10. If left to their own devices the character will start to wander again.

```
-> CharacterDevScript

= CharacterDevScript

{stopping:
- Hi, I'm your new character.
- What shall I do now?
}

  + [Go to Mark 1] -> Mark1
  * [Goodbye] -> END
  
= Mark1

>>> MoveTo: Goat Herder, Mark1

Oh, I love Mark 1.

-> CharacterDevScript
```

