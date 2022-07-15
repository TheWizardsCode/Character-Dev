In the previous scenes the character wandered aimlessly in the hope of finding a place they could satisfy their desires. If actors are given a `MemoryController` component then they will remember things
that happen to them. These memories can influence which behaviours are selected by the brain, for example, if they remember eating at the table they will return to it even when it is out of sensory range.

Thw `211 Memories Scene` provides a simple example. It is almost the same as the 201 State Driven Behaviours scene, however, in this scene the actor has a memory compont (in a child object, but it could be anywhere). 

The initial behaviour of the character will be the same as in the 201 scene. The actor will go straight to the table and eat. They will then start wandering. Once the cooldown period is over things change. Unlike in the 201 scene when they become hungry again they will remember the location of the table and return immedietaly.

Clicking on the character in the Hierarchy in the editor will allow you to inspect the actors decision making process. This will show that the character remembers the table as a place to eat, but that they won't return there for a short while - opting to wander and look for somewhere else.

They will not remember places unless they had an effect on their stats. So initially the character will not remember the bed. However, once they sleep in the bed they will remember it.