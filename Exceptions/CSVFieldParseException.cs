using System;
using System.Diagnostics;

namespace CSVParser
{
    [DebuggerStepThrough]
    public class CSVFieldParseException : Exception
    {
        internal CSVFieldParseException(string fieldValue, Type fieldType) : base($"Cannot parse \"{fieldValue}\" to \"{fieldType}\".") { }
    }
}
