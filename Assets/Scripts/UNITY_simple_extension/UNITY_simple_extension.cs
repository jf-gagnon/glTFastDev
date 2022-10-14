using GLTFast;
using GLTFast.Export;
using GLTFast.Schema;
using System;
using UnityEngine;

namespace UNITY_simple_extension
{
    /// <summary>
    /// Sample to show how to add custom extension in glTFast
    /// This extension has no API call. It only exposes a new component
    /// which gets included in gltf.
    /// TODO: handle import
    /// </summary>
    public static partial class UNITY_simple_extension_class
    {
        public static string name = "UNITY_simple_extension";

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            CustomPropertyRegistry.Register<RootExtension, RootExtensionData>(name);
            CustomPropertyRegistry.Register<NodeExtensions, NodeExtensionData>(name);

            ExportDelegates.gameObjectAdded += OnGameobjectAdded;
            ExportDelegates.update += OnUpdateGltf;
        }
    }
}