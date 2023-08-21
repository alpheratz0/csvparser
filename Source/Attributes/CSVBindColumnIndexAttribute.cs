using System;

namespace CSVParser
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class CSVBindColumnIndexAttribute : Attribute
	{
		public int Index { get; private set; }

		public CSVBindColumnIndexAttribute(int index)
		{
			this.Index = index;
		}
	}
}
