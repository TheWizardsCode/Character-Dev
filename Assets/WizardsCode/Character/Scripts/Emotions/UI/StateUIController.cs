using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Stats;

namespace WizardsCode.Character
{
    public class StateUIController : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField, Tooltip("The character whos states we want to view")]
        StatsController character;


    }
}
