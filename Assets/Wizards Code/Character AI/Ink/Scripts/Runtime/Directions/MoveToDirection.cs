using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Ink;
using WizardsCode.Character;

namespace WizardsCode.Ink
{
    /// <summary>
    /// The MoveTo direction instructs an actor to move to a specific location. It is up to the ActorController
    /// to decide how they should move. By default the story
    /// will wait for the actor to reach their mark before continuing. Add a NoWait parameter to allow the story to continue without waiting.
    /// </summary>
    /// <param name="parameters">ACTOR, LOCATION [, Wait(default)|No Wait]</param>
    public class MoveToDirection : AbstractDirection
    {
        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 2, 3))
            {
                return;
            }

            BaseActorController actor = Manager.FindActor(parameters[0].Trim());
            if (actor == null) return;

            Transform target = Manager.FindTarget(parameters[1].Trim());
            if (target == null) return;

            actor.MoveTo(target);

            if (parameters.Length == 3)
            {
                string waitArg = parameters[2].ToLower().Trim();
                if (waitArg == "no wait")
                {
                    return;
                }
                else if (waitArg != "wait")
                {
                    Debug.LogError($"MoveTo instruction with arguments {string.Join(",", parameters)} has an invalid argument in posision 3. Valid values are 'Wait' and 'No Wait'. Falling back to the default of 'Wait'. Please correct the Ink Script.");
                }
                Manager.WaitFor(new string[] { parameters[0], "ReachedTarget" });
            } else
            {
                Manager.WaitFor(new string[] { parameters[0], "ReachedTarget" });
            }
        }
    }
}
