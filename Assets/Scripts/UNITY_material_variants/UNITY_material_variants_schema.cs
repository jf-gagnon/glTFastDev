using System;
using GLTFast.Schema;
using UnityEngine;

namespace UNITY_material_variants.Schema
{
    /// <summary>
    /// Naive approach to support material variants: create a pool of
    /// materials and assign them on demand.
    /// </summary>

    [Serializable]
    public class Variant
    {
        /// <summary>
        /// Index into the root primitive list
        /// </summary>
        public int primitiveId;
            
        /// <summary>
        /// Index into the primitive's material list
        /// </summary>
        public int primitiveMaterialId;
            
        /// <summary>
        /// Index into the root material list
        /// </summary>
        public int materialId;
    }

    [Serializable]
    public class VariantSet : NamedObject
    {
        public Variant[] variants;
    }
    
    [Serializable]
    public class MaterialVariants
    {
        public VariantSet[] variantSets;
    }
}
