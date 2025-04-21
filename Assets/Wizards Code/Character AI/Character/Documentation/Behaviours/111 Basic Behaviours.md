Characters can have a number of behaviours that they may carry out at any time. The `Brain` component decides which behaviour to execute at 
any given moment. This decision is based on the current state of the Actor and the world around it. 

In this scene the actor has a simple wander behaviour. When the brain decides to wander a destination is picked and the character moves towards it.

The `BaseActorController` has been exchanged for an `AnimatorActorController` which will convert movement of the actor 
into animation parameters driving a mecanim state machine.

# About Behaviours

Behaviours are special MonoBehaviours that implement the `AbstractAIBehaviour`. Instead of implementing an `Update` method they implement `OnUpdate` which is called by the Brain when the behaviour is active. Any initialization normally done in the `Start` method should be done in the `Init` method.

In this scene we have a character with a simple `Wander` behaviour. The behaviour component is attached to a child of the character object called `Wander only Behaviours`, though you can put them anywhere. While this particular object is very simple later scenes will add more complexity and you may find these prefabs useful.

# Components

## Animator

This is a standard Unity Animator component. You can use any animation controller you want as long as it uses the parameters setup in the Actor Controller. If you want to get going quickly you can either copy or override the `Animations/Controllers/Humanoid Controller (Override This)`.

# NavMeshAgent

A standard Unity NavMeshAgent used by the Actor Controller to move the actor.





