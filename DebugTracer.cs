using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Find
{
    public class DebugTracer
    {
        private const string traceCategory = "Find:";

        public static void DebugTrace(string logText)
        {
            Trace.WriteLine(logText, traceCategory);
        }
    }
}
