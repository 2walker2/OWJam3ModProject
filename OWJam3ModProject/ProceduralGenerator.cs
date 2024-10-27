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
        [Tooltip("The transforms to generate around")]
        [SerializeField] List<Transform> generatorTransforms;
        [Tooltip("Whether to ignore the generation region for each generator transform")]
        [SerializeField] List<bool> generatorTransformIgnoreGenerationRegion;
        [Tooltip("The position to store deactivated tiles at")]
        [SerializeField] Transform deactivatedTileStoragePosition;

        [Header("Editor")]
        [Tooltip("The transforms to generate around")]
        [SerializeField] Transform[] editorGeneratorTransforms;

        [Tooltip("The tiles that have been generated")]
        Dictionary<Vector2Int, GeneratorTile> generatedTiles;
        [Tooltip("The tile each generator transform was on last frame")]
        Dictionary<Transform, Vector2Int> generatorTransformsTilePrevious = new Dictionary<Transform, Vector2Int>();
        [Tooltip("Whether each generator's gameObject was active last frame")]
        Dictionary<Transform, bool> generatorTransformsActivePrevious = new Dictionary<Transform, bool>();
        [Tooltip("Colliders to not generate inside")]
        List<Shape> exclusionRegions = new List<Shape>();
        [Tooltip("Saved coordinates of unique tiles (GameObject is prefab)")]
        Dictionary<Vector2Int, GameObject> uniqueTilePositions = new Dictionary<Vector2Int, GameObject>();

        [Tooltip("A dictionary of deactivated tiles for reuse later (by tile id)")]
        Dictionary<int, List<GeneratorTile>> tilePool = new Dictionary<int, List<GeneratorTile>>();
        #endregion

        #region Unity Methods
        void Awake()
        {
            //Add self to generator dictionary
            if (generatorID == "")
                generatorID = gameObject.name;
            generatorInstances[generatorID] = this;

            //Initialize tile pool
            for (int i=0; i<spawnablePrefabs.Length; i++)
            {
                tilePool[i] = new List<GeneratorTile>();
            }
        }

        void Start()
        {
            generatedTiles = new Dictionary<Vector2Int, GeneratorTile>();

            //In-game initialization
            if (OWJam3ModProject.inGame)
            {
                FixShaders();

                //Add player and probe to generator transforms
                generatorTransforms.Add(Locator.GetPlayerTransform());
                generatorTransforms.Add(Locator.GetProbe().transform);

                generatorTransformIgnoreGenerationRegion.Add(false);
                generatorTransformIgnoreGenerationRegion.Add(false);
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

            //Initialize previous generator positions and activations states
            Vector2Int initialTilePrevious = new Vector2Int(-10000, -10000);
            foreach (Transform generatorTransform in generatorTransforms)
            {
                generatorTransformsTilePrevious[generatorTransform] = initialTilePrevious;
                generatorTransformsActivePrevious[generatorTransform] = generatorTransform.gameObject.activeSelf;
            }
        }

        void OnDestroy()
        {
            //Remove self from generator dicationary
            generatorInstances.Remove(generatorID);
        }

        void Update()
        {
            if (generate && ProjectionSimulation.instance.IsPlayerEnteringSimulation())
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
            //Iterate over the generation transforms to generate tiles
            Dictionary<Transform, Vector2Int> generatorTiles = new Dictionary<Transform, Vector2Int>();
            bool anyGeneratorChanged = false;
            for (int i=0; i<generatorTransforms.Count; i++)
            {
                Transform generatorTransform = generatorTransforms[i];

                //Skip if transform's gameObject is deactivated
                if (!generatorTransform.gameObject.activeSelf)
                {
                    if (generatorTransformsActivePrevious[generatorTransform])
                    {
                        generatorTransformsActivePrevious[generatorTransform] = false;
                        anyGeneratorChanged = true;
                    }
                    continue;
                }      

                //Get the generator's position in the generator's local space
                Vector3 localGeneratorPosition = generationRoot.InverseTransformPoint(generatorTransform.position);

                //Figure out what tile the generator is on
                Vector2Int generatorTile = new Vector2Int(Mathf.FloorToInt((localGeneratorPosition.x / tileSize) + 0.5f), Mathf.FloorToInt((localGeneratorPosition.z / tileSize) + 0.5f));
                generatorTiles[generatorTransform] = generatorTile;

                //Generate if tile has changed or generator has just activated
                if (generatorTile != generatorTransformsTilePrevious[generatorTransform] || !generatorTransformsActivePrevious[generatorTransform])
                {
                    GenerateArea(generatorTile, generatorTransformIgnoreGenerationRegion[i]);
                    generatorTransformsActivePrevious[generatorTransform] = true;
                }

                //Mark whether this generator's tile changed
                if (generatorTile != generatorTransformsTilePrevious[generatorTransform])
                {
                    generatorTransformsTilePrevious[generatorTransform] = generatorTile;
                    anyGeneratorChanged = true;
                }
            }
            
            //If any generator has changed tile or been deactivated, iterate over the tiles to clear tiles that are no longer within the range of any generator
            if (anyGeneratorChanged)
            {
                HashSet<Vector2Int> tilesToDestroy = new HashSet<Vector2Int>();
                foreach (KeyValuePair<Vector2Int, GeneratorTile> pair in generatedTiles)
                {
                    //Check each generator to see if this tile is in range
                    bool insideRange = false;
                    foreach (Transform generatorTransform in generatorTransforms)
                    {
                        //Skip if generator's gameObject is deactivated
                        if (!generatorTransform.gameObject.activeSelf)
                            continue;

                        if (Mathf.Abs(pair.Key.x - generatorTiles[generatorTransform].x) <= generationRange && Mathf.Abs(pair.Key.y - generatorTiles[generatorTransform].y) <= generationRange)
                        {
                            insideRange = true;
                            break;
                        }
                    }
                    //If the tile is not in range of any generator, add it to the list for destruction
                    if (!insideRange)
                        tilesToDestroy.Add(pair.Key);
                }

                //Deactivate all tiles not in range of any generator and add them to the pool
                foreach (Vector2Int tileCoordinates in tilesToDestroy)
                {
                    generatedTiles[tileCoordinates].transform.position = deactivatedTileStoragePosition.position;
                    tilePool[generatedTiles[tileCoordinates].tileId].Add(generatedTiles[tileCoordinates]);
                    generatedTiles.Remove(tileCoordinates);
                }
            }
        }

        void GenerateArea(Vector2Int center, bool ignoreGenerationRegion = false)
        {
            //Iterate over all positions to generate
            for (int x=-generationRange; x<=generationRange; x++)
            {
                for (int z=-generationRange; z<=generationRange; z++)
                {
                    //Generate the tile
                    Vector2Int tileCoordinates = new Vector2Int(center.x + x, center.y + z);
                    GenerateTile(tileCoordinates, ignoreGenerationRegion: ignoreGenerationRegion);
                }
            }
        }

        void GenerateTile(Vector2Int tileCoordinates, bool ignoreGenerationRegion = false)
        {
            //Skip if there's already a tile there
            if (generatedTiles.ContainsKey(tileCoordinates))
                return;

            //Find the tile position
            Vector3 spawnPosition = new Vector3(tileCoordinates.x * tileSize, 0, tileCoordinates.y * tileSize);

            //Skip if the tile position is not in a generation zone
            if (!ignoreGenerationRegion)
            {
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
            }

            //Skip if tile position is in an exclusion zone
            foreach (Shape exclusionRegion in exclusionRegions)
            {
                if (exclusionRegion.PointInside(transform.TransformPoint(spawnPosition)))
                    return;
            }

            //Find the tile prefab
            int spawnPrefabIndex = UnityEngine.Random.Range(0, spawnablePrefabs.Length);
            GameObject spawnPrefab = spawnablePrefabs[spawnPrefabIndex];

            GameObject spawnedGO = null;
            GeneratorTile spawnedTile = null;
            //Check if there is a matching tile in the pool
            if (tilePool[spawnPrefabIndex].Count > 0)
            {
                int lastIndex = tilePool[spawnPrefabIndex].Count - 1;
                spawnedTile = tilePool[spawnPrefabIndex][lastIndex];
                spawnedGO = spawnedTile.gameObject;
                tilePool[spawnPrefabIndex].RemoveAt(lastIndex);
            }
            //If no matching tile was found, instantiate one
            else
            {
                spawnedGO = Instantiate(spawnPrefab, generationRoot);
                spawnedTile = spawnedGO.GetComponent<GeneratorTile>();
                spawnedTile.tileId = spawnPrefabIndex;
            }

            //Place the tile
            spawnedGO.transform.localPosition = spawnPosition;
            spawnedGO.transform.Rotate(transform.up, 90 * UnityEngine.Random.Range(0, 4)); //Randomize rotation in increments of 90 degrees

            //Add it to the dictionary
            generatedTiles[tileCoordinates] = spawnedTile;
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
