using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;

namespace OWJam3ModProject
{
    internal class UniqueGameObject : MonoBehaviour
    {
        static HashSet<string> instantiatedIds = new HashSet<string>();

        [Tooltip("The ID for which no duplicate UniqueGameObjects can exist")]
        [SerializeField] string id;

        void Awake()
        {
            if (instantiatedIds.Contains(id))
                Destroy(gameObject);
            else
                instantiatedIds.Add(id);
        }

        void OnDestroy()
        {
            if (instantiatedIds.Contains(id))
                instantiatedIds.Remove(id);
        }
    }
}
