# Behaviours

Characters can have a number of behaviours that they may carry out at any time. The Brain component decides which behaviour to execute at any given time. This decision is based on the current state of the Actor.

Behaviours are special MonoBehaviours that implement the `AbstractAIBehaviour`. Instad of implementing an `Update` method they should implement `OnUpdate`. Any initialization normally done in the `Start` method should be done in the `Init` method.

In `Scenes/Behaviours/111 Basic Behaviours` there is a character with a single behaviour, `Wander`. The behaviour component is attached to a child of the character object called `Wander only Behaviours`, though you can put them anywhere. Note that this behaviour object is available in the prefabs/Behaviour Sets` folder. While this particulat object is very simple later scenes will add more complexity and you may find these prefabs useful.





