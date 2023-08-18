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

		//assert
		XElement relayEvent = individualXml.Descendants(Namespace + "Event").First();
		XElement individualEvent = individualXml.Descendants(Namespace + "Event").First();
		Assert.AreEqual(relayEvent.ToString(), individualEvent.ToString());

		List<XElement> relayClassResults = relayXml.Descendants(Namespace + "ClassResult").ToList();
		List<XElement> individualClassResults = individualXml.Descendants(Namespace + "ClassResult").ToList();
		Assert.AreEqual(relayClassResults.Count, individualClassResults.Count);

		foreach (XElement relayClassResult in relayClassResults)
		{
			XElement relayClassInfo = relayClassResult.Descendants(Namespace + "Class").First();

			string classId = relayClassInfo.Descendants(Namespace + "Id").First().Value;
			string className = relayClassInfo.Descendants(Namespace + "Name").First().Value;

			XElement? individualClassResult = individualClassResults.FirstOrDefault(x => x.Descendants(Namespace + "Class").Descendants(Namespace + "Name").First().Value == className);
			Assert.IsNotNull(individualClassResult);

			XElement individualClassInfo = individualClassResult.Descendants(Namespace + "Class").First();
			Assert.AreEqual(classId, individualClassInfo.Descendants(Namespace + "Id").First().Value);

			List<XElement> teamMemberResults = relayClassResults.Descendants(Namespace + "TeamMemberResult").ToList();
			List<XElement> personResults = individualClassResults.Descendants(Namespace + "PersonResult").ToList();
			Assert.AreEqual(teamMemberResults.Count, personResults.Count);

			foreach (XElement teamMemberResult in teamMemberResults)
			{
				XElement? relayOrganization = teamMemberResult.Parent?.Descendants(Namespace + "Organisation").First();
				Assert.IsNotNull(relayOrganization);

				XElement teamPerson = teamMemberResult.Descendants(Namespace + "Person").First();
				string teamPersonId = teamPerson.Descendants(Namespace + "Id").First(x => x.Attribute("type")?.Value == "QuickEvent").Value;

				XElement? individualPersonResult = personResults.FirstOrDefault(x => x.Descendants(Namespace + "Person").Descendants(Namespace + "Id").First(y => y.Attribute("type")?.Value == "QuickEvent").Value == teamPersonId);
				Assert.IsNotNull(individualPersonResult);

				XElement individualPerson = individualPersonResult.Descendants(Namespace + "Person").First();
				Assert.AreEqual(teamPerson.ToString(), individualPerson.ToString());

				XElement? individualOrganisation = individualPersonResult.Descendants(Namespace + "Organisation").FirstOrDefault();
				Assert.IsNotNull(individualOrganisation);
				Assert.AreEqual(2, individualOrganisation.Descendants().Count());
				Assert.AreEqual(relayOrganization.Descendants(Namespace + "Id").First().ToString(), individualOrganisation.Descendants(Namespace + "Id").First().ToString());
				Assert.AreEqual(relayOrganization.Descendants(Namespace + "Name").First().ToString(), individualOrganisation.Descendants(Namespace + "Name").First().ToString());

				XElement? individualResult = individualPersonResult.Descendants(Namespace + "Result").FirstOrDefault();
				XElement relayResult = teamMemberResult.Descendants(Namespace + "Result").First();
				
				Assert.IsNotNull(individualResult);
				Assert.AreEqual(relayResult.Descendants(Namespace + "StartTime").First().Value, individualResult.Descendants(Namespace + "StartTime").First().Value);
				Assert.AreEqual(relayResult.Descendants(Namespace + "FinishTime").First().Value, individualResult.Descendants(Namespace + "FinishTime").First().Value);
				Assert.AreEqual(relayResult.Descendants(Namespace + "Time").First().Value, individualResult.Descendants(Namespace + "Time").First().Value);
				Assert.AreEqual(relayResult.Descendants(Namespace + "Status").First().Value, individualResult.Descendants(Namespace + "Status").First().Value);

				XElement timeBehind = individualResult.Descendants(Namespace + "TimeBehind").First();
				Assert.AreEqual(relayResult.Descendants(Namespace + "TimeBehind").First().Value, timeBehind.Value);
				Assert.IsNull(timeBehind.Attribute("type"));

				XElement position = individualResult.Descendants(Namespace + "Position").First();
				Assert.AreEqual(relayResult.Descendants(Namespace + "Position").First().Value, position.Value);
				Assert.IsNull(position.Attribute("type"));

				List<XElement> relaySplitTimes = relayResult.Descendants(Namespace + "SplitTime").ToList();
				List<XElement> individualSplitTimes = individualResult.Descendants(Namespace + "SplitTime").ToList();
				Assert.AreEqual(relaySplitTimes.Count, individualSplitTimes.Count);
				Assert.AreEqual(string.Join(';', relaySplitTimes.Select(x => x.ToString())), string.Join(';', individualSplitTimes.Select(x => x.ToString())));
			}
		}
	}

	[Test]
	public void FromIndividualToRelayEntries_Simple()
	{
		//arrange
		XDocument individualEntries = LoadXml("wc2023_entries_middle.xml");

		//act
		XDocument relayEntries = IofXmlManager.FromIndividualToRelayEntries(individualEntries);

		relayEntries.Save(@"C:\Devel\relay-entries.xml");

		//assert
		Assert.AreEqual("Relay", relayEntries.Descendants(Namespace + "Event").First().Descendants(Namespace + "Form").First().Value);

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