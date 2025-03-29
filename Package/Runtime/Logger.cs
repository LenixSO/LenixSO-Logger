using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Logger
{
    public class Logger : MonoBehaviour
    {
        #region Static

        private static Logger logger;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            logger = new GameObject().AddComponent<Logger>();
            DontDestroyOnLoad(logger.gameObject);
        }

        public static void Log(string message, LogFlags flag = 0) => GenerateLog(message, flag, LogType.Log);
        public static void LogWarning(string message, LogFlags flag = 0) => GenerateLog(message, flag, LogType.Warning);
        public static void LogError(string message, LogFlags flag = 0) => GenerateLog(message, flag, LogType.Error);

        private static void GenerateLog(string message, LogFlags flag, LogType type)
        {
            if (FlagUtilities.ContainsAnyBits((int)logger.activeFlags, (int)flag) || flag == 0)
            {
                LogMessage();
            }
            else
            {
                //split flags
                int[] flags = FlagUtilities.SeparateBits((int)flag);
                //stacktrace it
                message = StackTraceMessage(message, type);
                for (int i = 0; i < flags.Length; i++)
                {
                    LogFlags currentFlag = (LogFlags)flags[i];
                    
                    logger.LogCashe.TryAdd(currentFlag, null);
                    logger.LogCashe[currentFlag] += CacheLog; 
                }

                void CacheLog()
                {
                    //log
                    LogMessage();
                    
                    //remove from lists
                    for (int i = 0; i < flags.Length; i++)
                    {
                        LogFlags currentFlag = (LogFlags)flags[i];

                        if (logger.LogCashe.ContainsKey(currentFlag))
                            logger.LogCashe[currentFlag] -= CacheLog;
                    }
                }
            }

            return;

            void LogMessage()
            {
                switch (type)
                {
                    case LogType.Warning:
                        Debug.LogWarning(message);
                        break;
                    case LogType.Error:
                        Debug.LogError(message);
                        break;
                    default:
                        Debug.Log(message);
                        break;
                }
            }
        }
        private static string StackTraceMessage(string message, LogType type = LogType.Log)
        {
            StringBuilder sb = new(message);
            if (Application.GetStackTraceLogType(type) != StackTraceLogType.None)
            {
                string stackTraceRaw = StackTraceUtility.ExtractStackTrace();
                stackTraceRaw = stackTraceRaw.Remove(0, stackTraceRaw.IndexOf('\n') + 1);
                if (logger.logSettings.ignoreLoggerOnStackTrace)
                {
                    //remove logger from stackTrace
                }

                StringBuilder stackTrace = new();
                
                //Add UnityEngine.Debug.Log to stacktrace
                stackTrace.Append("\nUnityEngine.Debug.Log");
                //Add different log types
                if (type != LogType.Log) stackTrace.Append(type.ToString());
                stackTrace.Append(" (object)");
                
                //Split stackTrace lines to add link to 
                string[] traceLines = stackTraceRaw.Split('\n');
                for (int i = 0; i < traceLines.Length; i++)
                {
                    int atIndex = traceLines[i].IndexOf("(at ", StringComparison.Ordinal);
                    int endIndex = traceLines[i].LastIndexOf(')');
                    if (atIndex >= 0 && endIndex > atIndex)
                    {
                        atIndex += 4;
                        string link = traceLines[i]
                            .Substring(atIndex, endIndex - atIndex);
                        int colonId = link.IndexOf(':');
                        string script = link.Substring(0, colonId);
                        string line = link.Substring(colonId + 1, link.Length - colonId - 1);
                        
                        //skip if is ignoring logger trace
                        if(link.Contains("Logger.cs") && logger.logSettings.ignoreLoggerOnStackTrace) continue;
                        
                        stackTrace.Append("\n");
                        stackTrace.Append(traceLines[i].Replace(link, $"<a href=\"{script}\" line=\"{line}\">{link}</a>"));
                    }
                }
                sb.Append(stackTrace);

                sb.Append("\n\nOriginal stack:");
            }
            return sb.ToString();
        }

        #endregion

        #region Local

        private Dictionary<LogFlags, Action> LogCashe;
        private FlaggedLogSO logSettings;

        private LogFlags activeFlags;
        private List<LogFlags> cashedFlags;

        private void Awake()
        {
            LogCashe = new();
            logSettings = Resources.Load<FlaggedLogSO>("FlaggedLogSettings");
            #if UNITY_EDITOR
            if (logSettings == null)
            {
                FlaggedLogSO cache = ScriptableObject.CreateInstance<FlaggedLogSO>();
                string root = System.IO.Directory.GetCurrentDirectory();
                if (!System.IO.Directory.Exists($"{root}\\Assets\\Resources"))
                    System.IO.Directory.CreateDirectory($"{root}\\Assets\\Resources");
                UnityEditor.AssetDatabase.CreateAsset(cache, "Assets\\Resources\\FlaggedLogSettings.asset");
                UnityEditor.AssetDatabase.Refresh();
                logSettings = cache;
                
                //Notify
                Debug.LogError($"\"{cache.name}\" created in the resources folder because it was not found");
            }
            #endif
            cashedFlags = GetFlags();
            activeFlags = logSettings.activeFlags;

            logSettings.onFlagsChanged += UpdateLogs;
        }
        private void OnDestroy()
        {
            logSettings.onFlagsChanged -= UpdateLogs;
        }

        private List<LogFlags> GetFlags()
        {
            int[] rawFlags = FlagUtilities.SeparateBits((int)logSettings.activeFlags);
            List<LogFlags> flags = new(rawFlags.Length);

            for (int i = 0; i < rawFlags.Length; i++)
                flags.Add((LogFlags)rawFlags[i]);

            return flags;
        }

        private void UpdateLogs()
        {
            List<LogFlags> flags = GetFlags();

            //find differences
            List<LogFlags> addedFlags = new(flags.Count);

            for (int i = 0; i < flags.Count; i++)
            {
                if (!cashedFlags.Contains(flags[i]))
                {
                    addedFlags.Add(flags[i]);
                }
            }

            cashedFlags = flags;
            activeFlags = logSettings.activeFlags;

            //Debug added flags
            if (!logSettings.restoreLogsOnChange) return;
            for (int i = 0; i < addedFlags.Count; i++)
            {
                int[] splitFlags = FlagUtilities.SeparateBits((int)addedFlags[i]);
                for (int j = 0; j < splitFlags.Length; j++)
                {
                    LogFlags currentFlag = (LogFlags)splitFlags[j];

                    if(!LogCashe.ContainsKey(currentFlag)) continue;
                    LogCashe[currentFlag]?.Invoke();
                    LogCashe.Remove(currentFlag);
                }
            }
        }

        #endregion
    }
}