using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;

namespace TrackGridlineLocation
{
    public class Logger
    {
        Stopwatch stopwatch;
        string LoggerName { get; set; }
        public Logger(string str)
        {
            if (!System.Environment.UserName.ToUpper().Contains("CROWE")) return;
            this.LoggerName = str;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }
        public static void Log(string message)
        {
            if (!System.Environment.UserName.ToUpper().Contains("CROWE")) return;
            string fileName = "DataLog.txt";
            string docName = GetMyDocs(fileName);

            var lines = new List<string>() { $"{DateTime.Now.ToString("yyyyMMddhhmmss")} - {message}" };
            File.AppendAllLines(docName, lines);
        }
        private static string GetMyDocs(string Subdir)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string subdir = directory + "\\" + Subdir;
            return subdir;
        }
    }
}
