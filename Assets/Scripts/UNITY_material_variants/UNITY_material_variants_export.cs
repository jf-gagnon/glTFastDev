using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLTFast.Export;
using UnityEngine;
using UNITY_material_variants.Schema;
using Material = UnityEngine.Material;

namespace UNITY_material_variants
{
    public static partial class UNITY_material_variants_class
    {
        /// <summary>
        /// There is only 1 instance of this class per export
        /// That is, it includes all the game objects to be processed 
        /// </summary>
        class ExportData
        {
            internal HashSet<Material> materialsBank = new HashSet<Material>();
            internal Dictionary<Material, int> materialsToId = new Dictionary<Material, int>();
            internal List<(Material material, Schema.Variant variantToUpdate)> materialsToUpdate = new List<(Material, Schema.Variant)>();
            internal List<(Renderer renderer, Schema.Variant variantToUpdate)> primitivesToUpdate = new List<(Renderer, Schema.Variant)>();
            internal List<(int nodeId, MaterialVariants materialVariants)> nodesToUpdate = new List<(int, MaterialVariants)>();

            internal IEnumerable<Material> Update(int nodeId, MaterialVariantsComponent component)
            {
                var newMaterials = component.variantSet
                    .SelectMany(vs => vs.variants, (vs, v) => v.targetMaterial)
                    .ToHashSet();
                newMaterials.ExceptWith(materialsBank);
                materialsBank.UnionWith(newMaterials);
                
                var schemaMaterialVariants = new List<Schema.VariantSet>();
                foreach (var vs in component.variantSet)
                {
                    var schemaVariantSetVariants = new List<Schema.Variant>(); 
                    foreach (var v in vs.variants)
                    {
                        var schemaVariant = new Schema.Variant()
                        {
                            primitiveMaterialId = v.sourceMaterialIndex,
                        };
                    
                        materialsToUpdate.Add((v.targetMaterial, schemaVariant));
                        primitivesToUpdate.Add((v.renderer, schemaVariant));

                        schemaVariantSetVariants.Add(schemaVariant);
                    }

                    var schemaVariantSet = new Schema.VariantSet()
                    {
                        name = vs.name,
                        variants = schemaVariantSetVariants.ToArray(),
                    };
                    schemaMaterialVariants.Add(schemaVariantSet);
                }
            
                var nodeMaterialVariants = new MaterialVariants()
                {
                    variantSets = schemaMaterialVariants.ToArray(),             
                };
                nodesToUpdate.Add((nodeId, nodeMaterialVariants));

                return newMaterials;
            }

            internal bool UpdateMaterials()
            {
                materialsToUpdate.ForEach(data =>
                {
                    data.variantToUpdate.materialId = materialsToId[data.material];
                });
                    
                return true;
            }
            
            internal bool UpdatePrimitives()
            {
                // TODO
                Debug.LogWarning("Missing mapping of renders to primitive");
                
                primitivesToUpdate.ForEach(data =>
                {
                    data.variantToUpdate.primitiveId = -1;
                });
                    
                return true;
            }
        }

        static void OnGameobjectAdded(IGltfWritable gltf, GameObject gameObject, int nodeId, IMaterialExport materialExport)
        {
            if (gltf == null
                || gameObject == null
                || !gameObject.TryGetComponent<MaterialVariantsComponent>(out var component)
                || component.variantSet == null 
                || !component.variantSet.Any())
            {
                return;
            }

            var exportData = gltf.GetCustomData<ExportData>(name);
            if (exportData == null)
            {
                exportData = new ExportData();
                gltf.SetCustomData(name, exportData);
            }
            
            var newMaterials = exportData.Update(nodeId, component);
            newMaterials
                .ToList()
                .ForEach(m =>
                {
                    gltf.AddMaterial(m, out var materialId, materialExport);
                    exportData.materialsToId.Add(m, materialId);
                });
        }
        
        static void ObBakeGltf(IGltfWritable gltf, string directory, List<Task<bool>> tasks)
        {
            if (gltf == null)
                return;

            var exportData = gltf.GetCustomData<ExportData>(name);
            if (exportData == null)
                return;

            var updateMaterialsTask = Task.Run( () => exportData.UpdateMaterials());
            var updatePrimitivesTask = Task.Run( () => exportData.UpdatePrimitives());
            
            tasks.Add(updateMaterialsTask);
            tasks.Add(updatePrimitivesTask);
        }
        
        static void OnUpdateGltf(IGltfWritable gltf)
        {
            if (gltf == null)
                return;

            var exportData = gltf.GetCustomData<ExportData>(name);
            if (exportData == null)
                return;
            
            exportData.nodesToUpdate
                .ForEach(data =>
                {
                    gltf.SetNodeExtension(data.nodeId, name, data.materialVariants);
                });
        }
    }
}
