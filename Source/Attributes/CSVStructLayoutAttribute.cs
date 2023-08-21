using System;

namespace CSVParser
{
	public enum CSVLayoutKind
	{
		Sequential,
		Explicit
	}

	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class CSVStructLayoutAttribute : Attribute
	{
		public CSVLayoutKind Kind { get; private set; }

		public CSVStructLayoutAttribute(CSVLayoutKind kind)
		{
			this.Kind = kind;
		}
	}
}
