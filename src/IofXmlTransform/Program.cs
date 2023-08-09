// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;
using IofXmlTransform;

Console.WriteLine("Starting IOF XML transformation...");

if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
{
	Console.WriteLine("Missing path to XML file with results!");
	Console.ReadKey();
	return;
}

string filePath = args[0];
if (!File.Exists(filePath))
{
	Console.WriteLine("File {0} does not exists!", filePath);
	Console.ReadKey();
	return;
}

try
{
	XDocument relayXml = XDocument.Load(filePath);
	XDocument individualXml = IofXmlManager.FromRelayResultsToIndividualResults(relayXml);
}
catch (Exception e)
{
	Console.WriteLine("An error ocurred during transformation!");
	Console.WriteLine(e);

	Console.ReadKey();
	return;
}

Console.WriteLine("OK");
Console.ReadKey();

