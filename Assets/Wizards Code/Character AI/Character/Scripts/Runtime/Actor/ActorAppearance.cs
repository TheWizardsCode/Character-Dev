using NaughtyAttributes;
using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode {
    /// <summary>
    /// CharacterAppearance randomizes the appearance of a character OnStart.
    /// It will adjust colours, textures, blend shapes and other visual properties to create a unique look for each character instance.
    /// </summary>
    public class ActorAppearance : MonoBehaviour
    {
        [SerializeField, Tooltip("Should this component delete itself after it has randomized the appearance of the animal? If this is false and the animal is disabled/re-enabled then it will be randomized again on. If true, the animal will not be randomized again on re-enablement."), BoxGroup("General")]
        private bool deleteAfterEnable = true;

        [SerializeField, Tooltip("The name of the actor as used in the game."), BoxGroup("Meta Data")]
        private string actorName = string.Empty;

        [SerializeField, Tooltip("The main mesh renderer for the animal."), BoxGroup("Rendering")]
        private SkinnedMeshRenderer mainMeshRenderer;
        [SerializeField, Tooltip("The set of materials from which to randomly select a material for the animal's body."), BoxGroup("Rendering")]
        private Material[] bodyMaterials;
        [SerializeField, Tooltip("The set of additive mesh renderers that may be enabled/disabled to create a unique look for the animal. There may be multiple of these turned on."), BoxGroup("Rendering")]
        private SkinnedMeshRenderer[] additiveMeshRenderers;
        [SerializeField, Tooltip("A set of items from which a single instance will be enabled."), BoxGroup("Items")]
        private GameObject[] uniqueItem;

        [Space]
        [SerializeField, Tooltip("If true, the animal's shape will be randomized on start, using the options defined below."), ShowIf("HasBlendShapes"), BoxGroup("Blend Shapes")]
        private bool randomizeShape;
        [SerializeField, Tooltip("The range of values to use when randomizing the blend shapes."), ShowIf("randomizeShape"), BoxGroup("Blend Shapes")]
        private Vector2 randomizeShapeRange = new Vector2(-40, 40);

        internal bool HasBlendShapes => mainMeshRenderer && mainMeshRenderer.sharedMesh.blendShapeCount > 0;

        public SkinnedMeshRenderer MainMeshRenderer
        {
            get => mainMeshRenderer;
            set => mainMeshRenderer = value;
        }

        public Material[] BodyMaterials
        {
            get => bodyMaterials;
            set => bodyMaterials = value;
        }
        public SkinnedMeshRenderer[] AdditiveMeshRenderers
        {
            get => additiveMeshRenderers;
            set => additiveMeshRenderers = value;
        }

        void OnEnable()
        {
            GenerateAndSetName();
            RandomizeAppearance();

            if (deleteAfterEnable)
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Generates a name for the actor if one has not been set.
        /// Ensures that the actor name is unique in the scene.
        /// Ensures the object name is the same as the actor name.
        /// </summary>
        void GenerateAndSetName()
        {
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                int suffix = 1;
                string baseName = actorName;
                while (GameObject.Find(actorName) != null && GameObject.Find(actorName) != gameObject)
                {
                    actorName = $"{baseName}_{suffix}";
                    suffix++;
                }
            }

            gameObject.name = actorName;

            BaseActorController actorController = GetComponent<BaseActorController>();
            if (actorController != null)
            {
                actorController.DisplayName = actorName;
            }
        }

        [Button("Randomize Appearance")]
        private void RandomizeAppearance()
        {
            RandomizeRendering();
            RandomizeShape();
            RandomizeItems();
        }

        void RandomizeRendering()
        {
            if (bodyMaterials.Length > 0)
            {
                mainMeshRenderer.material = bodyMaterials[Random.Range(0, bodyMaterials.Length)];
            }

            foreach (var renderer in additiveMeshRenderers)
            {
                renderer.gameObject.SetActive(Random.value > 0.5f);
            }
        }

        void RandomizeShape()
        {
            if (!randomizeShape || !HasBlendShapes)
            {
                return;
            }

            float[] blendShapes = new float[mainMeshRenderer.sharedMesh.blendShapeCount];

            for (int i = 0; i < blendShapes.Length; i++)
            {
                float value = Random.Range(randomizeShapeRange.x, randomizeShapeRange.y);
                mainMeshRenderer.SetBlendShapeWeight(i, value);
            }
        }

        void RandomizeItems()
        {
            if (uniqueItem.Length == 0)
            {
                return;
            }

            foreach (var item in uniqueItem)
            {
                item.SetActive(false);
            }

            int randomIndex = Random.Range(0, uniqueItem.Length);
            uniqueItem[randomIndex].SetActive(true);
        }

        void OnValidate()
        {
            if (actorName == string.Empty)
            {
                actorName = gameObject.name;
            }
        }
    }
}