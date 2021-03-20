==Prologue==
// ~ loc_state = LOC_PROLOGUE
//Opening Scene School Bus
//Riders on Bus Already: Bestie, Stan, (Sitting in adjacent seats behind/in front of each other) Techie, (also sitting near friends) HallMonintor, (sitting by self) Rival, Enemy (sitting together) and any other students you want.
//Protagonist enters bus with camera, POV should pan over all the students before Protagonist takes a picture/a couple of picture. He should look at the camera (you don't have to show the pictures if that's difficult just have him smile.) You can have some of the students pose if you like Bestie and Stan should take should take notice of him.

Bestie: "Did you get the prefect photo yet?"
//Stan should be leaning over the back of the seat 
* [Maybe...] Protagonist: "Maybe..."
-

Stan: "Of course he did! Doesn't he always? That's why the photography mentor is coming to see his work after class."
Bestie: "Yeah, but, that doesn't mean he always gets it right away. Sometimes it takes a couple of shots to capture an entire moment in a single shot."
// Techie should be seated across from Bestie and Stan, playing on some device. Not looking up.
Techie: "Why would you even try to capture a whole moment in one single frame? You know science says that our minds rewrite memories everytime we recall them? If you want to remember the moment, video is where it's at."
Stan: "He can take a series of photos then, make an Instagram story!"
HallMonitor: "Not Instagram. None of you are old enough to have an Instagram account."
Rival: "Who even cares about that? You're so lame."
Enemy: "Don't use ableist language, you banal oaf."
Techie: "Uh, Finsta anyone..."
Bus Driver: "Take your seat, I don't have all day."

Choose your seat
* Sit beside your best friend
    ~ bfaff +=1
    -> Bestie
* Sit beside your biggest fan
    ~ stanaff +=1
    -> Stan
* Sit beside your classmate
    ~ techaff +=1
    -> Classmate
* Sit beside the HallMonintor
    -> HallMonitor
    * Sit by yourself
    -> Senpai



=Bestie
Bestie: "Today's the day. Your photography idol is flying in just to look at your work. You ready?"

* [Yeah! I got this in the bag] Protagonist: "Yeah! I got this in the bag!"
    Protagonist: "or should I say... I got the photos I want to show in this bag."
    //taps schoolbag.
    Bestie: Oh. That was bad, grampa. I hope you don't plan to do any of that when you meet them.
    Protagonist: Maybe they like dad jokes. 
    Bestie: No one likes bad jokes.
    Stan: I do!
    -> Frndst
* [I'm a little nervous.] Protagonist: "I'm a little nervous."
    Bestie: "Makes sense. If I was meeting Ranboo I'd be nervous too."
    Protagonist: "I still can't believe your idol is the villain from some minecraft twitch channel."
    Bestie: "What can I say? Ranboo is mood."
    Techie: "No Cap."
    -> Frndst


=Stan
Stan: "Yo, Mr CEO of photography, we should wallpaper the front of the school in your photos before your idol gets here."
Bestie: "I think that might be a little bit much, don't you?"
Stan: "Would you deny our school seeing more of Protagonists work? If anything it's not quite enough."

* [Great idea.] Protagonist: "Great idea."
    ~ stanaff +=1
    ** [If only] Protagonist: "If only we'd thought of that a week earlier!"
    --
    Bestie: "You two would be in denention? Remember you're supposed to be on your best behavior before your idol gets here. Aren't they kind of serious?"
    Stan: "The photos would win them over."
    ** [My work will speak for me.] Protagonist: "My work will speak for me."
    --
    -> Frndst

* [I got to keep a low profile. Principals orders.] Protagonist: "I got to keep a low profile. Principals orders."
    ~ stanaff -=1
    Stan: "I don't think the principal understands how moving your work is! It's unfair!"
    Bestie: "Didn't he set up this meeting for him? I think he gets it."
    ** [I want to make a good impression] Protagonist: "I want to make a good impression maybe if we just did the front doors."
    --
    Stan: "Yes."
    Techie: "You guys remember they come in today, right?
    Stan: "Ye-"
    -> Frndst



=Classmate
//Techie put down his device and hold his hand out for the camera when he sits beside Techie. Stan and Bestie should finished up a conversation at the same time as this one and then move to sit behind Protagonist and Techie.
Techie: "So is this the new camera?"
* [Yeah...] Protagonist: "Yeah, my parents got it for me when PrincipalGuy gave told my parents the news."
-

Techie: "Oh, this model has in-body images stabilzation and a 15fps shutter! Pretty sophisticated for a grade schooler."
* [Yeah... it is] Protagonist: "Yeah... it is. How did..? Are you getting into photography too?"
- 

Techie: "Nah, you're my friend. So I like to keep up on technology you'd use. Well, technology in generally is kind of my jam."
Techie: "We might have our own things going on now but I can still enjoy looking at the tech specs."
* [Haha.] Protagonist: "Haha. Wish I could keep up with computers too."
- 
-> Frndst



=Frndst
//Anatagonist comes in takes a picture and sits beside in the open seat beside stan or Bestie.
<b>Click</b>
Anatagonist: "Hey, Protagonist."
-> Bus



=HallMonitor
//turns to look at Protagonist with an unhappy look on his face.
HallMonitor: "What is it this time?"
* ["Huh?"] Protagonist: "Huh?"
-
HallMonitor:"You usually only come to sit with me if you want something or if you're going to do something to me. So which is it?"
* [I just wanted...] Protagonist: "I just wanted to hang out with you today."
-
HallMonitor: "Well now, that's even more suspicious..."
* [No prannks...] Protagonist: "No prannks or anything I swear! My idol is coming to see my photos today. I got to be on my best behavior."
-
HallMonitor: "Yeah... today."
* [Since when...] Protagonist: "Since when do you not like my pranks?"
-
HallMonitor: "I'm just not in the mood for them today."
* [Well lets go play some Amoung Us after...] Protagonist: "Well lets go play some Amoung Us after everything is all worked out then?"
    HallMonitor: "Yeah, okay."
    ~ hmaff +=1
    -> Frndst
* [you got to admit...] Protagonist: "But you got to admit the last prank was pretty funny."
    HallMonitor: "Maybe to you, but it took me 3 days to get that smell out of my locker!"
    ** [Whats three days...] Protagonist: "Whats three days worth compared to the memories?"
    --
    HallMonitor: "You got me in trouble with my home room teacher!"
    ** [That's because...] Protagonist: "That's because your home room teacher is lame."
    --
    ~ hmaff -=1
    -> Frndst



=Senpai
//Anatagonist comes in takes a picture and sits Protagonist.
<b>Click</b>
* ["Huh?"] Protagonist: "Huh?"
-
//Puts her camera away and sits beside Protagonist
Antagonist: "Thought I'd capture the day your life might change."
* [Haha...] Protagonist: "Haha. Me too."
-
Antagonist: "Can I see?"
* [Sure.] Protagonist: "Sure."
-
//hands Antagonist the Camera.
Antagonist: "You got some decent shots today. Nice composition for a school bus."
* [Thanks...] Protagonist: "Thanks. That means a lot coming from you. Have you ever thought of switching over to digital instead of film?"
-
Antagonist: "No. I like the surprise of the shot coming alive later in development. Your digital camera feels like easy mode to me."
* [I never thought of it that way.] Protagonist: "I never thought of it that way."
-
Antagonist: "Well now you will."
//Bestie and Stan should come sit close to these two.
-> Bus



=Bus
* [Can you beleive...] Protagonist: "Can you beleive that in about 8 hours from now. The person who's phtographs inspired me is going to be here, looking at my work?"
-
Antagonist: "I thought I was the one who inspired you."
* [You do too!] Protagonist: "You do too! But they're an adult. It's their job! They're a professional."
-
Antagonist: "Being an adult is overrated."
Stan: "Once they see your work, you're going to be famous! Famous and only in grade school! I can't wait to tell people I helped you get there one day!"
Bestie: "Hey, take it down! They haven't even met yet. Don't make him Stress!"
Antagonist: "I'd be more worried about FOMO."
Stan: "Missing out on what?"
Antagonist: "On childhood. Who wants to rush into being an adult?"
Stan: "Sounds like your just jealous because the photographer isn't coming to see you! Don't you try to hold my bro back."
* [Actually...] Protagonist: "Actually, the prinicpal offered for the photographer to see both or works but Antagonist turned them down."
-
Bestie: "What?"
Stan: "Woah."
Antagonist: "We're in grade school. It's not like this is going to be my only oppurtunity. I don't feel a need to show off. I know my capabilties. The only recognition I need is from myself."
Bestie: "Damn."
Techie: "Or is it you just can't keep up with all the innovations."
Antagonist: "Well there's something to be said for doing it old school."
Stan: "Haha. Yu're such a hipster."
//Rival shouting from whereever they are.
Rival: "Well I for one hope this goes great for you! I've been enjoying the number one spot as mischief maker at the school."
HallMonitor: "I'm not."
Enemy: "I'd be glad if this meant you'd be leaving our school."
Rival: "Oh ho! Someones still mad."
//Enemey leaves and the rest should get off the bus. switch scene to morning classroom.
-> main
