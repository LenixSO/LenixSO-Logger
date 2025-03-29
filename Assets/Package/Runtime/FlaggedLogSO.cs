using System;
using System.Collections.Generic;
using UnityEngine;

namespace LenixSO.Logger
{
    [CreateAssetMenu(menuName = "FlaggedLogSettings", fileName = "new FlaggedLogSettings")]
    public class FlaggedLogSO : ScriptableObject
    {
        [Tooltip("Show previous unseen logs when flags change")]
        public bool restoreLogsOnChange;
        [Tooltip("Logger doesnt show up on stackTrace")]
        public bool ignoreLoggerOnStackTrace;

        [Space] public LogFlags activeFlags;
        public List<string> flags = new() { "Flag1" };

        public event Action onFlagsChanged;

        private void OnValidate() => onFlagsChanged?.Invoke();
    }
}