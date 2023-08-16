using System.Xml.Linq;

namespace IofXmlTransform.Tests;

public class IofXmlManagerTests
{
	private static readonly XNamespace Namespace = "http://www.orienteering.org/datastandard/3.0";

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

		List<XElement> personEntries = individualEntries.Descendants(Namespace + "PersonEntry").ToList();
		List<XElement> teamEntries = relayEntries.Descendants(Namespace + "TeamEntry").ToList();
		Assert.AreEqual(personEntries.Count, teamEntries.Count);

		foreach (XElement personEntry in personEntries)
		{
			string firstName = personEntry.Descendants(Namespace + "Given").First().Value;
			string lastName = personEntry.Descendants(Namespace + "Family").First().Value;

			XElement? teamEntry = teamEntries.FirstOrDefault(x => x.Descendants(Namespace + "Name").First().Value == $"{lastName} {firstName}");
			Assert.IsNotNull(teamEntry);

			Assert.AreEqual(personEntry.Descendants(Namespace + "Id").First().Value, teamEntry.Descendants(Namespace + "Id").First().Value);
			Assert.AreEqual(personEntry.Descendants(Namespace + "Organisation").First().ToString(), teamEntry.Descendants(Namespace + "Organisation").First().ToString());

			List<XElement> teamEntryPersons = teamEntry.Descendants(Namespace + "TeamEntryPerson").ToList();
			Assert.AreEqual(1, teamEntryPersons.Count);

			Assert.AreEqual(personEntry.Descendants(Namespace + "Person").First().ToString(), teamEntryPersons[0].Descendants(Namespace + "Person").First().ToString());
			Assert.AreEqual(personEntry.Descendants(Namespace + "Organisation").First().ToString(), teamEntryPersons[0].Descendants(Namespace + "Organisation").First().ToString());
			Assert.AreEqual("1", teamEntryPersons[0].Descendants(Namespace + "Leg").First().Value);
			Assert.AreEqual("1", teamEntryPersons[0].Descendants(Namespace + "LegOrder").First().Value);

			XElement? controlCard = personEntry.Descendants(Namespace + "ControlCard").FirstOrDefault();
			XElement? relayControlCard = teamEntryPersons[0].Descendants(Namespace + "ControlCard").FirstOrDefault();
			
			if (controlCard != null)
			{
				Assert.IsNotNull(relayControlCard);
				Assert.AreEqual(controlCard.ToString(), relayControlCard.ToString());
			}
			else
			{
				Assert.IsNull(relayControlCard);
			}

			Assert.AreEqual(personEntry.Descendants(Namespace + "Class").First().ToString(), teamEntry.Descendants(Namespace + "Class").First().ToString());
		}
	}

	private XDocument LoadXml(string xmlFileName)
	{
		string xmlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", xmlFileName);
		if (!File.Exists(xmlFilePath)) 
			throw new ArgumentException($"File '{xmlFilePath}' does not exists!");

		return XDocument.Load(xmlFilePath);
	}
}