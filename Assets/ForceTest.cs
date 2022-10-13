using GLTFast;
using GLTFast.Export;
using GLTFast.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
class SimplePair
{
    public string key;
    public int value;
}

[Serializable]
class SimpleRootExtension
{
    public SimplePair[] datas;
}

[Serializable]
class SimpleNodeExtension
{
    public int data;
}


class GltfExportHelper
{
    internal GameObjectExportDelegates gameObjectExportDelegates { get; } = new GameObjectExportDelegates();
    internal ExportDelegates exportDelegates { get; } = new ExportDelegates();

    List<(SimpleRootExtension rootData, int nodeId, SimpleNodeExtension nodeData)> m_Data;

    internal GltfExportHelper()
    {
        gameObjectExportDelegates.gameObjectAdded += OnGameobjectAdded;
        gameObjectExportDelegates.reset += OnReset;
        exportDelegates.update += OnUpdate;
        exportDelegates.dispose += OnDispose;
    }

    void OnReset()
    {
        m_Data = new List<(SimpleRootExtension, int, SimpleNodeExtension)>();
    }

    void OnGameobjectAdded(GameObject gameObject, int nodeId)
    {
        if (gameObject.TryGetComponent<ForceTest>(out var dummy))
        {
            var rootData = new SimpleRootExtension()
            {
                datas = new SimplePair[]
                {
                    new SimplePair()
                    {
                        key = "my_key_0",
                        value = -999,
                    },
                    new SimplePair()
                    {
                        key = "my_key_1",
                        value = -111,
                    }
                }
            };

            var nodeData = new SimpleNodeExtension()
            {
                data = m_Data.Count,
            };
            
            m_Data.Add((rootData, nodeId, nodeData));
        }
    }

    void OnUpdate(Root root, IGltfWritable gltf)
    {
        if (!m_Data.Any())
            return;
        
        var nodes = root.nodes;
        if (nodes == null)
            return;
        
        if (root.extensions == null)
            root.extensions = new RootExtension();
        var rootExtension = root.extensions;
        rootExtension.genericProperties ??= new Dictionary<string, object>();
        
        m_Data.ForEach(data =>
        {
            rootExtension.genericProperties.Add(ForceTest.customExtensionName, data.rootData);

            // this is not correct, should query with id
            var node = nodes[data.nodeId];
            if (node != null)
            {
                node.extensions ??= new NodeExtensions();
                node.extensions.genericProperties ??= new Dictionary<string, object>(); 
                node.extensions.genericProperties.Add(ForceTest.customExtensionName, data.nodeData);
            }
        });
        
        // review this whole delegate thing
        gltf.RegisterExtensionUsage(ForceTest.customExtensionName, true);
    }

    void OnDispose()
    {
        m_Data = null;
    }
}

public class ForceTest : MonoBehaviour
{
    internal const string customExtensionName = "UNITY_simple_extension";
    
    GltfExportHelper m_GltfExportHelper;
    
    void Start()
    {
        CustomExtensionRegistry.RegisterSimpleParser<RootExtension, SimpleRootExtension>(customExtensionName);
        CustomExtensionRegistry.RegisterSimpleParser<NodeExtensions, SimpleNodeExtension>(customExtensionName);

        m_GltfExportHelper = new GltfExportHelper();
        
        //TestImport("C:/Users/jeanfrancois.gagnon/Documents/Glft - Glb/SimpleLightsRig.gltf");
        TestExport("C:/Users/jeanfrancois.gagnon/Documents/Glft - Glb/TestExport.gltf");
    }

    async Task<bool> TestExport(string filename)
    {
        var exportSettings = new ExportSettings()
        {
            format = GltfFormat.Json,
            fileConflictResolution = FileConflictResolution.Overwrite,
        };
        
        var export = new GameObjectExport(exportSettings,
            exportDelegates: m_GltfExportHelper.exportDelegates,
            gameObjectExportDelegates: m_GltfExportHelper.gameObjectExportDelegates);
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

        return false;
    }

    async Task<bool> TestImport(string filename)
    {
        try
        {
            using var gltf = new GLTFast.GltfImport();

            // Create a settings object and configure it accordingly
            var settings = new GLTFast.ImportSettings()
            {
                generateMipMaps = false,
                anisotropicFilterLevel = 3,
                nodeNameMethod = GLTFast.ImportSettings.NameImportMethod.OriginalUnique
            };

            var success = await gltf.Load(filename, settings);
            if (success)
            {
                var root = gltf.GetSourceRoot();
                
                Debug.Log(root.extensions.genericProperties[customExtensionName]);
                Debug.Log(root.nodes[0].extensions.genericProperties[customExtensionName]);
                
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
        }

        return false;
    }
}
