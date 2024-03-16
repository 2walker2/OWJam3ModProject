using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;
using NewHorizons;
using System.Collections.Generic;

namespace OWJam3ModProject
{
    internal class ProceduralGenerator : MonoBehaviour
    {
        #region Variables
        [Tooltip("The root transform to child all the procedural tiles under")]
        [SerializeField] Transform generationRoot;
        [Tooltip("The set of prefabs that can be generated")]
        [SerializeField] GameObject[] spawnablePrefabs;
        [Tooltip("The size of a tile in the grid")]
        [SerializeField] float tileSize;
        [Tooltip("The distance around the player to generate tiles")]
        [SerializeField] int generationRange;
        [Tooltip("The maximum distance away from the generation root that the player can be before generation stops")]
        [SerializeField] float maxGenerationDistance = 100;

        [Header("Editor")]
        [Tooltip("The transform to treat as the player transform")]
        [SerializeField] Transform playerTransform;

        [Tooltip("The tiles that have been generated")]
        Dictionary<Vector2Int, ProceduralTile> generatedTiles;
        [Tooltip("The tile the player was on last frame")]
        Vector2Int playerTilePrevious = new Vector2Int(-1000000, -10000000);
        #endregion

        #region Unity Methods
        void Start()
        {
            generatedTiles = new Dictionary<Vector2Int, ProceduralTile>();

            //In-game initialization
            if (OWJam3ModProject.inGame)
            {
                FixShaders();
                playerTransform = Locator.GetPlayerTransform(); //Overwrite the one set in the editor
            }
        }

        void Update()
        {
            GenerateAroundPlayer();
        }
        #endregion

        #region Custom Methods
        void FixShaders()
        {
            foreach (GameObject prefab in spawnablePrefabs)
            {
                NewHorizons.Utility.Files.AssetBundleUtilities.ReplaceShaders(prefab);
            }
        }

        void GenerateAroundPlayer()
        {
            //Get the player's position in the generator's local space
            Vector3 localPlayerPosition = generationRoot.InverseTransformPoint(playerTransform.position);

            //Skip if player is too far away
            if (localPlayerPosition.magnitude > maxGenerationDistance)
                return;

            //Figure out what tile the player is on
            Vector2Int playerTile = new Vector2Int(Mathf.FloorToInt((localPlayerPosition.x / tileSize) + 0.5f), Mathf.FloorToInt((localPlayerPosition.z / tileSize) + 0.5f));
            if (playerTile != playerTilePrevious)
            {
                Generate(playerTile);
                playerTilePrevious = playerTile;
            }
        }

        void Generate(Vector2Int center)
        {
            //Generate the props
            for (int x=-generationRange; x<generationRange; x++)
            {
                for (int z=-generationRange; z<generationRange; z++)
                {
                    Vector2Int tileCoordinates = new Vector2Int(center.x + x, center.y + z);

                    //Skip if there's already a tile there
                    if (generatedTiles.ContainsKey(tileCoordinates))
                        continue;

                    //Spawn the tile
                    Vector3 spawnPosition = new Vector3(tileCoordinates.x*tileSize, 0, tileCoordinates.y*tileSize);
                    int spawnPrefabIndex = Random.Range(0, spawnablePrefabs.Length);
                    GameObject spawnedGO = Instantiate(spawnablePrefabs[spawnPrefabIndex], generationRoot);
                    spawnedGO.transform.localPosition = spawnPosition;

                    //Add it to the dictionary
                    ProceduralTile spawnedTile = spawnedGO.GetComponent<ProceduralTile>();
                    generatedTiles[tileCoordinates] = spawnedTile;
                }
            }
        }

        [ContextMenu("Clear")]
        void Clear()
        {
            generationRoot.DestroyAllChildrenImmediate();
        }
        #endregion
    }
}
