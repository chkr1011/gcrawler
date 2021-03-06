﻿using System;

namespace GCrawler
{
    internal static class Tracer
    {
        private static readonly object SyncRoot = new object();

        public static void Write(ConsoleColor color, string text, params object[] parameters)
        {
            string line = string.Format(
                "{0}: {1}", 
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), 
                string.Format(text, parameters));

            lock (SyncRoot)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(line);
            }
        }

        public static void WriteVerbose(string text, params object[] parameters)
        {
            Write(ConsoleColor.Gray, text, parameters);
        }

        public static void WriteHint(string text, params object[] parameters)
        {
            Write(ConsoleColor.DarkMagenta, text, parameters);
        }

        public static void WriteInformation(string text, params object[] parameters)
        {
            Write(ConsoleColor.DarkGreen, text, parameters);
        }

        public static void WriteWarning(string text, params object[] parameters)
        {
            Write(ConsoleColor.DarkYellow, text, parameters);
        }
    }
}
