using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSVParser
{
	internal sealed class CSVFieldComparer : IEqualityComparer<CSVField>
	{
		public bool Equals(CSVField f1, CSVField f2)
		{
			if (ReferenceEquals(f1, f2))
				return true;

			if (f2 is null || f1 is null)
				return false;

			return f1.Value == f2.Value;
		}

		public int GetHashCode(CSVField field)
		{
			if (field is null)
				return 0;
			return field.Value.GetHashCode();
		}
	}

	[DebuggerStepThrough]
	public sealed class CSVField
	{
		public string Value { get; private set; }

		public CSVField(string value)
		{
			if (value is null)
				throw new ArgumentException(nameof(value));
			this.Value = value;
		}

		public object ConvertTo(Type type, IFormatProvider provider)
		{
			try
			{
				if (type.IsEnum) return Enum.Parse(type, this.Value);
				if (type == typeof(string)) return this.Value;
				if (type == typeof(bool)) return bool.Parse(this.Value);
				if (type == typeof(char)) return char.Parse(this.Value);
				if (type == typeof(sbyte)) return sbyte.Parse(this.Value, provider);
				if (type == typeof(byte)) return byte.Parse(this.Value, provider);
				if (type == typeof(decimal)) return decimal.Parse(this.Value, provider);
				if (type == typeof(double)) return double.Parse(this.Value, provider);
				if (type == typeof(float)) return float.Parse(this.Value, provider);
				if (type == typeof(int)) return int.Parse(this.Value, provider);
				if (type == typeof(uint)) return uint.Parse(this.Value, provider);
				if (type == typeof(long)) return long.Parse(this.Value, provider);
				if (type == typeof(ulong)) return ulong.Parse(this.Value, provider);
				if (type == typeof(short)) return short.Parse(this.Value, provider);
				if (type == typeof(ushort)) return ushort.Parse(this.Value, provider);
				if (type == typeof(DateTime)) return DateTime.Parse(this.Value, provider);
			}
			catch (FormatException) { throw new ConvertException(this.Value, type); }
			throw new ConvertException(this.Value, type);
		}

		public override string ToString()
		{
			string res = this.Value;

			if (res.Contains("\""))
				res = res.Replace("\"", "\"\"");

			if (res.Contains("\"") || res.Contains("\r")
					|| res.Contains("\n") || res.Contains(","))
				res = $"\"{res}\"";

			return res;
		}
	}
}
