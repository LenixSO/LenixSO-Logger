using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LenixSO.Logger
{
    public abstract class LogSettingsSO<T> : ScriptableObject where T: Enum
    {
        [Tooltip("Show previous unseen logs when flags change")]
        public bool restoreLogsOnChange;
        [Tooltip("Logger doesnt show up on stackTrace")]
        public bool ignoreLoggerOnStackTrace;
        [Tooltip("Regular Debug.Log messages doesnt show up")]
        public bool supressRegularLogs;

        [Space] public T activeFlags;
        public List<string> flags = new() { "Flag1" };

        public event Action onFlagsChanged;

        private void OnValidate() => onFlagsChanged?.Invoke();

        private void Reset()
        {
            restoreLogsOnChange = true;
            ignoreLoggerOnStackTrace = true;
            supressRegularLogs = false;
            var values = Enum.GetNames(typeof(T));
            var parseValue = new StringBuilder();
            flags = new(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                flags.Add(values[i]);
                if (i > 0) parseValue.Append(", ");
                parseValue.Append(values[i]);
            }

            activeFlags = (T)Enum.Parse(typeof(T), parseValue.ToString());
        }
    }
}