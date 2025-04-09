using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Logger
{
    public class LogManager<T> where T : Enum
    {
        #region Static

        public void GenerateLog(string message, T flag, LogType type)
        {
            if (FlagUtilities.ContainsAnyBits((int)(object)activeFlags, (int)(object)flag) || (int)(object)flag == 0)
            {
                //stacktrace it
                message = StackTraceMessage(message, type);
                LogMessage();
            }
            else
            {
                //split flags
                int[] flags = FlagUtilities.SeparateBits((int)(object)flag);
                //stacktrace it
                message = StackTraceMessage(message, type);
                for (int i = 0; i < flags.Length; i++)
                {
                    T currentFlag = (T)(object)flags[i];

                    LogCashe.TryAdd(currentFlag, null);
                    LogCashe[currentFlag] += CacheLog;
                }

                void CacheLog()
                {
                    //log
                    LogMessage();

                    //remove from lists
                    for (int i = 0; i < flags.Length; i++)
                    {
                        T currentFlag = (T)(object)flags[i];

                        if (LogCashe.ContainsKey(currentFlag))
                            LogCashe[currentFlag] -= CacheLog;
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
        private string StackTraceMessage(string message, LogType type = LogType.Log)
        {
            StringBuilder sb = new(message);
            if (Application.GetStackTraceLogType(type) != StackTraceLogType.None)
            {
                string stackTraceRaw = StackTraceUtility.ExtractStackTrace();
                stackTraceRaw = stackTraceRaw.Remove(0, stackTraceRaw.IndexOf('\n') + 1);

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
                        if (logSettings.ignoreLoggerOnStackTrace)
                        {
                            if (link.Contains($"LogManager.cs") || link.Contains($"Logger.cs")) continue;
                        }

                        stackTrace.Append("\n");
                        stackTrace.Append(traceLines[i]
                            .Replace(link, $"<a href=\"{script}\" line=\"{line}\">{link}</a>"));
                    }
                }

                sb.Append(stackTrace);

                sb.Append("\n.\n.\n.\n<color=orange>Original stackTrace:</color>");
            }

            return sb.ToString();
        }

        #endregion

        #region Local

        private Dictionary<T, Action> LogCashe;
        private LogSettingsSO<T> logSettings;

        private T activeFlags;
        private List<T> cashedFlags;
        
        public LogManager(LogSettingsSO<T> settings)
        {
            logSettings = settings;
            LogCashe = new();
            cashedFlags = GetFlags();
            activeFlags = logSettings.activeFlags;
        }

        public void Initialize()
        {
            logSettings.onFlagsChanged += UpdateLogs;
        }
        public void Reset()
        {
            logSettings.onFlagsChanged -= UpdateLogs;
        }

        private List<T> GetFlags()
        {
            int[] rawFlags = FlagUtilities.SeparateBits((int)(object)logSettings.activeFlags);
            List<T> flags = new(rawFlags.Length);

            for (int i = 0; i < rawFlags.Length; i++)
                flags.Add((T)(object)rawFlags[i]);

            return flags;
        }

        private void UpdateLogs()
        {
            List<T> flags = GetFlags();

            //find differences
            List<T> addedFlags = new(flags.Count);

            for (int i = 0; i < flags.Count; i++)
            {
                if (!cashedFlags.Contains(flags[i]))
                    addedFlags.Add(flags[i]);
            }

            cashedFlags = flags;
            activeFlags = logSettings.activeFlags;

            //Debug added flags
            if (!logSettings.restoreLogsOnChange) return;
            for (int i = 0; i < addedFlags.Count; i++)
            {
                int[] splitFlags = FlagUtilities.SeparateBits((int)(object)addedFlags[i]);
                for (int j = 0; j < splitFlags.Length; j++)
                {
                    T currentFlag = (T)(object)splitFlags[j];

                    if (!LogCashe.ContainsKey(currentFlag)) continue;
                    LogCashe[currentFlag]?.Invoke();
                    LogCashe.Remove(currentFlag);
                }
            }
        }

        #endregion
    }

    public class ScenePresence : MonoBehaviour
    {
        public event Action OnCreated;
        public event Action OnDestroyed;

        private void Start() => OnCreated?.Invoke();
        private void OnDestroy() => OnDestroyed?.Invoke();
    }
}