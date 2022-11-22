using System;
using System.Diagnostics;

namespace CSVParser
{
    [DebuggerStepThrough]
    public class CSVFieldFormatException : Exception
    {
        internal CSVFieldFormatException(string reason) : base($"Bad format: {reason}") {}
    }
}