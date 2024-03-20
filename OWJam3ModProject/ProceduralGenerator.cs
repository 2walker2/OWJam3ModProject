using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;
using NewHorizons;
using System.Collections.Generic;
using System;
using UnityEngine.Analytics;

namespace OWJam3ModProject
{
    internal class ProceduralGenerator : MonoBehaviour
    {
        #region Variables
        [Tooltip("A mapping of generator IDs to instances")]
        public static Dictionary<string, ProceduralGenerator> generatorInstances = new Dictionary<string, ProceduralGenerator>();

        [Tooltip("Whether this generator should generate")]
        [SerializeField] bool generate = true;
        [Tooltip("The ID of this generator")]
        [SerializeField] string generatorID;
        [Tooltip("The shapes within which to generate tiles")]
        [SerializeField] Shape[] generationRegion;
        [Tooltip("The root transform to child all the procedural tiles under")]
        [SerializeField] Transform generationRoot;
        [Tooltip("The set of prefabs that can be generated")]
        [SerializeField] GameObject[] spawnablePrefabs;
        [Tooltip("The size of a tile in the grid")]
        [SerializeField] float tileSize;
        [Tooltip("The distance around the player to generate tiles")]
        [SerializeField] int generationRange;
        /*[Tooltip("The tiles to start already spawned")]
        [SerializeField] GameObject[] startingTilePrefabs;
        [Tooltip("The positions of the tiles to start already spawned")]
        [SerializeField] Vector2Int[] startingTilePositions;
        [Tooltip("The transforms to generate around")]*/
        [SerializeField] List<Transform> generatorTransforms = new List<Transform>();

        [Header("Editor")]
        [Tooltip("The transforms to generate around")]
        [SerializeField] Transform[] editorGeneratorTransforms;

        [Tooltip("The tiles that have been generated")]
        Dictionary<Vector2Int, ProceduralTile> generatedTiles;
        [Tooltip("The tile the player was on last frame")]
        Vector2Int playerTilePrevious = new Vector2Int(-1000000, -10000000);
        [Tooltip("Colliders to not generate inside")]
        List<Shape> exclusionRegions = new List<Shape>();
        #endregion

        #region Unity Methods
        void Awake()
        {
            //Add self to generator dictionary
            if (generatorID == "")
                generatorID = gameObject.name;
            generatorInstances[generatorID] = this;
        }

        void Start()
        {
            generatedTiles = new Dictionary<Vector2Int, ProceduralTile>();

            //In-game initialization
            if (OWJam3ModProject.inGame)
            {
                FixShaders();

                //Add player and probe to generator transforms
                generatorTransforms.Add(Locator.GetPlayerTransform());
                generatorTransforms.Add(Locator.GetProbe().transform);
            }
            //Editor initialization
            else
            {
                //Add debug generator transforms
                foreach (Transform editorGeneratorTransform in editorGeneratorTransforms)
                {
                    generatorTransforms.Add(editorGeneratorTransform);
                }
            }

            //Place starting tiles
            /*for (int i = 0; i < startingTilePrefabs.Length; i++)
            {
                GenerateTile(startingTilePositions[i], startingTilePrefabs[i]);
            }*/
        }

        void OnDestroy()
        {
            //Remove self from generator dicationary
            generatorInstances.Remove(generatorID);
        }

        void Update()
        {
            if (generate)
                GenerateAroundTransforms();
        }
        #endregion

        #region Private Methods
        void FixShaders()
        {
            foreach (GameObject prefab in spawnablePrefabs)
            {
                NewHorizons.Utility.Files.AssetBundleUtilities.ReplaceShaders(prefab);
            }
        }

        void GenerateAroundTransforms()
        {
            //Iterate over the generation transforms
            foreach (Transform generatorTransform in generatorTransforms)
            {
                //Get the player's position in the generator's local space
                Vector3 localPlayerPosition = generationRoot.InverseTransformPoint(generatorTransform.position);

                //Figure out what tile the player is on
                Vector2Int playerTile = new Vector2Int(Mathf.FloorToInt((localPlayerPosition.x / tileSize) + 0.5f), Mathf.FloorToInt((localPlayerPosition.z / tileSize) + 0.5f));
                if (playerTile != playerTilePrevious)
                {
                    GenerateArea(playerTile);
                    playerTilePrevious = playerTile;
                }
            }
        }

        void GenerateArea(Vector2Int center)
        {
            //Iterate over all positions to generate
            for (int x=-generationRange; x<=generationRange; x++)
            {
                for (int z=-generationRange; z<=generationRange; z++)
                {
                    //Generate the tile
                    Vector2Int tileCoordinates = new Vector2Int(center.x + x, center.y + z);
                    GenerateTile(tileCoordinates);
                }
            }
        }

        void GenerateTile(Vector2Int tileCoordinates, GameObject prefab = null)
        {
            //Skip if there's already a tile there
            if (generatedTiles.ContainsKey(tileCoordinates))
                return;

            //Find the tile position
            Vector3 spawnPosition = new Vector3(tileCoordinates.x * tileSize, 0, tileCoordinates.y * tileSize);

            //Skip if the tile position is not in a generation zone
            bool inGenerationRegion = false;
            foreach (Shape generationShape in generationRegion)
            {
                if (generationShape.PointInside(transform.TransformPoint(spawnPosition)))
                {
                    inGenerationRegion = true;
                    break;
                }
            }
            if (!inGenerationRegion)
                return;

            //Skip if tile position is in an exclusion zone
            foreach (Shape exclusionRegion in exclusionRegions)
            {
                if (exclusionRegion.PointInside(transform.TransformPoint(spawnPosition)))
                    return;
            }

            //Find the tile prefab
            GameObject spawnPrefab = prefab;
            if (spawnPrefab == null)
            {
                int spawnPrefabIndex = UnityEngine.Random.Range(0, spawnablePrefabs.Length);
                spawnPrefab = spawnablePrefabs[spawnPrefabIndex];
            }

            //Spawn the tile
            GameObject spawnedGO = Instantiate(spawnPrefab, generationRoot);
            spawnedGO.transform.localPosition = spawnPosition;

            //Add it to the dictionary
            ProceduralTile spawnedTile = spawnedGO.GetComponent<ProceduralTile>();
            generatedTiles[tileCoordinates] = spawnedTile;
        }

        void Clear()
        {
            generationRoot.DestroyAllChildrenImmediate();
        }
        #endregion

        #region Public Methods
        public void AddExclusionRegion(Shape collider)
        {
            exclusionRegions.Add(collider);
        }

        public void SetIsGenerating(bool value)
        {
            generate = value;
        }
        #endregion
    }
}
