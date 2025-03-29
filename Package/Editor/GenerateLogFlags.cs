using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace LenixSO.Logger
{
    [CustomEditor(typeof(FlaggedLogSO))]
    public class GenerateLogFlags : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Flags"))
                GenerateFlags();
        }

        private void GenerateFlags()
        {
            IEnumerable<string> found = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "LogFlags.cs", SearchOption.AllDirectories);
            for (int i = 0; i < found.Count();)
            {
                string filePath = found.ElementAt(i);

                File.WriteAllText(filePath, FlagsScript());
                AssetDatabase.Refresh();
                break;
            }
        }

        private string FlagsScript()
        {
            FlaggedLogSO logFlags = (FlaggedLogSO)target;
            List<string> flags = logFlags.flags;
            List<string> addedFlags = new(flags.Count);
            StringBuilder sb = new();

            sb.Append("using System;\n");
            sb.Append($"\nnamespace {nameof(LenixSO)}.{nameof(LenixSO.Logger)}");
            sb.Append("\n{");
            sb.Append("\n    [Flags]");
            sb.Append("\n    public enum LogFlags");
            sb.Append("\n    {");
            int flagValue = 1;
            for (int i = 0; i < flags.Count; i++)
            {
                if (addedFlags.Contains(flags[i]))
                {
                    Debug.LogWarning($"Flag at index {i}({flags[i]}) already exists, it will be ignored.");
                    continue;
                }
                sb.Append($"\n        {flags[i]} = {flagValue},");
                addedFlags.Add(flags[i]);
                flagValue <<= 1;
            }
            sb.Append("\n    }");
            sb.Append("\n}");

            return sb.ToString();
        }
    }
}