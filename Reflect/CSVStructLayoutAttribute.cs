using System;

namespace CSVParser
{
    /// <summary>
    /// Enables you to control the CSV parsing flow.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class CSVStructLayoutAttribute : Attribute
    {
        public CSVLayoutKind Kind { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVStructLayoutAttribute"/> class with the specified <paramref name="kind"/>.
        /// </summary>
        public CSVStructLayoutAttribute(CSVLayoutKind kind)
            => Kind = kind;
    }
}