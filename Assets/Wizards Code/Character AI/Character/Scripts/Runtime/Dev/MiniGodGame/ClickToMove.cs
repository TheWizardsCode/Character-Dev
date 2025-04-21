using UnityEngine;


namespace WizardsCode.Character
{
    public class ClickToMove : MonoBehaviour
    {
        [SerializeField, Tooltip("The character that is being controlled.")]
        BaseActorController m_Character;
        
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.name.StartsWith("Ground"))
                    {
                        m_Character.MoveTo(hit.point);
                        m_Character.TurnToFace(hit.point);
                    }
                }
            }
        }
    }
}
