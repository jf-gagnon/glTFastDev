using System;
using UnityEngine;

namespace UNITY_material_variants
{
    [Serializable]
    public class Variant
    {
        public Renderer renderer;
        public int sourceMaterialIndex;
        public Material targetMaterial;
    }

    [Serializable]
    public class VariantSet
    {
        public string name;
        public Variant[] variants;
    }
    
    public class MaterialVariantsComponent : MonoBehaviour
    {
        [SerializeField]
        VariantSet[] m_VariantSet;

        internal VariantSet[] variantSet => m_VariantSet;

        public void ApplyVariant(string name)
        {
            throw new NotImplementedException();
        }
    }
}
