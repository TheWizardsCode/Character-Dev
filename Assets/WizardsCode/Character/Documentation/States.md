# States

Characters can keep track of their current state via the `StatsController`. These states can be used to control what the character can do at any given time.

# States

States are defined in `StateSO` serializable objects. To create a state right click in the project window and select `Create/Wizards Code/Stats/New State`. Each state can define required stat values and substates as conditions for the state to be active. 

# Using States in Behaviours

[Behaviors](Behaviours.md) define actions a character might take. Behaviours can have required states. If all required states are active then the behaviour is a candidate for the current decision cycle. If any state is inactive then the behaviour will not be available.

# Example: Alive State

In `Scenes/Stats/Health Manager` there is an example scene that has a single character. This character has a Stats Controller which defines a desired state of `Alive`. This means that the character will seek to take actions that keep them alive.

The character also has a `Health Controller` which manages a `Health` stat. If this health stat drops to zero then the character will die, meaning they will play a death animation.

There is also a `Click To Move` behaviour which will move the character whenever the player clicks on the navmesh. This behaviour has a `Required State` of `CanMove`.

When you click play in this scene you are presented with a UI for controlling the value of the health stat. Clicking around the scene will move the character. Dropping the health stat to zero will play the death animation and prevent the player from moving, since the `Can Move` condition will not be satisfied.

If you remove the `Can Move` required state then you will be able to move the character even if they are dead.

