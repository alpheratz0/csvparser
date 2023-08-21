using System;
using System.Diagnostics;

namespace CSVParser
{
	[DebuggerStepThrough]
	public class MissingAttributeException : Exception
	{
		internal MissingAttributeException(Type target, Type attributeMissed) :
			base($"\"{target}\" is missing \"{attributeMissed}\" attribute.")
		{ }
	}
}
