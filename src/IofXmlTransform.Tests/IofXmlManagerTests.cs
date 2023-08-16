using System.Xml.Linq;

namespace IofXmlTransform.Tests;

public class IofXmlManagerTests
{
		
	[Test]
	public void FromRelayToIndividualResults_Simple()
	{
		//arrange
		XDocument relayXml = LoadXml("wc2023_sprint_relay_results.xml");

		//act
		XDocument individualXml = IofXmlManager.FromRelayToIndividualResults(relayXml, 4);

		individualXml.Save(@"C:\Devel\tes-result.xml");
	}

	[Test]
	public void FromIndividualToRelayEntries_Simple()
	{
		//arrange
		XDocument individualEntries = LoadXml("wc2023_entries_middle.xml");

		//act
		XDocument relayEntries = IofXmlManager.FromIndividualToRelayEntries(individualEntries);

		relayEntries.Save(@"C:\Devel\relay-entries.xml");
	}

	private XDocument LoadXml(string xmlFileName)
	{
		string xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", xmlFileName);
		if (!File.Exists(xmlFilePath)) 
			throw new ArgumentException($"File '{xmlFilePath}' does not exists!");

		return XDocument.Load(xmlFilePath);
	}
}