using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyComponents
{
    public static class Log
    {
        public static bool Silent = false;
        public static void Msg(string format = "", params object[] args)
        {
            if (Silent) return;
            Console.Write(format, args);
        }
        public static void Warn(string format = "", params object[] args)
        {
            if (Silent) return;
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(format, args);
            Console.ForegroundColor = oldColor;
        }
        public static void Err(string format = "", params object[] args)
        {
            if (Silent) return;
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(format, args);
            Console.ForegroundColor = oldColor;
        }
        public static void MsgLn(string format = "", params object[] args)
        {
            Msg(format + Environment.NewLine, args);
        }
        public static void WarnLn(string format = "", params object[] args)
        {
            Warn(format + Environment.NewLine, args);
        }
        public static void ErrLn(string format = "", params object[] args)
        {
            Err(format + Environment.NewLine, args);
        }
    }
}
