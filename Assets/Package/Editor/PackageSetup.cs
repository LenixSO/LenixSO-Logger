using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace LenixSO.Logger.Editor
{
    public class PackageSetup : UnityEditor.Editor
    {
        [MenuItem("Logger/Import Essentials")]
        public static void MenuImport()
        {
            ImportEssentials();
        }
        //names
        private const string flagEnumName = "LogFlags";
        private const string scriptableName = "LoggerSettingsSO";
        
        //paths
        private static string resourcesPath = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\";
        private static string scriptsPath = Directory.GetCurrentDirectory() + "\\Assets\\Logger\\";
        
        public static void ImportEssentials()
        {
            EditorUtilities.VerifyPath(resourcesPath);
            EditorUtilities.VerifyPath(scriptsPath);
            CreateFlagScript();
            CreateSettingsScriptable();
            CreateLoggerManager();
            
            AssetDatabase.Refresh();
        }

        private static void CreateFlagScript()
        {
            string path = scriptsPath + $"{flagEnumName}.cs";
            GenerateScript(FlagsScript(), path);
        }

        private static void CreateSettingsScriptable()
        {
            string path = scriptsPath + $"{scriptableName}.cs";
            GenerateScript(ScriptableScript(), path);
        }

        private static void CreateLoggerManager()
        {
            string path = scriptsPath + "Logger.cs";
            GenerateScript(LoggerScript(), path);
        }

        private static void GenerateScript(string script, string filePath)
        {
            File.WriteAllText(filePath, script);
        }

        #region Scripts

        private static string FlagsScript()
        {
            StringBuilder sb = new();

            sb.Append("using System;\n");
            sb.Append($"\nnamespace {nameof(LenixSO)}.{nameof(LenixSO.Logger)}");
            sb.Append("\n{");
            sb.Append("\n    [Flags]");
            sb.Append($"\n    public enum {flagEnumName}");
            sb.Append("\n    {");
            sb.Append($"\n        Flag1 = 1,");
            sb.Append("\n    }");
            sb.Append("\n}");

            return sb.ToString();
        }
        private static string ScriptableScript()
        {
            StringBuilder sb = new();

            sb.Append("using System;\n");
            sb.Append($"\nnamespace {nameof(LenixSO)}.{nameof(LenixSO.Logger)}");
            sb.Append("\n{");
            sb.Append($"\n    public class LoggerSettingsSO : LogSettingsSO<{flagEnumName}> {{ }}");
            sb.Append("\n}");

            return sb.ToString();
        }
        private static string LoggerScript()
        {
            StringBuilder sb = new();

            sb.Append("using UnityEngine;\n");
            sb.Append($"\nnamespace {nameof(LenixSO)}.{nameof(LenixSO.Logger)}");
            sb.Append("\n{");
            sb.Append("\n    public static class Logger");
            sb.Append("\n    {");
            sb.Append("\n        private static LogManager<LogFlags> logger;");
            sb.Append("\n        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
            sb.Append("\n        private static void Initialize()");
            sb.Append("\n        {");
            sb.Append("\n            ScenePresence scenePresence = new GameObject(\"Logger\").AddComponent<ScenePresence>();");
            sb.Append("\n            var logSettings = Resources.Load<LoggerSettingsSO>(\"LoggerSettings\");");
            sb.Append("\n#if UNITY_EDITOR");
            sb.Append("\n            //verify logSettings presence");
            sb.Append("\n            if (logSettings == null)");
            sb.Append("\n            {");
            sb.Append("\n                logSettings = Editor.EditorUtilities.CreateScriptable<LoggerSettingsSO>(\"Resources\", \"LoggerSettings\");");
            sb.Append("\n                //Notify");
            sb.Append("\n                Debug.LogError($\"\\\"{logSettings.name}\\\" created in the resources folder because it was not found\");");
            sb.Append("\n            }");
            sb.Append("\n#endif");
            sb.Append("\n            logger = new(logSettings);");
            sb.Append("\n            scenePresence.OnCreated += logger.Initialize;");
            sb.Append("\n            scenePresence.OnDestroyed += logger.Reset;");
            sb.Append("\n            Object.DontDestroyOnLoad(scenePresence.gameObject);");
            sb.Append("\n        }");
            sb.Append("\n        public static void Log(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Log);");
            sb.Append("\n        public static void LogWarning(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Warning);");
            sb.Append("\n        public static void LogError(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Error);");
            sb.Append("\n    }");
            sb.Append("\n}");

            return sb.ToString();
        }

        #endregion
    }

    public static class EditorUtility
    {
        
    }
}