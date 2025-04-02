using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace LenixSO.Logger.Editor
{
    [CustomEditor(typeof(LogSettingsSO<>), editorForChildClasses:true)]
    public class GenerateLogFlags : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Flags"))
                GenerateFlags();
        }

        private void GenerateFlags()
        {
            string flagName = target.GetType().GetField("activeFlags").FieldType.Name;
            IEnumerable<string> found = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), 
                $"{flagName}.cs", SearchOption.AllDirectories);
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
            List<string> flags = (List<string>)target.GetType().GetField("flags")?.GetValue(target);
            List<string> addedFlags = new(flags?.Count ?? 0);
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