Actors can have a number of behaviours that they may carry out at any time. We already used one in the `Player Control and Movement` scene. This scene takes a closer look at what they are and how they are used. Behaviours can be very complex, but in this scene we will add a simple wander behaviour that can be executed at any time.

Behaviours are managed by the `Brain` component, which decides which behaviour to execute at any given moment. This decision is based on the current state of the Actor and the world around it. 

Since the actor in this scene only has a wander behaviour available to it the brain only has two options at any one time. To Wander or to Idle. The Idle behaviour does not need to be explicitly defined since it is a "do nothing" behaviour. When the brain decides to wander a destination is picked and the character moves towards it.

It is also worth noting that the Actor model has been swapped out for a simple humanoid and the `BaseActorController` has been exchanged for an `AnimatorActorController`. This Actor Controller will convert movement of an actor on a NavMesh 
into animation parameters driving a mecanim state machine.

# Important Components

## Behaviours

Behaviours are special MonoBehaviours that implement the `AbstractAIBehaviour`. They can be located anywhere in the hierarchy of the Actor, but we like to always make them a child of the Actor.

### Behaviour Configuration

If you select the `Wander Anywhere Behaviour` in the scene view and scroll to the bottom of the inspector you will see that the top part of the inspector has a lot of parameters that allow for detailed control of the Actor and the environment. In this example scene we will look at how behaviours are implemented, later scenes will explore other features of behaviours.

At the bottom of the inspector there is a set of parameters for configuring the specifics of the wander behaviour. It is good practice to ensure that all parameters are fully described in a Tooltip. Since this is the case for the Wander behaviour we won't go into detail here.

### Behaviour Implementation

All behaviours will extend the `AbstractBehaviourClass` which implements the core functionality of the behaviour. The most critical aspect of behaviour implementation is the behaviour lifecycle. Since Behaviours are Unity `MonoBehaviours` you have access to all the standard Unity lifecycle events. In addition you have the following AI Behaviour lifecycle events available to your implementations:

* `Init` called whenever the behaviour is enabled. Any configuration for all runs of the behaviour should be added here. It is important to recognize that this means it may be called multiple times since behaviours are enabled and disabled by the brain depending on their relevance to the current Actor state. Be sure to consider the impact of this on performance.
* `StartBehaviour` called whenever the Brain wants this behaviour to be enacted. Any configuration for a specific execution of the behaviour should occur here.
* `OnUpdateState` this is used to update the behaviour while it is being executed. It is is not called every frame, the frequency of calls can be customized in the inspector. This is important from a performance perspective. The behaviour moves through a number of states (see below) with specific methods being called during these states. 

#### Behaviour States

Behaviours move through a series of `State`s during execution. The `OnUpdateState` method manages transition between stats. Many of the stages allow for an Actor Cue to be prompted. Cues tell the actor to do specific things like move, play an animation, play a sound and more. In this scene no cues are being used and so they are not discussed in detail here.

The default set of States are:

* `Inactive` - the behaviour is not active and no progress through the states will occur.
* `MovingTo` - the character is moving towards the location that the behaviour can be enacted. This is important when the behaviour involves an interactable item (interactables will be introduced later).
* `Starting` = the behaviour is starting. An `OnStartCue` may be prompted and `OnStartEvent`s will be fired. The environment is also checked to ensure that the behaviour can still be enacted, this is a check to ensure nothing has changed in the environment between deciding to take the action and actually performing it.
* `Preparing` - the actor is preparing to enact the behaviour. Any `OnPrepareCue` will be prompted here.
* `Performing` - the actor is performing whatever actions the behaviour requires. A `OnPerformingCue` may be prompted
* `Finalizing` - the actor is now closing out the execution of the behaviour. Any `OnFinalizeCue` will be prompted at this point.
* `Ending` - the system is being cleaned up before moving back into the `Inactive` state.

## Animator

This is a standard Unity Animator component. It is controlled through parameters set in the `AnimatorActorController` and in, potentially, in behaviours directly. You can use any animation controller you want as long as it uses the parameters setup in the Animation section of the `AnimatorActorController`. There is a basic humanoid animation controller provided in the asset at `Animations/Controllers/Humanoid Controller (Override This)`, you can either copy or override this to get started.

# NavMeshAgent

A standard Unity NavMeshAgent used by the Actor Controller to move the actor. It is important to note that some of the settings for this are set in the `AnimatorActorController`.





