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

	[DebuggerStepThrough]
	public sealed class CSVReader : IDisposable
	{
		private enum CSVReaderStateInfo
		{
			None,
			InsideQuotes,
			WaitingForComma,
			PreviousWasDoubleQuote
		}

		public int Position { get => m_position; set => SetPosition(value); }
		public FieldPreProccessor FieldPreProccessor { get; set; }

		private bool m_disposed;
		private int m_position;
		private readonly string m_path;
		private readonly bool m_hasHead;
		private readonly CSVRecord m_head;
		private readonly IFormatProvider m_formatProvider;
		private StreamReader m_stream;
		private bool m_shouldSkipIfLineFeed;

		public CSVReader(string path, bool hasHead, int position, IFormatProvider provider)
		{
			if (provider is null)
				throw new ArgumentException(nameof(provider));

			m_formatProvider = provider;
			m_stream = File.OpenText(path);
			m_path = path;
			m_hasHead = hasHead;
			FieldPreProccessor = (str) => str;

			if (m_hasHead)
				m_head = Read();

			SetPosition(position);
		}

		public CSVReader(string path, bool hasHead, IFormatProvider provider) :
			this(path, hasHead, 0, provider)
		{ }

		public CSVReader(string path, bool hasHead, int position) :
			this(path, hasHead, position, CultureInfo.InvariantCulture)
		{ }

		public CSVReader(string path, bool hasHead) :
			this(path, hasHead, 0, CultureInfo.InvariantCulture)
		{ }

		public bool SetPosition(int position)
		{
			if (m_disposed)
				throw new ObjectDisposedException(nameof(CSVReader));

			if (position < 0)
				throw new ArgumentException(nameof(position));

			if (position == m_position)
				return true;

			if (position < m_position)
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

		public bool Skip()
			=> Read() != null;

		public bool Skip(int n)
		{
			while (n-- > 0)
				if (Read() == null)
					return false;
			return true;
		}

		public CSVRecord Read()
		{
			if (m_disposed)
				throw new ObjectDisposedException(nameof(CSVReader));

			int c;
			CSVReaderStateInfo state = CSVReaderStateInfo.None;
			IList<string> record_fields = new List<string>();
			StringBuilder current_field = new StringBuilder();

			while (true)
			{
				c = m_stream.Read();

				if (m_shouldSkipIfLineFeed)
				{
					m_shouldSkipIfLineFeed = false;
					if (c == '\n')
						c = m_stream.Read();
				}

				switch (c)
				{
					case -1:
						if (state == CSVReaderStateInfo.InsideQuotes)
						{
							throw new DataFormatException(
								"Unfinished quoted field.");
						}

						if (record_fields.Count == 0
								&& current_field.ToString().Length == 0)
							return null;

						m_position++;

						record_fields.Add(
							FieldPreProccessor(
								current_field.ToString()
							)
						);

						return new CSVRecord(record_fields.ToArray());
					case '\r':
					case '\n':
						if (state == CSVReaderStateInfo.InsideQuotes)
						{
							current_field.Append((char)c);
							break;
						}

						if (c == '\r')
							m_shouldSkipIfLineFeed = true;

						m_position++;

						record_fields.Add(
							FieldPreProccessor(
								current_field.ToString()
							)
						);

						return new CSVRecord(record_fields.ToArray());
					case ',':
						if (state == CSVReaderStateInfo.InsideQuotes)
						{
							current_field.Append((char)c);
							break;
						}

						record_fields.Add(
							FieldPreProccessor(
								current_field.ToString()
							)
						);

						current_field.Clear();
						state = CSVReaderStateInfo.None;

						break;
					case '"':
						if (state == CSVReaderStateInfo.PreviousWasDoubleQuote)
						{
							current_field.Append((char)c);
							state = CSVReaderStateInfo.InsideQuotes;
						}
						else if (state == CSVReaderStateInfo.InsideQuotes)
						{
							state = CSVReaderStateInfo.PreviousWasDoubleQuote;
						}
						else if (current_field.Length > 0)
						{
							throw new DataFormatException(
								"Double quote was not expected.");
						}
						else
						{
							state = CSVReaderStateInfo.InsideQuotes;
						}

						break;
					default:
						if (state == CSVReaderStateInfo.PreviousWasDoubleQuote)
						{
							throw new DataFormatException(
								"Expected comma or CRLF after field.");
						}
						current_field.Append((char)c);
						break;
				}
			}
		}

		public T Read<T>() where T : struct
		{
			if (m_disposed)
				throw new ObjectDisposedException(nameof(CSVReader));

			var type = typeof(T);

			var layoutAttr = type.
				GetCustomAttribute<CSVStructLayoutAttribute>();

			if (layoutAttr == null)
			{
				throw new MissingAttributeException(typeof(T),
					typeof(CSVStructLayoutAttribute));
			}

			var record = Read();

			if (record == null)
				throw new Exception("No record to read.");

			// Creates a new empty instance of the result type.
			object result = Activator.CreateInstance(type);

			// Get all public instance properties of the result type
			// sorted by declaration order.
			var properties = type.
					GetProperties(BindingFlags.Public|BindingFlags.Instance).
					OrderBy(x => x.MetadataToken).
					ToArray();

			for (int propertyIndex = 0; propertyIndex < properties.Length;
					++propertyIndex)
			{
				var property = properties[propertyIndex];
				var fieldIndex = -1;

				if (layoutAttr.Kind == CSVLayoutKind.Sequential)
				{
					fieldIndex = propertyIndex;
				}
				else // CSVLayoutKind.Explicit
				{
					var columnIndexAttr = property.
						GetCustomAttribute<CSVBindColumnIndexAttribute>();

					var columnNameAttr = property.
						GetCustomAttribute<CSVBindColumnNameAttribute>();

					if (columnIndexAttr is null && columnNameAttr is null)
						continue;

					if (columnNameAttr is not null && !m_hasHead)
						throw new DataFormatException(
							"CSV data doesn't have column names");

					fieldIndex = columnIndexAttr is not null ?
						columnIndexAttr.Index :
						m_head.IndexOf(columnNameAttr.Name,
								columnNameAttr.IgnoreCase);
				}

				if (fieldIndex < 0 || fieldIndex >= record.Length)
					continue;

				property.GetSetMethod()?.Invoke(
					result,
					new object[] {
						record.Fields[fieldIndex].
							ConvertTo(
								property.PropertyType,
								m_formatProvider
							)
					}
				);
			}

			return (T)result;
		}

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

		public bool TryRead<T>(out T res) where T : struct
		{
			try
			{
				res = Read<T>();
				return true;
			}
			catch (Exception)
			{
				res = default;
				return false;
			}
		}

		public void Dispose()
		{
			if (!m_disposed)
			{
				m_disposed = true;
				m_stream.Dispose();
			}
		}
	}
}
