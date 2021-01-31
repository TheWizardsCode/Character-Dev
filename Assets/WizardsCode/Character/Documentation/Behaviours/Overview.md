# Behaviours

Characters can have a number of behaviours that they may carry out at any time. 

Behaviors define actions a character might take. Behaviours can have required [states](States.md) that defined whether the character is able to take the actions defined in a behaviour.

Behaviours extend `AbstractAIBehaviour`, which in turn extends `MonoBehaviour`. Instad of implementing an `Update` method they should implement `OnUpdate`. Any initialization normally done in the `Start` mmethod should be done in the `Init` method.

# Example: Behaviours 101

In `Scenes/Behaviours/Basic Behaviours` we have a character with a single behaviour, `Wander`. This behaviour has no required states and thus will always fire. In other words the character in this scene will wander forever.





