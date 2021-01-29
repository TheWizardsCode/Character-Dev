#if CiDy
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Utility;
using CiDy;

namespace WizardsCode.CiDYExtension
{
    /// <summary>
    /// Extends the Wizards Code Spawner with features to automatically configure the NavMesh Areas in a
    /// CiDy generated city.
    /// </summary>
    public class CiDyPedestrians : Spawner
    {
        [SerializeField, Tooltip("The CiDy graph object that manages the generation of the city.")]
        CiDyGraph cidyGraph;


    }
}
#endif
