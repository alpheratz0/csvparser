using System;

namespace CSVParser
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class CSVBindColumnNameAttribute : Attribute
	{
		public string Name { get; private set; }
		public bool IgnoreCase { get; set; }

		public CSVBindColumnNameAttribute(string name)
		{
			this.Name = name;
			this.IgnoreCase = true;
		}
	}
}
