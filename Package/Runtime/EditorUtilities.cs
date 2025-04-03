#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LenixSO.Logger.Editor
{
    public static class EditorUtilities
    {
        public static void VerifyPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        /// <param name="path">Path starting from asset.</param>
        /// <returns></returns>
        public static T CreateScriptable<T>(string path, string filename) where T : ScriptableObject
        {
            T so = ScriptableObject.CreateInstance<T>();
            string fullPath = Directory.GetCurrentDirectory() + $"\\Assets\\{path}";
            VerifyPath(fullPath);
            AssetDatabase.CreateAsset(so, $"Assets\\{path}\\{filename}.asset");
            AssetDatabase.Refresh();
            return so;
        }
        
        /// <param name="path">Path starting from asset.</param>
        /// <returns></returns>
        public static void CreateScriptable(string path, string filename, string scriptableType)
        {
            ScriptableObject so = ScriptableObject.CreateInstance(scriptableType);
            string fullPath = Directory.GetCurrentDirectory() + $"\\Assets\\{path}";
            VerifyPath(fullPath);
            AssetDatabase.CreateAsset(so, $"Assets\\{path}\\{filename}.asset");
            AssetDatabase.Refresh();
        }
    }
}
#endif