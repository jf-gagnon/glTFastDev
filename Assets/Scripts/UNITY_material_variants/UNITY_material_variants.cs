using GLTFast;
using GLTFast.Schema;
using GLTFast.Export;
using UnityEngine;

namespace UNITY_material_variants
{
    public static partial class UNITY_material_variants_class
    {
        public const string name = "UNITY_material_variants";

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            CustomPropertyRegistry.Register<NodeExtensions, Schema.MaterialVariants>(name);

            ExportDelegates.gameObjectAdded += OnGameobjectAdded;
            ExportDelegates.bake += ObBakeGltf;
            ExportDelegates.update += OnUpdateGltf;
        }
    }
}
