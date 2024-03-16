using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;
using NewHorizons;

namespace OWJam3ModProject
{
    internal class ProceduralGenerator : MonoBehaviour
    {
        [Tooltip("The root transform to child all the procedural tiles under")]
        [SerializeField] Transform generationRoot;
        [Tooltip("The set of prefabs that can be generated")]
        [SerializeField] GameObject[] spawnablePrefabs;
        [Tooltip("The size of the grid to generate")]
        [SerializeField] Vector2Int gridSize;
        [Tooltip("The size of a tile in the grid")]
        [SerializeField] float tileSize;

        void Start()
        {
            if (OWJam3ModProject.instance != null)
                FixShaders();
            Generate();
        }

        void Update()
        {
#if DEBUG
            //Debug regenerate
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Generate();
            }
#endif
        }

        void FixShaders()
        {
            foreach (GameObject prefab in spawnablePrefabs)
            {
                NewHorizons.Utility.Files.AssetBundleUtilities.ReplaceShaders(prefab);
            }
        }

        [ContextMenu("Generate")]
        void Generate()
        {
            Clear();

            //Generate the props
            for (int x=0; x<gridSize.x; x++)
            {
                for (int z=0; z<gridSize.y; z++)
                {
                    Vector3 spawnPosition = new Vector3(x*tileSize, 0, z*tileSize);
                    spawnPosition -= new Vector3((tileSize * gridSize.x) / 2, 0, (tileSize * gridSize.y) / 2);
                    int spawnPrefabIndex = Random.Range(0, spawnablePrefabs.Length);
                    GameObject spawnedGO = Instantiate(spawnablePrefabs[spawnPrefabIndex], generationRoot);
                    spawnedGO.transform.localPosition = spawnPosition;
                }
            }
        }

        [ContextMenu("Clear")]
        void Clear()
        {
            generationRoot.DestroyAllChildrenImmediate();
        }
    }
}
