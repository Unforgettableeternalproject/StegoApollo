using System;
using System.Collections.Generic;
using System.Text;

namespace StegoLib.Utilities
{
    public class LogManager
    {
        // 單例模式
        private static readonly Lazy<LogManager> _instance = new Lazy<LogManager>(() => new LogManager());
        public static LogManager Instance => _instance.Value;

        // 日誌列表
        private List<LogEntry> _logEntries;

        // 日誌類型枚舉
        public enum LogType
        {
            Info,
            Warning,
            Error,
            Success
        }

        // 日誌條目類
        public class LogEntry
        {
            public DateTime Timestamp { get; }
            public string Message { get; }
            public LogType Type { get; }

            public LogEntry(string message, LogType type)
            {
                Timestamp = DateTime.Now;
                Message = message;
                Type = type;
            }

            public override string ToString()
            {
                string prefix;
                switch (Type)
                {
                    case LogType.Info:
                        prefix = "[訊息]";
                        break;
                    case LogType.Warning:
                        prefix = "[警告]";
                        break;
                    case LogType.Error:
                        prefix = "[錯誤]";
                        break;
                    case LogType.Success:
                        prefix = "[成功]";
                        break;
                    default:
                        prefix = "[未知]";
                        break;
                }
                return $"{Timestamp:HH:mm:ss} {prefix} {Message}";
            }
        }

        private LogManager()
        {
            _logEntries = new List<LogEntry>();
        }

        public void Clear()
        {
            _logEntries.Clear();
        }

        public void Log(string message, LogType type = LogType.Info)
        {
            _logEntries.Add(new LogEntry(message, type));
            // 這裡可以添加事件通知機制，通知UI更新
        }

        public void LogInfo(string message)
        {
            Log(message, LogType.Info);
        }

        public void LogWarning(string message)
        {
            Log(message, LogType.Warning);
        }

        public void LogError(string message)
        {
            Log(message, LogType.Error);
        }

        public void LogSuccess(string message)
        {
            Log(message, LogType.Success);
        }

        public List<LogEntry> GetLogs()
        {
            return _logEntries;
        }

        public string GetFormattedLogs()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in _logEntries)
            {
                sb.AppendLine(entry.ToString());
            }
            return sb.ToString();
        }
    }
}