using System;
using System.Collections.Generic;
using UnityEngine;

namespace LenixSO.Logger
{
    public abstract class LogSettingsSO<T> : ScriptableObject where T: Enum
    {
        [Tooltip("Show previous unseen logs when flags change")]
        public bool restoreLogsOnChange;
        [Tooltip("Logger doesnt show up on stackTrace")]
        public bool ignoreLoggerOnStackTrace;

        [Space] public T activeFlags;
        public List<string> flags = new() { "Flag1" };

        public event Action onFlagsChanged;

        private void OnValidate() => onFlagsChanged?.Invoke();
    }
}