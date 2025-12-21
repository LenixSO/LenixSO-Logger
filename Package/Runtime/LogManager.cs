using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LenixSO.Logger
{
    public class LogManager<T> where T : Enum
    {
        private Dictionary<T, Action> logCache;
        private List<Action> defaultLogCache;
        private LogSettingsSO<T> logSettings;

        private T activeFlags;
        private List<T> cashedFlags;

        private LogHandler logHandler;
        private ILogHandler debugHandler => logHandler.handler;

        public LogManager(LogSettingsSO<T> settings)
        {
            logSettings = settings;
            logCache = new();
            defaultLogCache = new();
            cashedFlags = GetFlags();
            activeFlags = logSettings.activeFlags;
        }

        public void Initialize()
        {
            logHandler = new LogHandler(Debug.unityLogger.logHandler);
            Debug.unityLogger.logHandler = logHandler;
            logHandler.onLog += HandleDefaultLogs;
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

            //restore default logs
            for (int i = 0; i < defaultLogCache.Count; i++)
                defaultLogCache[i]?.Invoke();
            defaultLogCache.Clear();

            //restore flagged logs
            for (int i = 0; i < addedFlags.Count; i++)
            {
                int[] splitFlags = FlagUtilities.SeparateBits((int)(object)addedFlags[i]);
                for (int j = 0; j < splitFlags.Length; j++)
                {
                    T currentFlag = (T)(object)splitFlags[j];

                    if (!logCache.ContainsKey(currentFlag)) continue;
                    logCache[currentFlag]?.Invoke();
                    logCache.Remove(currentFlag);
                }
            }
        }

        public void GenerateLog(string message, T flag, LogType type)
        {
            List<string> ignoredStacks = new();
            //skip if is ignoring logger trace
            if (logSettings.ignoreLoggerOnStackTrace)
            {
                ignoredStacks.Add("Logger.cs");
                ignoredStacks.Add("LogManager.cs");
            }
            //custom stacktrace
            message = StackTraceMessage(message, type, ignoredStacks);

            if ((int)(object)flag != 0 && !FlagUtilities.ContainsAnyBits((int)(object)activeFlags, (int)(object)flag))
            {
                //split flags
                int[] flags = FlagUtilities.SeparateBits((int)(object)flag);
                for (int i = 0; i < flags.Length; i++)
                {
                    T currentFlag = (T)(object)flags[i];

                    logCache.TryAdd(currentFlag, null);
                    logCache[currentFlag] += CacheLog;
                }

                void CacheLog()
                {
                    //log
                    LogMessage();

                    //remove from lists
                    for (int i = 0; i < flags.Length; i++)
                    {
                        T currentFlag = (T)(object)flags[i];

                        if (logCache.ContainsKey(currentFlag))
                            logCache[currentFlag] -= CacheLog;
                    }
                }
            }
            else LogMessage();

            return;

            void LogMessage()
            {
                debugHandler.LogFormat(type, null, message);
            }
        }

        private void HandleDefaultLogs(LogType type, Object context, string format, params object[] args)
        {
            List<string> ignoredStacks = new();
            //skip if is ignoring logger trace
            if (logSettings.ignoreLoggerOnStackTrace)
            {
                ignoredStacks.Add("Logger.cs");
                ignoredStacks.Add("LogManager.cs");
            }

            string message = string.Format(format, args);
            //custom stacktrace
            message = StackTraceMessage(message, type, ignoredStacks);
            if (logSettings.supressRegularLogs) defaultLogCache.Add(() => debugHandler.LogFormat(type, context, message));
            else debugHandler.LogFormat(type, context, message);
        }

        private static string StackTraceMessage(string message, LogType type = LogType.Log, List<string> ignoredTrace = null)
        {
            StringBuilder sb = new(message);
            if (Application.GetStackTraceLogType(type) == StackTraceLogType.None) return sb.ToString();
            ignoredTrace ??= new();
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
                if (atIndex < 0 || endIndex <= atIndex) continue;
                atIndex += 4;
                string link = traceLines[i]
                    .Substring(atIndex, endIndex - atIndex);
                int colonId = link.IndexOf(':');
                string script = link.Substring(0, colonId);
                string line = link.Substring(colonId + 1, link.Length - colonId - 1);
                if (ignoredTrace.Any(l => script.Contains(l))) continue;

                stackTrace.Append("\n");
                stackTrace.Append(traceLines[i]
                    .Replace(link, $"<a href=\"{script}\" line=\"{line}\">{link}</a>"));
            }

            sb.Append(stackTrace);

            sb.Append("\n.\n.\n.\n<color=orange>Original stackTrace:</color>");

            return sb.ToString();
        }
    }

    public class ScenePresence : MonoBehaviour
    {
        public event Action OnCreated;
        public event Action OnDestroyed;

        private void Start() => OnCreated?.Invoke();
        private void OnDestroy() => OnDestroyed?.Invoke();
    }

    public class LogHandler : ILogHandler
    {
        public ILogHandler handler { get; }

        public delegate void LogEvent(LogType type, Object context, string format, params object[] args);

        public event LogEvent onLog;
        
        public LogHandler(ILogHandler handler)
        {
            this.handler = handler;
        }
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            onLog?.Invoke(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            handler.LogException(exception, context);
        }
    }
}