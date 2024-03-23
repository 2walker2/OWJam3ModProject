using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Collections.Generic;
using NewHorizons;

namespace OWJam3ModProject
{
    internal class SpawnPrefabOnStart : MonoBehaviour
    {
        [Tooltip("The prefab to spawn")]
        [SerializeField] GameObject prefab;
        [Tooltip("The Transform to match the prefab's location and rotation to")]
        [SerializeField] Transform anchor;

        void Start()
        {
            Main.Instance.ModHelper.Console.WriteLine("SPAWNING PREFAB ON START");

            NewHorizons.Utility.Files.AssetBundleUtilities.ReplaceShaders(prefab);
            Transform parent = transform.root.Find("Sector");
            GameObject go = Instantiate(prefab, parent);
            if (go != null)
            {
                go.transform.position = anchor.position;
                go.transform.rotation = anchor.rotation;
            }
        }
    }
}
