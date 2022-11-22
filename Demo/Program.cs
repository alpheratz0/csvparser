using System;
using CSVParser;

[CSVStructLayout(CSVLayoutKind.Explicit)]
struct Person 
{
    [CSVFieldHead("id")] public int ID { get; set; }
    [CSVFieldHead("name")] public string Name { get; set; }
    [CSVFieldHead("lastname")] public string Lastname { get; set; }

    public override string ToString() => $"{ID} | {Name} | {Lastname}";
}
        
public class Program
{
	static void Main(string[] args) 
	{
		using (CSVReader reader = new CSVReader("./Data.csv", true)) 
		{
			while (reader.TryRead<Person>(out Person person))
				Console.WriteLine(person.ToString());
		}
	}
}
