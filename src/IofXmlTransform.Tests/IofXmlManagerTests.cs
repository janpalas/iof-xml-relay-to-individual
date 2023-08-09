using System.Xml.Linq;

namespace IofXmlTransform.Tests;

public class IofXmlManagerTests
{
		
	[Test]
	public void FromRelayResultsToIndividualResults_Simple()
	{
		//arrange
		XDocument relayXml = LoadXml("relays-results-CPS-1.xml");

		//act
		XDocument individualXml = IofXmlManager.FromRelayResultsToIndividualResults(relayXml, 3);
	}

	private XDocument LoadXml(string xmlFileName)
	{
		string xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", xmlFileName);
		if (!File.Exists(xmlFilePath)) 
			throw new ArgumentException($"File '{xmlFilePath}' does not exists!");

		return XDocument.Load(xmlFilePath);
	}
}