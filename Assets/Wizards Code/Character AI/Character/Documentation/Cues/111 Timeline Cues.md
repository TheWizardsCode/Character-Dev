In this scene rather than triggering cues from a UI button we trigger them from a timeline. This allows sequences of cues to be played with a single in-game event. This is useful for cut scenes or scripted NPC events.

The scene is setup the same to the 101 scene with three marks for the actor to move to. However, rather than clicking a button controlled by a director class to send the cues to the actor there is a `Scene Timeline` object which contains a standard Unity timeline containing each of the three cues to move to each mark. 

This scene also includes a new kind of cue, a `ActorCueAnimator`. This kind of cue allows animation clips, parameters and layers to be controlled. There are three such cues in this scene, one to sit down, one sitting idle and one to stand up.

When the scene starts this timeline will start playing and thus the actor will move to the first mark, then to the second, where they will sit for a while and finally they will move to the third mark.