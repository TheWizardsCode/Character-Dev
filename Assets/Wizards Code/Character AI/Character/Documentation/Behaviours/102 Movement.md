The 102 scene demonstrates a basic player controlled actor. This allows the player to influence an actors brain in its decision making. That is, it can force specific behaviours at specific times.

The agent has a "Behaviour" attached to the brain (see the `Input Behaviours` child object under the `Brain`). We'll look at behaviours in more detail in subsequent examples, but for now it is enough to know that they are behaviours the brain can decide to have the actor engage in. In this case the behaviour is to respond to a mouse click by moving to the point of interaction.

In this demo the player is simply controlling the Actors movement. The movement is a simple translation and rotation managed by the `BaseActorController`.