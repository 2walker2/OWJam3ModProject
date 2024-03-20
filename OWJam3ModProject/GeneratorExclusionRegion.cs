using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using NewHorizons;

namespace OWJam3ModProject
{
    internal class GeneratorExclusionRegion : MonoBehaviour
    {
        [Tooltip("The generator to exclude this region from")]
        [SerializeField] string generatorID;
        [Tooltip("The shapes that make up this region")]
        [SerializeField] Shape[] exclusionShapes;

        void Start ()
        {
            //Add colliders to generator's exclusion regions
            if (ProceduralGenerator.generatorInstances.ContainsKey(generatorID))
            {
                ProceduralGenerator generator = ProceduralGenerator.generatorInstances[generatorID];

                foreach (Shape exclusionShape in exclusionShapes)
                {
                    generator.AddExclusionRegion(exclusionShape);
                }
            }
            else
            {
                Main.Instance.ModHelper.Console.WriteLine("Attempted to exclude a region from a generator that doesn't exist", MessageType.Error);
            }
        }
    }
}
