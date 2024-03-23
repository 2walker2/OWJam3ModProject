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

        [Tooltip("Whether this UniqueGameObject successfully instantiated")]
        [SerializeField] bool instantiated;

        void Awake()
        {
            if (instantiatedIds.Contains(id))
                Destroy(gameObject);
            else
            {
                instantiatedIds.Add(id);
                instantiated = true;
            }
        }

        void OnDestroy()
        {
            if (instantiated && instantiatedIds.Contains(id))
                instantiatedIds.Remove(id);
        }
    }
}
