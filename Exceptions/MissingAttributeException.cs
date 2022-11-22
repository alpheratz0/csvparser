using System;
using System.Diagnostics;

namespace CSVParser
{
    [DebuggerStepThrough]
    public class MissingAttributeException : Exception
    {
        public MissingAttributeException(Type target, Type attributeMissed) : base($"Expecting \"{attributeMissed}\" on \"{target}\", not found.") { }
    }
}
