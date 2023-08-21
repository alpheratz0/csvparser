using System;
using CSVParser;

[CSVStructLayout(CSVLayoutKind.Explicit)]
struct Person
{
	[CSVBindColumnName("id")]
	public int ID { get; set; }

	[CSVBindColumnName("name")]
	public string Name { get; set; }

	[CSVBindColumnName("lastname")]
	public string Lastname { get; set; }

	public override string ToString()
	{
		return $"{Lastname}, {Name}";
	}
}

public class Program
{
	static void Main(string[] args)
	{
		using (CSVReader reader = new CSVReader("./Data.csv", true))
		{
			while (reader.TryRead<Person>(out Person person))
				Console.WriteLine(person);
		}
	}
}
