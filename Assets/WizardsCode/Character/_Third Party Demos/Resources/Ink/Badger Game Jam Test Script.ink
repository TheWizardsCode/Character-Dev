>>> Cue: Glan, Move To Rocks (Glan)
>>> Cue: Kal, Move To Rocks (Kal)
>>> PlayerControl: On

-> Cliff_Edge

== Cliff_Edge

{ !You stalk to the cliff edge and place yourself behind a rock, your partners do the same. There are bandits visible in a camp below. }

* [Talk to Glan] -> Urge_Glan_To_Go_In

* [Use Binoculars] -> Scouting_The_Scene # Cue Player UseBinoculars

* [Climb down in the camp] -> Climb_Down


= Urge_Glan_To_Go_In
>>> Cue: Player, Talk To Glan
>>> TurnToFace: Glan, Player
 
you: this is a waste of time. We should just go in
glan: we need to  do this safely!
you: I wish we were just going in OR you're right... but still a wuss

-> Cliff_Edge

= Climb_Down
Glan signals that he heard something. Over to the right. You think you see movement.

* Throw a dagger
You throw a dagger in the direction of the sound. It doesn't hit anything, there was nothing there. You continue down to the camp.
-> Enter_The_Camp

* Lay flat and listen
Your party lies flat and still. After a short while you decide there is nothing there and continue down.
-> Enter_The_Camp

* Tweet like a bird and wait
You tweet like a bird, worried that they may have heard you. After a while there is no more signs of movement and you continue down the hill.
-> Enter_The_Camp

* Keep going -> Enter_The_Camp


= Scouting_The_Scene
A quick scan of the area reveals a few items of interest.

* [Look at the main tent]
You can see three people milling about inside the tent, there is at least one more you cannot see judging by their actions.
-> Scouting_The_Scene

* [Look at the boat in the distance] -> Scout_The_Boat

* [Look at the fellows play fighting by the bonfire]
They look ridiculous. Their movements are clumsy, almost child like. They need to play fight more.
->Scouting_The_Scene

* [Climb Down into the camp] -> Climb_Down

= Scout_The_Boat
The boat has a flag that looks familiar, it looks like they have backup coming in.

* Ask Kal if he recognizes the flag
Kal raises his binoculars to his eyes, after a few moments he says "Yep, you are right, that looks like one of theirs we should get this done quickly."
-> Scouting_The_Scene

* Keep it to yourself -> Scouting_The_Scene

== Enter_The_Camp
Well... the story needs to be finshed...

-> DONE


TODO: If she climbs down but used binoculars first on the play fighting, she can wait until a knockout, then go down without being noticed (because they are all distracted)

TODO: if she calls glan a wuss, modify a variable tracking their relationship friendliness... and integrate it with any other game state you need to track
