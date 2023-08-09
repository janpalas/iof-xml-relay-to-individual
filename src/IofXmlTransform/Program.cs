// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;
using IofXmlTransform;

Console.WriteLine("Starting IOF XML transformation...");

if (args.Length < 2)
{
	Console.WriteLine("Invalid parameters. Usage: IofXmlTransform <<path to input file>> <<path to output file>>");
	Console.ReadKey();
	return;
}

string inputFilePath = args[0];
string outputFilePath = args[1];

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
	XDocument relayXml = XDocument.Load(inputFilePath);
	XDocument individualXml = IofXmlManager.FromRelayResultsToIndividualResults(relayXml);

	individualXml.Save(outputFilePath);

	Console.WriteLine("Transformation completed!");
}
catch (Exception e)
{
	Console.WriteLine("An error ocurred during transformation!");
	Console.WriteLine(e);
}

Console.ReadKey();
