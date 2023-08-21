using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSVParser
{
	[DebuggerStepThrough]
	public sealed class CSVRecord : IEquatable<CSVRecord>
	{
		public List<CSVField> Fields { get; private set; }
		public int Length { get => Fields.Count; }

		public CSVRecord(IEnumerable<string> fields)
		{
			this.Fields = fields.Select(field => {
				return new CSVField(field);
			}).ToList();
		}

		public override string ToString()
		{
			return string.Join(",", this.Fields.Select(field => {
				return field.ToString();
			}));
		}

		public int IndexOf(string value, bool ignoreCase = false)
		{
			StringComparison sc = ignoreCase ?
				StringComparison.OrdinalIgnoreCase :
				StringComparison.Ordinal;

			return this.Fields.FindIndex(field => {
				return string.Equals(field.Value, value, sc);
			});
		}

		public bool Equals(CSVRecord other)
		{
			bool canCompare = other is not null &&
								other.Fields is not null &&
								this.Fields is not null;

			if (!canCompare)
				return false;

			return this.Fields.SequenceEqual(other.Fields,
				new CSVFieldComparer());
		}
	}
}
