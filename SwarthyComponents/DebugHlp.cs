using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarthyComponents.Debug
{
    public class DebugHlp
    {
        static DateTime startTime;
        public static void Start(string msg)
        {
            startTime = DateTime.Now;
            Console.Write(msg);
        }
        public static void Stop(string msg = " Готово")
        {
            Console.WriteLine("{0} ({1})", msg, DateTime.Now - startTime);
        }
    }
}
