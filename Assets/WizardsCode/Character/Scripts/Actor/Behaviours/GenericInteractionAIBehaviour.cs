using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Character;

namespace WizardsCode.Character
{
    /// <summary>
    /// This generic AI behaviour will seek an interactable within a defined range that will
    /// impact a specific set of states. The most appropriate interactable will be selected
    /// and the actor will move towards it.
    /// 
    /// Upon arrival the interactable should impart an influencer on the actor.
    /// </summary>
    public class GenericInteractionAIBehaviour : AbstractAIBehaviour
    {
    }
}
