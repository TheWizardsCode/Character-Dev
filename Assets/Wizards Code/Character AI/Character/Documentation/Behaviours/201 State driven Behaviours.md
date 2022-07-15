The `Brain` component selects the most appropriate behaviour to execute at any given moment. It does this by looking at a set of desired states in the brain. If any of these desired states are not satisfied the actor will look for a behaviour that affects stats in such a way as to bring them closer to the desired state.

If a behaviour is found that can be executred then the actor checks to see if they have access to whatever is needed to enact this behaviour. For example, if they are hungry they may look for a place they can eat. That is they check whether there is an interactable object within a detection range (set in the behaviour itself).

The actor in this scene has three behaviours. Eat, Sleep and Wander. Each behaviour has a weight that will give it priority over other behaviours if more than one is possible. The brain will attempt to pick the one that will have the most impact on desired states, taking into account the weight. So, for example, eat has a higher weight than sleep, and both have a higher weight than wander. If the need for food and sleep is otherwise equal then Eat will be chosen because of the higher weight. 

Note that the actor in this scene will wander "looking" for a place to eat or sleep, if they cannot detect a suitable location within their awareness range. In a later scene we will add a memory to the actor so that they can find their way back to places they have previusly discovered.

You can change the hunger and energy levels of the character in the bottom left of the screen when playing, this will allow you to observe the AI making decisions.

# Interactables

Behaviours that are intended to impact one ore more desired state will often require an interactable in order to execute the behaviour. For example, to sleep the actor will need a safe place to lie down. To eat they will need a source of food. These are setup as Interactable Objects. 

Interactable Objects have the `Interactable` component attached. They should also have one or more `StatInfluencerTrigger` and a collider to detect when an actor triggers the object. When looking for a suitable interactable Actors will detect these objects and filter them according to their current desires. If they find what they need then the behaviour is enabled and may be selected for enactment.

When a behaviour is enacted the actor will first move to the interactable. Upon reaching it they will trigger the collider and an influencer will be added to the `Brain` of the actor. This influencer will change stats of the character and/or the interactable and this the world state is changed by the actors behaviour. 

In this scene there are two interactables objects, a bed for sleeping and a table for eating.

# Generic Behaviours

While it is possible to create custom behaviours in code, many behaviours can be created using the `Generic AI Behaviour` component. This component will scan for interactables of the appropriate type and, if a suitable one is found, will tell the brain that this is a candidate behaviour for execution. Both the Eat and Sleep behaviours are such generic behaviours. These behaviours can be configured using a series of parameters and scriptable objects. All the behaviours in this scene are Generic Behaviours. 