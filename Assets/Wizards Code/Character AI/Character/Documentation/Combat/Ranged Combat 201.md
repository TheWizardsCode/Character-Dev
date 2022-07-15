In this scene there are two characters. They wander around. If they see the other then they will approach and melee attack one another.


## Implementation

  * Create a normal AI character with a Wander behaviour (reduce the `Weight Multiplier` to 0.5)
    * Set to the Blue Layer
    * Give them a blue skin colour
  * Add a `Enemy Senses` empty Game Object as a child of the `Brain`
    * Add a `SightSense` component to the brain, name it `Spot Enemy` and configure to sense on the red layer
  * Add a `Spotted Enemy Behaviours` empty Game Object as a child of the `Brain`
    * Add a `Melee Combat Behaviour` to the `Spotted Enemy Behaviours` object
    * Name it `Melee Attack`
    * Reduce the `Retry frequency` to 1 second
    * Mark is as `Is Interuptable`
    * Change the `Maximum Execution Time` to 5 seconds
    * Add `Melee Attack - Start`, `Melee Attack - Perform` and `Melee Attack - End` Actor Cues to the `Melee Attack` behaviour
    * Add a required sense to the `Melee Attack` behaviour and drag in the `Spot Enemy` sense into the slot
    * Ensure `Require Consent` is set to false
    * Reduce the `Cooldown Time` to 1 second
    * Add the `Packages/wizardscode.character/Character/Resources/Stats/Stat Influencers/Weapons/Fists Damage Influencer.asset` to the `Enemy Stat Influencers`
  
