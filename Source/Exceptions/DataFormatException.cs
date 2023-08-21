using System;
using System.Diagnostics;

namespace CSVParser
{
	[DebuggerStepThrough]
	public class DataFormatException : Exception
	{
		internal DataFormatException(string reason) :
			base($"Data is malformed: {reason}")
		{ }
	}
}
