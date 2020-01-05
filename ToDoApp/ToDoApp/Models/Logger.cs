using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace ToDoApp.Models
{
    public class Logger
    {
        private ILog Log;

        public Logger(Type type)
        {
            Log = LogManager.GetLogger(type);
        }

        public void Info(string message)
        {
            Log.Info(message);
        }
        public void Warn(string message)
        {
            Log.Warn(message);
        }
        public void Error(string message)
        {
            Log.Error(message);
        }
        public void Fatal(string message)
        {
            Log.Fatal(message);
        }
    }
}