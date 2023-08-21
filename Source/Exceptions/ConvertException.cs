using System;
using System.Diagnostics;

namespace CSVParser
{
	[DebuggerStepThrough]
	public class ConvertException : Exception
	{
		internal ConvertException(string value, Type type) :
			base($"Cannot convert \"{value}\" to \"{type}\".")
		{ }
	}
}
