VAR awareness_of_party = 0
EXTERNAL GetPartyNoticability()

-> Top_Knot

== Top_Knot
>>> Music: Upbeat, Strings

>>> SetEmotion: Glan, Fear, 1
>>> SetEmotion: Kal, Fear, 1
>>> MoveTo: Glan, Mark: Behind Rocks (Glan)
>>> MoveTo: Kal, Mark: Behind Rocks (Kal)
>>> PlayerControl: On
* [Approach: Cliff] -> Cliff_Edge

== Cliff_Edge

{ !You stalk to the cliff edge and place yourself behind a rock, your partners do the same. There are bandits visible in a camp below. }

* [Talk to Glan] -> Urge_Glan_To_Go_In

* [Use Binoculars] -> Scouting_The_Scene

* [Climb down to the camp] -> Climb_Down


= Urge_Glan_To_Go_In
>>> TurnToFace: Glan, Player
>>> TurnToFace: Player, Glan
>>> SetEmotion: Glan, Fear, 0
>>> SetEmotion: Glan, Anger, 0.5
 
you: this is a waste of time. We should just go in
glan: we need to  do this safely!
you: I wish we were just going in OR you're right... but still a wuss

-> Cliff_Edge

= Climb_Down
>>> Camera: Main Virtual Camera
>>> Cue: Glan, Move To Climbing Down
>>> Cue: Kal, Move To Climbing Down
>>> PlayerControl: On
* [>>> Approach: Camp] -> Heard_Something

-> Heard_Something

= Heard_Something
TODO: If player comes here directly then Glan may not be here. Need Glan and Kal to come over and the following line should be either Glan or a line about the player hearing a sound
{!>>> Music: Tense, Percussion}
>>> StopMoving: Player
>>> TurnToFace: Player, Enemy in the Bushes

{!Glan signals that he heard something. Over to the right. You think you see movement.}

{ GetPartyNoticability() <= RANDOM(0,100) / 100 :
You are discovered. All the bandits are rushing you - RUN FOR IT!!!

>>> MoveTo: Glan, Game Over Mark
>>> MoveTo: Kal, Game Over Mark
->END
}

* Throw a dagger
    You throw a dagger in the direction of the sound. It doesn't hit anything, there was nothing there.
    
    >>> Action: Player, Throw Knife

    -> Heard_Something

* Lay flat and listen 
  -> Lay_Still

* Tweet like a bird and wait
    You tweet like a bird, worried that they may have heard you. After a while there are no more signs of movement.
    -> Heard_Something

* Keep going -> Approach_The_Camp

= Approach_The_Camp

>>> MoveTo: Glan, Mark: Near Main Tent
>>> MoveTo: Kal, Mark: Near Main Tent
>>> PlayerControl: On
* [Approach: Enter_The_Camp] -> Enter_The_Camp

= Lay_Still

Your party lies flat and still. After a short while you decide there is nothing there and continue down.
    
>>> AnimationParam: Player, Sleeping, True
>>> AnimationParam: Glan, Sleeping, True
>>> AnimationParam: Kal, Sleeping, True
    
* [Get Back Up]
    >>> AnimationParam: Player, Sleeping, False
    >>> AnimationParam: Glan, Sleeping, False
    >>> AnimationParam: Kal, Sleeping, False
    
    -> Heard_Something

== Scouting_The_Scene

{! A quick scan of the area reveals a few items of interest. }

    * [Look at the main tent] -> Scout_Main_Tent

    * [Look at the boat in the distance] -> Scout_The_Boat

    * [Look at the fellows play fighting by the bonfire]

        They look ridiculous. Their movements are clumsy, almost child like. They need to play fight more.
    
        >>> Camera: Binoculars, Bonfire
    
        ->Scouting_The_Scene

    * [Climb Down into the camp] -> Cliff_Edge.Climb_Down

= Scout_Main_Tent  

You can see three people milling about inside the tent, there is at least one more you cannot see judging by their actions.

>>> Camera: Binoculars, Main Tent

-> Scouting_The_Scene


= Scout_The_Boat

The boat has a flag that looks familiar, it looks like they have backup coming in.

>>> Camera: Binoculars, Boat

* [Ask Kal if he recognizes the flag]
Kal raises his binoculars to his eyes, after a few moments he says "Yep, you are right, that looks like one of theirs we should get this done quickly."
-> Scouting_The_Scene

* [Keep it to yourself] -> Scouting_The_Scene

== Enter_The_Camp

Well... the story needs to be finshed...

-> END


TODO: if she calls glan a wuss, modify a variable tracking their relationship friendliness... and integrate it with any other game state you need to track
