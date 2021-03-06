The `Brain` component selects the most appropriate behaviour to execute at any given moment. It does this by looking at the desired states set in the brain. If any of these desired states are not satisfied the actor will look for a behaviour that affects stats in such a way as to bring them closer to the desired state.

If a behaviour is found that can be executred then the actor checks to see if there is a place to do so. That is they check whether there is an interactable object within a detection range (set in the behaviour iteself).

In the `201 State driven behaviours` scene the actor has three behaviours. Eat, Sleep and Wander. They are evaluated in that order, that is, if the actor is hungry and tired they will eat first and sleep second (although there is a weighting system that could result in sleeping before eating in extreme circumstances). If they are neither hungry nor tired then they will wander.

Note that the actor in this scene will wander aimlessly if they cannot detect a place to eat or sleep within their awareness range. In a later scene we will add a memory to the actor so that they can find their way back to places they have previusly discovered.

When you start the scene the character is both hungry and sleepy. The brain will therefore pick the highest priroty behaviour (priority is set by order in the inspector). This behaviour will then be executed. When complete another behaviour will be selected.

When playing the scene clicking on the character will activate the `DebugInfo` gizmo which will show you what is happening inside the brain.

# Interactables

Behaviours that are intended to impact one ore more desired state must have an interactable to work with in order to have that effect. Interactable Objects have the `Interactable` component attached. They should also have one or more `StatInfluencerTrigger` and a collider to detect when an actor triggers the object.

When an actor triggers an interactable an influencer will be added to the `Brain` of the actor. This influencer will change stats. In the  201 scene there are two interactables objects, a bed for sleeping and a table for eating.

# Generic Behaviours

Many behaviours can be created using the `Generic AI Behaviour` component. This component will scan for interactables of the appropriate type and, if a suitable one is found, will tell the brain that this is a candidate behaviour for execution. Both the Eat and Sleep behaviours are such generic behaviours.