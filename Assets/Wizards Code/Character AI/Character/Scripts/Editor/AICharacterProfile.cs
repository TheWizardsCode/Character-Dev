using UnityEngine;

namespace WizardsCode.Character {
    [CreateAssetMenu(fileName = "New AI Character Profile", menuName = "Wizards Code/AI/Character Profile", order = 1)]
    public class AICharacterProfile : ScriptableObject
    {
        public enum ColliderType
        {
            Capsule,
            Box,
            Sphere
        }
        public enum Speed {
            Lumbering = 10,
            Slow = 20,
            Normal = 30,
            Fast = 40,
            VeryFast = 50
        }
        
        [SerializeField, Tooltip("A description of the defaults defined in this character profile")]
        public string description;
        [SerializeField, Tooltip("The type of collider to use for the character.")]
        public ColliderType colliderType;
        [SerializeField, Tooltip("The speed category of the character. This will be used to determine the character's movement and turning speed.")]
        public Speed speed = Speed.Normal;

        public void ConfigureActorController(BaseActorController controller)
        {
            switch (speed)
            {
                case Speed.Lumbering:
                    controller.MaxSpeed = 2f;
                    break;
                case Speed.Slow:
                    controller.MaxSpeed = 4f;
                    break;
                case Speed.Normal:
                    controller.MaxSpeed = 6f;
                    break;
                case Speed.Fast:
                    controller.MaxSpeed = 8f;
                    break;
                case Speed.VeryFast:
                    controller.MaxSpeed = 11f;
                    break;
            }
        }
        
    }
}
