using System;

namespace CSVParser
{
    /// <summary>
    /// Used with <see cref="CSVLayoutKind.Explicit"/> to specify the position mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CSVFieldPositionAttribute : Attribute
    {
        public int Position { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVFieldPositionAttribute"/> class with the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position to map from the raw CSV.</param>
        public CSVFieldPositionAttribute(int position)
            => Position = position;
    }
}