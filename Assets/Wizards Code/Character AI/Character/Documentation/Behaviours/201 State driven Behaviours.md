In this example scene we will add behaviours that will only fire under the right Actor and Environment conditions. Specifically, the actor will seek to eat when they are hungry and food is available and they will seek to sleep when they are tired and a bed is available.

# Brain Selection of Behaviours - Desired States

The `Brain` component selects the most appropriate behaviour to execute at any given moment. It does this by looking at a set of desired states in the brain. If any of these desired states are not satisfied the actor will look for a behaviour that affects stats in such a way as to bring them closer to the desired state.

If a behaviour is found that can help reach a desired state then the actor checks to see if they have access to whatever is needed to enact this behaviour. For example, if they are hungry they may look for a place they can eat. This "place where they can eat" is represented as an interactable object in the game world. This particular actor has some food with them, however, so they are able to eat whenever they are hungry. At least until their food runs out.

Each Actor has a detection range which is set in the behaviour itself. If an appropriate interactable is not found within range then the behaviour cannot be executed. If an interactable is found within range then the brain will compare the utility of this behaviour against others that are available. Ultimately the brain will (usually) pick the action that is most impactful on their desired states.

## Stats and Desired States

In order to track the desires of our `Actor` we need to define some stats for them. In this example we will be using two stats `Hunger` and `Energy`. These stats are defined as `StatSO` Scriptable Objects, examples can be found in `Assets/Character-Dev/Assets/Wizards Code/Character AI/Character/Resources/Stats`.

It is important to note that `StatSO` definitions will often define a natural rate of change for the stat. For example, the Actor will get more tired and more hungry over time.

To cause an `Actor` to track a stat we need to define at least one `Desired State` for that stat. These are defined in `StateSO` Scriptable Objects, examples of them can be found in `Assets/Character-Dev/Assets/Wizards Code/Character AI/Character/Resources/Stats/States`. To assign a `Desire State` to an Actor we add is to the list of `Desired States` in the `Brain`. 

When defining the `Desired State` we can also define changes to the Actors stats as a result of the undesired state. These effects are defined in `StatInfluencersSO` Scriptable Objects (examples in `Assets/Character-Dev/Assets/Wizards Code/Character AI/Character/Resources/Stats/Stat Influencers`). For example, the `Fed` desired state, which tracks hunger, will cause the characters Energy to decrease more quickly.

`Desired States` can also enable/disable behaviours based on whether they are satisfied or not. For example, when the `Fed` `Desired State` is unsatisfied the `Eat Behaviour Set` is enabled. This set of behaviours will be added to the Actors available behaviours for as long as the character is not in the `Fed` desired state. This adding and removing of behaviours based on desired states allows very complex characters to be built up without the overhead of processing many ineligible states every update tick.

## Selecting Behaviours

The actor in this scene starts out with a single behaviour, Wander.  Eating and resting behaviours are added when the Actors current state demands it. 

Wander can be executed at any time and operates in the same way as seen in a previous demo scene. Eating will only be executed when the Actor is hungry and when an interactable that reduces hunger is nearby. Sleeping will only be executed when the Actor is tired and an interactable that allows recuperation of energy is available nearby. In later examples we will show how different behaviours can be limited to specific time windows too. This would allow us to provide a "sit and rest" behaviour for the daytime and a sleep for the nighttime, for example.

Each behaviour has a weight that will give it priority over other behaviours if more than one is possible. The brain will attempt to pick the one that will have the most impact on desired states, taking into account the weight. So, for example, eat has a higher weight than sleep, and both have a higher weight than wander. If the need for food and sleep is otherwise equal then Eat will be chosen because of the higher weight. 

If an actor needs to sleep or eat, but a suitable location is not nearby they will wander randomly "looking" for a place to eat or sleep. In a later example we will add a memory to the actor so that they can find their way back to places they have previously discovered, thereby reducing the need to randomly wander.

Note that for testing purporses you can select the agent and then change the hunger and energy levels of the character in the bottom left of the screen when playing, this will allow you to observe the AI making decisions.

# Interactables

Behaviours that are intended to impact one or more desired state will often require an interactable in order to execute the behaviour. For example, to sleep the actor will need a safe place to lie down. To eat they will need a source of food. These are setup as Interactable Objects. 

Interactable Objects have the `Interactable` component attached, they also need a `Collider` (setup as a trigger) to allow detection of when an actor is interacting with the object. Note that this collider needs to be larger than the object itself so that the Actor can move within it. To optimize for performance place all interactables onto the `Interactables` layer (Layer 10 by default).

When a behaviour that uses an interactable is enacted the actor will first move to the object. Upon reaching it they will trigger the collider and an `influencer` will be added to the `Brain` of the actor. This `influencer` will change stats of the character and/or the interactable and this the world state is changed by the actors behaviour. 

The stat influencers used in this example can be found under the `Character Influences` section of the `Interactable` objects inspector. 

In this scene there are two interactables objects, a bed for sleeping and a table for eating.


# Testing the Behaviours

This is our first fairly complex Actor. In order to test we can hit play, click on the actor and watch it pick behaviours based on its current stats which are displayed in the bottom right of the screen. You will notice that the character displays an icon above their head indicating their current active behaviour.

The characters stats will update in real time, but you can trigger actions by manipulating the sliders.

The Brains reasoning is output to the console for debug purporses.