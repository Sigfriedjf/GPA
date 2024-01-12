using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LMS2.components
{
    public static class Log
    {
        private static StreamWriter _sw = null;
        public static StreamWriter OutputStream { get { return _sw; } }

        static Log()
        {
            //if (Directory.Exists("D:\\LMS2LogDir"))
            //{
            //  _sw = File.CreateText("D:\\LMS2LogDir\\LMS2.txt");
            //  _sw.AutoFlush = true;
            //}
        }

        public static void Write(string s) { if (_sw != null) { _sw.Write(s); } }
        public static void WriteLine(string s) { if (_sw != null) { _sw.WriteLine(s); } }
    }
}
