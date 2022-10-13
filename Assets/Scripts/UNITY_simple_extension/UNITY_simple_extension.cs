using GLTFast;
using GLTFast.Export;
using GLTFast.Schema;
using System;
using UnityEngine;

namespace UNITY_simple_extension
{
    [Serializable]
    public class KeyValuePair
    {
        public string key;
        public int value;
    }

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