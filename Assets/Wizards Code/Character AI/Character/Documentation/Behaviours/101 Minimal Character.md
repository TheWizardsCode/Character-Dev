The 101 scene shows the absolute minimal setup for a Wizards Code character, also known as an Actor.  

The actor in this scene will not exhibit any significant behaviours and has no animation controller. It  literally a blank slate for you to start the creation of an AI.

So how do you know they are even "alive"? The clues are in the UI.

# Developer UI

These example scenes all have a similar developer UI. In the top line you can see how many active brains there are in the scene. Since this scene has the one active brain you will see the number 1 here.

If you click on the actor then a status line will appear below this first line which tells you what the current behaviour of the character is. Since this character has no behaviours it will always be idle.

You can move the camera around this scene using the same controls as you would to move the scene camera.

# Required Components

The following is a list of the required components on this character and in the scene to make it work.

## On the Character

### Base Actor Controller

This is responsible for controlling the Actor. It does not make decisions (see `Brain` below) but it does enact those decisions in terms of movement etc.

While the Base Actor Controller is fully functional it is very limited in what it can do. In most cases you will use a controller that extends this one, such as the `Animator Actor Controller` which will convert movement on the navmesh to animation parameters. 

## Brain

The brain tracks all the characters stats and makes decisions about what the character will do. Since the character in this scene has no behaviours the brain has very little to do.

# Collider and Rigid Body

Required to ensure colliders and triggers are effective in the scene.

# Audio Source

A standard unity Audio Source allowing the Actor to make sound.
