using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSVParser
{
    /// <summary>
    /// Class representing a CSV record.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class CSVRecord : IEquatable<CSVRecord>
    {
        #region STATIC_METHODS

        /// <summary>
        /// Escapes the <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to escape.</param>
        private static string EscapeField(string field)
        {
            if (field.Contains("\""))
                field = field.Replace("\"", "\"\"");
            if (field.Contains("\"") || field.Contains("\r") || field.Contains("\n") || field.Contains(","))
                field = $"\"{field}\"";
            return field;
        }

        #endregion

        #region PUBLIC_PROPERTIES

        /// <summary>
        /// Gets the fields.
        /// </summary>
        public IReadOnlyList<string> Fields { get; private set; }

        /// <summary>
        /// Gets the total ammount of fields.
        /// </summary>
        public int Length { get => Fields.Count; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Creates a new instance of a <see cref="CSVRecord"/> with the specified <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The record fields.</param>
        public CSVRecord(IReadOnlyList<string> fields)
            => Fields = fields;

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Escapes all the fields and then join them with a comma.
        /// </summary>
        public string Escape()
        {
            string[] escaped = new string[Length];
            for (int i = 0; i < Length; i++)
                escaped[i] = EscapeField(Fields[i]);
            return string.Join(",", escaped);
        }

        public int IndexOf(string value, bool ignoreCase = false)
        {
            value = ignoreCase ? value.ToLower() : value;

            if (ignoreCase) {
                for (int i = 0; i < Length; i++)
                    if (Fields[i].ToLower() == value)
                        return i;
            } else {
                for (int i = 0; i < Length; i++)
                    if (Fields[i] == value)
                        return i;
            }
            return -1;
        }

        #endregion

        #region IEQUATABLE_IMPL

        /// <summary>
        /// Compares two records and check if they are equal or not.
        /// </summary>
        /// <param name="other"></param>
        public bool Equals(CSVRecord other)
        {
            if (other is null || other.Fields is null || Fields is null || other.Length != Length)
                return false;
            for (int i = 0; i < Length; i++)
                if (Fields[i] != other.Fields[i])
                    return false;
            return true;
        }

        #endregion
    }
}