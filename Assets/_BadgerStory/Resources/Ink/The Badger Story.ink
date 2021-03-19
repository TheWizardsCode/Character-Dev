    >>> Action: Bestie, Sit
    >>> Action: Stan, Sit
    >>> Action: Techie, Sit
    >>> Action: HallMonitor, Sit
    >>> Action: Rival, Sit
    >>> Action: Enemy, Sit
    
    * [Start] -> Top_Knot

== Top_Knot

-> School_Bus

== School_Bus
    //Riders on Bus Already: Bestie, Stan, (Sitting in adjacent seats behind/in front of each other) Techie, (also sitting near friends) HallMonintor, (sitting by self) Rival, Enemy (sitting together) and any other students you want.
    
    //Protagonist enters bus with camera, POV should pan over all the students before Protagonist takes a picture/a couple of picture. He should look at the camera (you don't have to show the pictures if that's difficult just have him smile.) You can have some of the students pose if you like Bestie and Stan should take should take notice of him.
    
    >>> Camera: CloseupCam, Bestie, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: CloseupCam, Stan, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: CloseupCam, Rival, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: CloseupCam, Enemy, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: CloseupCam, Techie, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: CloseupCam, HallMonitor, Neo_Head
    >>> WaitFor: 4
    
    >>> Camera: PlayerCam
    >>> WaitFor: 2
    >>> MoveTo: Player, Take Photo Mark
    >>> TurnToFace: Player, Rival
    >>> WaitFor: Player, ReachedTarget
    
    >>> Cue: Player, TakePhoto - Start
    >>> WaitFor: 3.5
    >>> TurnToFace: Player, Nothing
    >>> Cue: Player, ExaminePhoto - Start
    >>> SetEmotion: Player, Pleasure, 0.8
    >>> AnimationParam: Player, Emote

    >>> Camera: PlayerCam

    Bestie: "Did you get the prefect photo yet?"
    
    * [Maybe...]
    
    Stan: "Of course he did! Doesn't he always? That's why the photography mentor is coming to see his work after class."
    Bestie: "Yeah, but, that doesn't mean he always gets it right away. Sometimes it takes a couple of shots to capture an entire moment in a single shot."
    Techie: "Why would you even try to capture a whole moment in one single frame? You know science says that our minds rewrite memories everytime we recall them? If you want to remember the moment, video is where it's at."
    Stan: "He can take a series of photos then, make an Instagram story!"
    HallMonintor: "Not Instagram. None of you are old enough to have an Instagram account."
    Rival: "Who even cares about that? You're so lame."
    Enemy: "Don't use ableist language, you banal oaf."
    Techie: "Uh, Finsta anyone..."

-> END
