using GLTFast;
using GLTFast.Export;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UNITY_simple_extension;

public class TestImportExport : MonoBehaviour
{
    void Start()
    {
        TestAll("C:/Users/jeanfrancois.gagnon/Documents/Glft - Glb/TestExport.gltf");
    }

    async Task<bool> TestExport(string filename)
    {
        var exportSettings = new ExportSettings()
        {
            format = GltfFormat.Json,
            fileConflictResolution = FileConflictResolution.Overwrite,
        };
        
        var export = new GameObjectExport(exportSettings);
        var success = export.AddScene(new[] {gameObject}, name);
        if (success)
        {
            try
            {
                success = await export.SaveToFileAndDispose(filename);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                success = false;
            }
        }
        
        if (!success)
            Debug.LogError("Something went wrong exporting a glTF");
        else
            Debug.Log($"Export success - {filename}");

        return success;
    }

    async Task<bool> TestImport(string filename)
    {
        var success = true;
        try
        {
            using var gltf = new GltfImport();

            var settings = new ImportSettings()
            {
                generateMipMaps = false,
                anisotropicFilterLevel = 3,
                nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
            };

            success = await gltf.Load(filename, settings);
            if (success)
            {
                var root = gltf.GetSourceRoot();
                
                Debug.Log(root.extensions.genericProperties[UNITY_simple_extension_class.name]);
                Debug.Log(root.nodes[0].extensions.genericProperties[UNITY_simple_extension_class.name]);
                
                Debug.Log("gltf - loading done");
                success = await gltf.InstantiateScene(gameObject.transform);
                if (success)
                    Debug.Log("gltf - spawning scene done");
                else
                    Debug.LogError("gltf - error spawning scene");
            }
            else
                Debug.LogError("gltf - error loading");
        }
        catch (Exception e)
        {
            Debug.LogError($"gltf - {e.Message}");
            success = false;
        }

        return success;
    }

    async Task<bool> TestAll(string filename)
    {
        var success = await TestExport(filename);
        if (success)
            success = await TestImport(filename);

        return success;
    }
}
