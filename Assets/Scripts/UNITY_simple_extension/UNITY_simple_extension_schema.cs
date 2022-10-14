using System;
using UnityEngine;

namespace UNITY_simple_extension.Schema
{
    [Serializable]
    public class KeyValuePair
    {
        public string key;
        public int value;
    }
    
    [Serializable]
    public class RootExtensionData
    {
        public KeyValuePair[] datas;
    }

    [Serializable]
    public class NodeExtensionData
    {
        [System.ComponentModel.DefaultValue(-1)]
        public int data = -1;
    }
}