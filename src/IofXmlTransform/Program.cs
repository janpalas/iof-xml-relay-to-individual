// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;
using IofXmlTransform;

Console.WriteLine("Starting IOF XML transformation...");

if (args.Length < 3)
{
	Console.WriteLine("Invalid parameters. Usage: IofXmlTransform <<type>> <<path to input file>> <<path to output file>>");
	Console.ReadKey();
	return;
}

string type = args[0].ToLower();
string inputFilePath = args[1];
string outputFilePath = args[2];

if (type != "-e" && type != "-r")
{
	Console.WriteLine($"Invalid transformation type '{type}'! Use '-e' for entries, '-r' for results.");
	Console.ReadKey();
	return;
}

if (string.IsNullOrEmpty(inputFilePath))
{
	Console.WriteLine("Missing path to input XML file with relay results!");
	Console.ReadKey();
	return;
}

if (string.IsNullOrEmpty(outputFilePath))
{
	Console.WriteLine("Missing path to out XML file with individual results!");
	Console.ReadKey();
	return;
}

if (!File.Exists(inputFilePath))
{
	Console.WriteLine("File {0} does not exists!", inputFilePath);
	Console.ReadKey();
	return;
}

try
{
	XDocument inputXml = XDocument.Load(inputFilePath);

	XDocument outputXml = type == "-e" ? IofXmlManager.FromIndividualToRelayEntries(inputXml) : IofXmlManager.FromRelayToIndividualResults(inputXml);
	outputXml.Save(outputFilePath);

	Console.WriteLine("Transformation completed!");
}
catch (Exception e)
{
	Console.WriteLine("An error ocurred during transformation!");
	Console.WriteLine(e);
}

Console.ReadKey();
