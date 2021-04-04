// ------------------------------------------------- includes
INCLUDE Prologue.ink
INCLUDE StyleGuide.ink
INCLUDE 1st_Class.ink
INCLUDE SucessfulEnd.ink
INCLUDE PrincipalsOffice.ink




// ------------------------------ variables -- constants -- names
// note: I think these hurt the writer's ability to write fluently, so I cut them... -nathan
// CONST ENEMY_NAME = "Enemy"
// CONST BESTIE_NAME = "Bestie"
// CONST ANTAGONIST_NAME = "Antagonist"
// CONST HALL_MONITOR_NAME = "HallMonitor"
// CONST RIVAL_NAME = "Rival"
// CONST TECHIE_NAME = "Techie"
// CONST PRINCIPAL_NAME = "PrincipalGuy" // his name in the script, also his nickname
// CONST PRINCIPAL_LAST_NAME = "PrincipalGuy-last-name"

// CONST PROTAGONIST_NAME = "Protagonist"
// CONST PROTAGONIST_LAST_NAME = "Protagonist-Last-Name"



// ------------------------------ variables -- constants -- location names
CONST LOC_PROLOGUE = "Prologue"



// ------------------------------ variables -- constants -- other

// ------------------------------ variables -- character state
//likability
VAR senaff = 1
VAR bfaff = 1
VAR stanaff = 1
VAR techaff = 0
VAR hmaff = 0
VAR rivaff = 0
VAR enaff = -1

// VAR loc_state = "" // never used



// ------------------------------ variables -- debug mode 
VAR debug_menu_enabled = true



// ------------------------------------------------- main
-> main

=== main ===
- (opts)
* [Prologue]->Prologue
* {Prologue} [IncitingIncident] -> IncitingIncident
* {IncitingIncident} [PrincipalsOffice] -> PrincipalsOffice
* {PrincipalsOffice} [SucessfulEnd] -> SucessfulEnd



// ------------------------------ debug menu
+ {debug_menu_enabled} debug menu
    -- (opts_debug)
    // we can add options to set vasriables here too
    ++ jump to Prologue
        -> Prologue
    ++ jump to IncitingIncident
        -> IncitingIncident
    ++ jump to PrincipalsOffice
        -> PrincipalsOffice
    ++ jump to SuccessfulEnd
        -> SucessfulEnd
    ++ view style guilde
        -> StyleGuide
    ++ leave debug menu
        -> opts
- -> opts

* -> DONE