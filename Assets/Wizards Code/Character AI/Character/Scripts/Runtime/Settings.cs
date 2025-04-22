using UnityEngine;

namespace WizardsCode.Character 
{
    public static class Settings
    {
        public static string INTERACTABLE_LAYER_NAME = "Interactables"; 
        public static LayerMask InteractableLayerMask
        {
            get
            {
            return LayerMask.GetMask(INTERACTABLE_LAYER_NAME);
            }
        }
    }
}