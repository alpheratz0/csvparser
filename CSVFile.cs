#region RFC4180

/*
 
       RFC4180 - Common Format and MIME Type 
       for Comma-Separated Values (CSV) Files

       This section documents the format that seems to be followed by 
       most implementations.

       ----------------------------------------------------------------------------     
       
       1.  Each record is located on a separate line, delimited by a line
           break (CRLF).  For example:

           aaa,bbb,ccc CRLF
           zzz,yyy,xxx CRLF

       2.  The last record in the file may or may not have an ending line
           break.  For example:

           aaa,bbb,ccc CRLF
           zzz,yyy,xxx

       3.  There maybe an optional header line appearing as the first line
           of the file with the same format as normal record lines.  This
           header will contain names corresponding to the fields in the file
           and should contain the same number of fields as the records in
           the rest of the file (the presence or absence of the header line
           should be indicated via the optional "header" parameter of this
           MIME type).  For example:

           field_name,field_name,field_name CRLF
           aaa,bbb,ccc CRLF
           zzz,yyy,xxx CRLF

       4.  Within the header and each record, there may be one or more
           fields, separated by commas.  Each line should contain the same
           number of fields throughout the file.  Spaces are considered part
           of a field and should not be ignored.  The last field in the
           record must not be followed by a comma.  For example:

           aaa,bbb,ccc

       5.  Each field may or may not be enclosed in double quotes (however
           some programs, such as Microsoft Excel, do not use double quotes
           at all).  If fields are not enclosed with double quotes, then
           double quotes may not appear inside the fields.  For example:

           "aaa","bbb","ccc" CRLF
           zzz,yyy,xxx

       6.  Fields containing line breaks (CRLF), double quotes, and commas
           should be enclosed in double-quotes.  For example:

           "aaa","b CRLF
           bb","ccc" CRLF
           zzz,yyy,xxx

       7.  If double-quotes are used to enclose fields, then a double-quote
           appearing inside a field must be escaped by preceding it with
           another double quote.  For example:

           "aaa","b""bb","ccc"

 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSVParser
{
    /// <summary>
    /// This class provides basic CSV read/write functionality.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class CSVFile : IList<CSVRecord>
    {
        #region PRIVATE_FIELDS

        // All the records.
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly IList<CSVRecord> m_records;

        #endregion

        #region PUBLIC_PROPERTIES

        /// <summary>
        /// Gets the ammount of records.
        /// </summary>
        public int Count => m_records == null ? 0 : m_records.Count;

        #endregion

        #region INDEXERS

        public CSVRecord this[int index]
        {
            get
            {
                if (index < 0) index = Count + index; 
                if (index < 0 || index >= Count) return null;
                return m_records[index];
            }
            set
            {
                m_records[index] = value;
            }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Creates a new instance of the <see cref="CSVFile"/> class with the specified <paramref name="records"/>.
        /// </summary>
        /// <param name="records">The records.</param>
        public CSVFile(IEnumerable<CSVRecord> records)
            => m_records = new List<CSVRecord>(records);

        #endregion

        #region STATIC_METHODS

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static CSVFile Load(string path)
            => Load(path, 0, int.MaxValue);

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static CSVFile Load(string path, int start)
            => Load(path, start, int.MaxValue);

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static CSVFile Load(string path, int start, int end)
        {
            IList<CSVRecord> records = new List<CSVRecord>();
            
            if(end >= start)
            {
                using (CSVReader reader = new CSVReader(path, false, start))
                {
                    CSVRecord current = reader.Read();

                    if (current != null)
                    {
                        int fields_per_record = current.Length;
                        records.Add(current);

                        while (reader.Position < end && (current = reader.Read()) != null)
                        {
                            if (current.Length != fields_per_record)
                                throw new FormatException("All records must have the same ammount of fields.");
                            records.Add(current);
                        }
                    }
                }
            }

            return new CSVFile(records);
        }

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static bool TryLoad(string path, out CSVFile csv)
            => TryLoad(path, 0, int.MaxValue, out csv);

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static bool TryLoad(string path, int start, out CSVFile csv)
            => TryLoad(path, start, int.MaxValue, out csv);

        /// <summary>
        /// Loads the CSV file.
        /// </summary>
        public static bool TryLoad(string path, int start, int end, out CSVFile csv)
        {
            try
            {
                csv = Load(path, start, end);
                return true;
            }
            catch
            {
                csv = null;
                return false;
            }
        }

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Saves the <see cref="CSVFile"/> to the specified <paramref name="path"/>.
        /// </summary>
        public void Save(string path)
        {
            string[] escapedRecords = m_records.Select(record => record.Escape()).ToArray();
            File.WriteAllText(path, string.Join("\r\n", escapedRecords));
        }

        #endregion

        #region ILIST_IMPL

        bool ICollection<CSVRecord>.IsReadOnly => false;

        public int IndexOf(CSVRecord item)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Equals(item))
                    return i;
            return -1;
        }

        public bool Remove(CSVRecord item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void Insert(int index, CSVRecord item)
            => m_records.Insert(index, item);

        public void RemoveAt(int index)
            => m_records.RemoveAt(index);

        public void Add(CSVRecord item)
            => m_records.Add(item);

        public void Clear()
            => m_records.Clear();

        public bool Contains(CSVRecord item)
            => IndexOf(item) != -1;

        public void CopyTo(CSVRecord[] array, int arrayIndex)
            => m_records.CopyTo(array, arrayIndex);

        IEnumerator<CSVRecord> IEnumerable<CSVRecord>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<CSVRecord>)this).GetEnumerator();

        #endregion
    }
}