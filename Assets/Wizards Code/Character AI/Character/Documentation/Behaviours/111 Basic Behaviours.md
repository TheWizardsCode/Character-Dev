# Behaviours

Characters can have a number of behaviours that they may carry out at any time. The Brain component decides which behaviour to execute at any given time. This decision is based on the current state of the Actor and the world around it. In this scene we add a simple wander behaviour. We also change out the `BaseActorController` for a `AnimatorActorController` which will convert movement of the actor into animation parameters driving a mecanim state machine.

Behaviours are special MonoBehaviours that implement the `AbstractAIBehaviour`. Instead of implementing an `Update` method they implement `OnUpdate` which is called by the Brain when the behaviour is active. Any initialization normally done in the `Start` method should be done in the `Init` method.

In this scene we have a character with a simple `Wander` behaviour. The behaviour component is attached to a child of the character object called `Wander only Behaviours`, though you can put them anywhere. Note that this behaviour object is available as a prefab in the `prefabs/Behaviour Sets` folder. While this particulat object is very simple later scenes will add more complexity and you may find these prefabs useful.





