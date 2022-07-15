The 101 scene shows the absolute minimal setup for a Wizards Code character. also known as an Actor.  This actor will not exhibit any behaviours. They are a blank slare for your creation of an AI.


# Required Components

## Base Actor Controller

This is responsible for controlling the Actor. It does not make decisions (see Brain) but it does enact those decisions in terms of movement etc. It requires a NavMeshAgent component be attached. This Base Actor does not have any knowledge of an animation system. Although there is an animator controller on the character and so the Actor will idle. In the next scene we will see how to use Mecanim and in later scenes we will use Motion Matching for our characters. The ActorController is separated out like this to ensure you have maximum flexibility in selecting your chosen animation engine.

While the Base Actor Controller is fully functional it is very limited in what it can do. In most cases you will use another controller, such as the `Animator Actor Controller` which will convert movement on the navmesh to animation parameters. This allows the character to be automatically animated.

## Brain

The brain tracks all the characters stats and makes decisions about what the charcter will do.

## Animator

This is a standard Unity Animtor component. You can use any animation controller you want as long as it uses the parameters setup in the Actor Controller. If you want to get going quickly you can either copy or override the `Animations/Controllers/Humanoid Controller (Override This)`.

# NavMeshAgent

A standard Unity NavMeshAgent used by the Actor Controller to move the actor.

# Capsule Collider

Required by NavMeshAgent

# Rigidbody

Required to ensure colliders and triggers are effective in the scene.

# Audio Source

A standard unity Audio Source allowing the Actor to make sound.

# Optional Components

## DebugInfo

This is a useful component when in development. If this component is attached to your character an gizmo containing debug info will be shown.

