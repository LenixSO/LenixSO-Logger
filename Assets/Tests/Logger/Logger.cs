using UnityEngine;

namespace LenixSO.Logger
{
    public static class Logger
    {
        private static LogManager<LogFlags> logger;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ScenePresence scenePresence = new GameObject("Logger").AddComponent<ScenePresence>();
            var logSettings = Resources.Load<LoggerSettingsSO>("LoggerSettingsSO");
#if UNITY_EDITOR
            //verify logSettings presence
            if (logSettings == null)
            {
                logSettings = Editor.EditorUtilities.CreateScriptable<LoggerSettingsSO>("Resources", "LoggerSettingsSO");
                //Notify
                Debug.LogError($"\"{logSettings.name}\" created in the resources folder because it was not found");
            }
#endif
            logger = new(logSettings);
            scenePresence.OnCreated += logger.Initialize;
            scenePresence.OnDestroyed += logger.Reset;
            Object.DontDestroyOnLoad(scenePresence.gameObject);
        }
        public static void Log(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Log);
        public static void LogWarning(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Warning);
        public static void LogError(string message, LogFlags flag = 0) => logger.GenerateLog(message, flag, LogType.Error);
    }
}