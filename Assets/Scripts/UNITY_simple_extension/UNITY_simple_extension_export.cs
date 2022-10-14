using GLTFast.Export;
using GLTFast.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UNITY_simple_extension
{
    public static partial class UNITY_simple_extension_class
    {
        class ExportData
        {
            internal List<(RootExtensionData rootData, int nodeId, NodeExtensionData nodeData)> list;
        }

        static void OnGameobjectAdded(IGltfWritable gltf, GameObject gameObject, int nodeId)
        {
            if (gltf == null
                || gameObject == null
                || !gameObject.TryGetComponent<SimpleExtensionComponent>(out var component)
                || component.pairs == null
                || component.pairs.Length == 0)
            {
                return;
            }

            var exportData = gltf.GetCustomData<ExportData>(name);
            if (exportData == null)
            {
                exportData = new ExportData();
                gltf.SetCustomData(name, exportData);
            }

            exportData.list ??= new List<(RootExtensionData rootData, int nodeId, NodeExtensionData nodeData)>();

            var rootData = new RootExtensionData()
            {
                datas = component.pairs,
            };

            var nodeData = new NodeExtensionData()
            {
                data = exportData.list.Count,
            };
            
            exportData.list.Add((rootData, nodeId, nodeData));
        }

        static void OnUpdateGltf(IGltfWritable gltf)
        {
            if (gltf == null)
                return;
            
            var exportData = gltf.GetCustomData<ExportData>(name);
            if (exportData == null)
                return;
            
            if (!exportData.list.Any())
                return;

            var rootExtension = gltf.GetRootExtension();
            rootExtension.genericProperties ??= new Dictionary<string, object>();
            
            exportData.list.ForEach(data =>
            {
                var node = gltf.GetNode(data.nodeId);
                if (node != null)
                {
                    node.extensions ??= new NodeExtensions();
                    node.extensions.genericProperties ??= new Dictionary<string, object>(); 
                    node.extensions.genericProperties.Add(name, data.nodeData);
                }

                rootExtension.genericProperties.Add(name, data.rootData);
            });

            gltf.RegisterExtensionUsage(name, true);
        }
    }
}