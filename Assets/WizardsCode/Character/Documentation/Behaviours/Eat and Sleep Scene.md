# Example: Eat and Sleep Behaviours

The `Brain` component selects the most approrpiate behaviour to execute at any given moment. It does this by looking at what the required states are for the behaviour to be executed coupled with whether there is an interactable object within a detection range that will impact the desired states of the actor. You can see this in action in the `Eat and Sleep` scene.

Note that the actor in this scene will wander aimlessly if they cannot detect a place to eat or sleep within their awareness range for the behaviour in question. However, actors can have memories, see the 211 Eat and Sleep with memories scene.

## Interactbles

Interactable Objects have the `Interactable` component attached. They should also have one or more `StatInfluencerTrigger` and a collider to detect when an actor triggers the object.

When an actor triggers an interactable an influencer will be added to the `Brain` of the actor. This influencer will change stats. In the demo scene there are two interactables objects, a bed for sleeping and a table for eating.

When you start the scene the character is both hungry and sleepy. The brain will therefore pick the highest priroty behaviour (priority is set by order in the inspector). This behaviour will then be executed. When complete another behaviour will be selected.