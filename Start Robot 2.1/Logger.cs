using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Start_Robot_2._1
{
    public class LoggerOnLogEventArgs : EventArgs
    {
        public List<Log> Logs { get; set; }
    }
    public class LoggerOnUnreadMessagesEventArgs : EventArgs
    {
        public int UnreadMessagesCount { get; set; }
    }
    public class Logger
    {
        public List<Log> Logs { get; private set; }
        public event EventHandler<LoggerOnLogEventArgs> OnLog;
        public event EventHandler<LoggerOnUnreadMessagesEventArgs> OnUnreadMessage;
        public event EventHandler OnMessagesReaded;
        private int LastCheckedLog = 0;

        public Logger()
        {
            Logs = new List<Log>();
        }

        public void Clear()
        {
            Logs.Clear();
            LastCheckedLog = 0;
            OnLog?.Invoke(this, new LoggerOnLogEventArgs
            {
                Logs = Logs
            });
        }

        public void SetLogsChecked()
        {
            LastCheckedLog = Logs.Count;
            OnMessagesReaded?.Invoke(this, null);
        }

        public int GetUncheckedLogsCount()
        {
            return Logs.Count - LastCheckedLog;
        }
        
        public void Write(string message, Symbol symbol = Symbol.Emoji2)
        {
            Logs.Add(new Log
            {
                Time = DateTime.Now,
                Content = message,
                Symbol = symbol
            });
            OnLog?.Invoke(this, new LoggerOnLogEventArgs
            {
                Logs = Logs
            });
            var uncheckedMessages = GetUncheckedLogsCount();
            if (uncheckedMessages > 0)
            {
                OnUnreadMessage?.Invoke(this, new LoggerOnUnreadMessagesEventArgs
                {
                    UnreadMessagesCount = uncheckedMessages
                });
            }
        }
    }
}
