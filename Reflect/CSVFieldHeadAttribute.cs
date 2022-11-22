using System;

namespace CSVParser
{
    /// <summary>
    /// Used with <see cref="CSVLayoutKind.Explicit"/> to specify the head mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CSVFieldHeadAttribute : Attribute
    {
        public string Head { get; private set; }
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVFieldPositionAttribute"/> class with the specified <paramref name="head"/>.
        /// </summary>
        /// <param name="head">The head to map from the raw CSV.</param>
        public CSVFieldHeadAttribute(string head)
        {
            Head = head;
            IgnoreCase = true;
        }
    }
}