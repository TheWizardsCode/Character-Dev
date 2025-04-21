The Cues System allows the game to provide instructions to actors. These instructions control the characters movements and animations They do not change the actors statistics (see Behaviours and Interactables).

In this scene we have a single actor, with no behaviours attached. Left to their own devices the actor will simply stand sidling. However, there is also a simple director component in the scene. This component contains a set of cues that the director will send to the actor. Each time the user clicks the prompt button the actor will carry out the cue.

In this very simple scene each cue simply moves the character to the marked location.

Cues are defined using the `Create -> Wizards Code -> Actor -> Cue` and `Create -> Wizards Code -> Actor -> Animation Cue` menu options. The first is a simple cue that does not have animation effects attached, and is the type used in this scene. The walk animations come from the standard actor controller. 

The `Animation Cue` is one that defines animations the actor should play. We will look at those in the next example.
