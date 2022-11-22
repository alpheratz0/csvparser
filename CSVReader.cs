using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace CSVParser
{
    public delegate string FieldPreProccessor(string field);

    /// <summary>
    /// This class provides basic CSV reading functionality.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class CSVReader : IDisposable
    {
        #region CONSTANTS

        // Carriage return.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const char CR = (char)0x0000000D;

        // Line feed.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const char LF = (char)0x0000000A;

        // Double quote.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const char DQUOTE = (char)0x00000022;

        // Comma.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const char COMMA = (char)0x0000002C;

        // End of file.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private const int EOF = -1;

        // State: None.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private const int ST_NONE = 0x00000000;

        // State: Inside quotes.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private const int ST_INSIDE_QUOTES = 0x10000022;

        // State: Waiting for comma.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private const int ST_WAITING_COMMA = 0x1000002C;

        // State: Previous was dquote.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const int ST_PREVWASDQOTE = 0x10000222;

        #endregion

        #region STATIC_METHODS

        private static object ParseRecordField(string prop, Type res, IFormatProvider provider)
        {
            try
            {
                if (res.IsEnum) return Enum.Parse(res, prop);
                if (res == typeof(string)) return prop;
                if (res == typeof(bool)) return bool.Parse(prop);
                if (res == typeof(char)) return char.Parse(prop);
                if (res == typeof(sbyte)) return sbyte.Parse(prop, provider);
                if (res == typeof(byte)) return byte.Parse(prop, provider);
                if (res == typeof(decimal)) return decimal.Parse(prop, provider);
                if (res == typeof(double)) return double.Parse(prop, provider);
                if (res == typeof(float)) return float.Parse(prop, provider);
                if (res == typeof(int)) return int.Parse(prop, provider);
                if (res == typeof(uint)) return uint.Parse(prop, provider);
                if (res == typeof(long)) return long.Parse(prop, provider);
                if (res == typeof(ulong)) return ulong.Parse(prop, provider);
                if (res == typeof(short)) return short.Parse(prop, provider);
                if (res == typeof(ushort)) return ushort.Parse(prop, provider);
                if (res == typeof(DateTime)) return DateTime.Parse(prop, provider);
            }
            catch (FormatException) { throw new CSVFieldParseException(prop, res); }
            throw new CSVFieldParseException(prop, res);
        }

        #endregion

        #region PUBLIC_PROPERTIES

        /// <summary>
        /// Gets or sets the actual position where the <see cref="CSVReader"/> is reading.
        /// </summary>
        public int Position { get => m_position; set => SetPosition(value); }

        public FieldPreProccessor FieldPreProccessor { get; set; }

        #endregion

        #region PRIVATE_FIELDS

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool m_disposed;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int m_position;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string m_path;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool m_hasHead;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly CSVRecord m_head;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IFormatProvider m_formatProvider;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private StreamReader m_stream;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool m_shouldSkipIfLineFeed;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Creates a new instance of the <see cref="CSVReader"/> class, reads from the specified <paramref name="path"/>
        /// sets the format provider for parsing and goes to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="path">The path to search for the CSV file.</param>
        /// <param name="hasHead">True if the first row contains the column heads.</param>
        /// <param name="position">The position to read.</param>
        /// <param name="provider">The format provider.</param>
        public CSVReader(string path, bool hasHead, int position, IFormatProvider provider)
        {
            if (provider is null) throw new ArgumentException(nameof(provider));
            
            m_formatProvider = provider;
            m_stream = File.OpenText(path);
            m_path = path;
            m_hasHead = hasHead;
            FieldPreProccessor = (str) => str;

            if (m_hasHead)
                m_head = Read();

            SetPosition(position);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVReader"/> class, reads from the specified <paramref name="path"/>
        /// and sets the format provider for parsing.
        /// </summary>
        /// <param name="path">The path to search for the CSV file.</param>
        /// <param name="provider">The format provider.</param>
        public CSVReader(string path, bool hasHead, IFormatProvider provider) : this(path, hasHead, 0, provider) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVReader"/> class, reads from the specified <paramref name="path"/>
        /// and goes to the specified <paramref name="position"/>.
        /// </summary>
        /// <param name="path">The path to search for the CSV file.</param>
        /// <param name="position">The position to read.</param>
        public CSVReader(string path, bool hasHead, int position) : this(path, hasHead, position, CultureInfo.InvariantCulture) { }

        /// <summary>
        /// Creates a new instance of the <see cref="CSVReader"/> class and reads from the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to search for the CSV file.</param>
        public CSVReader(string path, bool hasHead) : this(path, hasHead, 0, CultureInfo.InvariantCulture) { }

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Sets the actual position where the <see cref="CSVReader"/> is reading.
        /// </summary>
        public bool SetPosition(int position)
        {
            if (m_disposed) throw new ObjectDisposedException(nameof(CSVReader));
            if (position < 0) throw new ArgumentException(nameof(position));

            if (position == m_position) return true;

            if(position < m_position)
            {
                m_shouldSkipIfLineFeed = false;
                m_position = 0;
                m_stream.Close();
                m_stream = File.OpenText(m_path);
            }

            position = m_hasHead ? position + 1 : position;

            while (position > m_position)
                if (Read() == null)
                    return false;
            
            return true;
        }

        /// <summary>
        /// Skips the next record, returns true if there was a record to skip.
        /// </summary>
        public bool Skip()
            => Read() != null;

        /// <summary>
        /// Skips the next <paramref name="n"/> records, return true if there were <paramref name="n"/> records to skip.
        /// </summary>
        public bool Skip(int n)
        {
            while (n-- > 0)
                if (Read() == null)
                    return false;
            return true;
         }

        /// <summary>
        /// Reads the next record.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the object is disposed.</exception>
        /// <exception cref="CSVFieldFormatException">Thrown when the record has bad format.</exception>
        public CSVRecord Read()
        {
            // If the object is disposed, return inmediately.
            if (m_disposed) 
                throw new ObjectDisposedException(nameof(CSVReader));

            int current,                        // The current char.
                state = ST_NONE;                // The current reading state.

            IList<string> record_fields = new List<string>();
            StringBuilder current_field = new StringBuilder();

            while (true)
            {
                current = m_stream.Read();

                if (m_shouldSkipIfLineFeed)
                {
                    m_shouldSkipIfLineFeed = false;
                    if (current == LF)
                        current = m_stream.Read();
                }

                switch (current)
                {
                    case EOF:
                        // If end of file is reached but we are still on a quoted field.
                        if (state == ST_INSIDE_QUOTES) throw new CSVFieldFormatException("Found EOF before exit the quotes.");
                        if (record_fields.Count == 0 && current_field.ToString().Length == 0) return null;

                        m_position++;
                        record_fields.Add(FieldPreProccessor(current_field.ToString()));
                        return new CSVRecord(record_fields.ToArray());
                    case CR:
                    case LF:
                        // If CR or LF char is found but we are not on a quoted field.
                        if (state != ST_INSIDE_QUOTES)
                        {
                            if (current == CR) m_shouldSkipIfLineFeed = true;
                            m_position++;
                            record_fields.Add(FieldPreProccessor(current_field.ToString()));
                            return new CSVRecord(record_fields.ToArray());
                        }
                        current_field.Append((char)current);
                        break;
                    case COMMA:
                        // If comma char is found but we are not on a quoted field.
                        if (state != ST_INSIDE_QUOTES)
                        {
                            record_fields.Add(FieldPreProccessor(current_field.ToString()));
                            current_field.Clear();
                            state = ST_NONE;
                        }
                        else current_field.Append((char)current);
                        break;
                    case DQUOTE:
                        if (state == ST_PREVWASDQOTE)
                        {
                            current_field.Append((char)current);
                            state = ST_INSIDE_QUOTES;
                        }
                        else if (state == ST_INSIDE_QUOTES) state = ST_PREVWASDQOTE;
                        else if (current_field.Length > 0) throw new CSVFieldFormatException("Double quote wasnot expected on the field.");
                        else state = ST_INSIDE_QUOTES;

                        break;
                    default:
                        if (state == ST_PREVWASDQOTE) throw new CSVFieldFormatException("Expecting comma or CRLF after field.");
                        else current_field.Append((char)current);
                        break;
                }
            }
        }

        /// <summary>
        /// Reads the next record.
        /// </summary>
        public bool TryRead(out CSVRecord record)
        {
            try
            {
                record = Read();
                return record != null;
            }
            catch (Exception)
            {
                record = default;
                return false;
            }
        }

        /// <summary>
        /// Reads the next <see cref="CSVRecord"/> and converts it to the specified struct, the struct must have the <see cref="CSVStructLayoutAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The result struct.</typeparam>
        /// <exception cref="ObjectDisposedException">Thrown when the object is disposed.</exception>
        /// <exception cref="MissingAttributeException">Thrown when the struct doesnt have the <see cref="CSVStructLayoutAttribute"/>.</exception>
        /// <exception cref="CSVFieldFormatException">Thrown when the record has bad format.</exception>
        /// <exception cref="CSVFieldParseException">Thrown when the result struct contain a type that is not accepted for parsing.</exception>
        public T Read<T>() where T : struct
        {
            // If the object is disposed, return inmediately.
            if (m_disposed) 
                throw new ObjectDisposedException(nameof(CSVReader));

            // Getting the result type.
            var type = typeof(T);

            // Gets the CSVStructLayoutAttribute in the result type if any.
            var structAttr = type.GetCustomAttribute<CSVStructLayoutAttribute>();

            // The CSVStructLayoutAttribute is required.
            if (structAttr == null) 
                throw new MissingAttributeException(typeof(T), typeof(CSVStructLayoutAttribute));

            // Gets the next record if any.
            var record = Read();

            // If no next record, returns the default of the result type.
            if (record == null) return default;

            // Creates a new empty instance of the result type.
            object result = Activator.CreateInstance(type);

            // Gets all public instance properties of the result type.
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).
                 OrderBy(x => x.MetadataToken).
                 ToArray();

            if (structAttr.Kind == CSVLayoutKind.Sequential)
            {
                for (int i = 0; i < props.Length && i < record.Fields.Count; i++)
                {                   
                    var propSetMethod = props[i].GetSetMethod();
                    if (propSetMethod is null) continue;
                    var propType = props[i].PropertyType;
                    var propValue = ParseRecordField(record.Fields[i], propType, m_formatProvider);
                    propSetMethod.Invoke(result, new object[] { propValue });
                }
            }
            else if (structAttr.Kind == CSVLayoutKind.Explicit)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    var propSetMethod = props[i].GetSetMethod();
                    var propPosAttr = props[i].GetCustomAttribute<CSVFieldPositionAttribute>();
                    var propHeadAttr = props[i].GetCustomAttribute<CSVFieldHeadAttribute>();

                    if (propSetMethod is null || (propPosAttr is null && propHeadAttr is null)) continue;

                    var propType = props[i].PropertyType;
                    var fieldIndex = propPosAttr == null ? m_head.IndexOf(propHeadAttr.Head, propHeadAttr.IgnoreCase) : propPosAttr.Position;

                    if (fieldIndex < 0 || fieldIndex >= record.Length) continue;

                    var propValue = ParseRecordField(record.Fields[fieldIndex], propType, m_formatProvider);
                    propSetMethod.Invoke(result, new object[] { propValue });
                }
            }

            return (T)result;
        }

        public bool TryRead<T>(out T res) where T : struct
        {
            try
            {
                res = Read<T>();
                return !res.Equals(default(T));
            }
            catch (Exception)
            {
                res = default;
                return false;
            }
        }

        #endregion

        #region IDISPOSABLE_IMPL

        /// <summary>
        /// Releases all resources used by the <see cref="CSVReader"/> object.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;
                m_stream.Dispose();
            }
        }

        #endregion
    }
}