using System.Xml.Linq;

namespace IofXmlTransform;

public class IofXmlManager
{
	private static readonly XNamespace Namespace = "http://www.orienteering.org/datastandard/3.0";

	public static XDocument FromRelayResultsToIndividualResults(XDocument relayXml, int maxAllowedLegsPerTeam = 1)
	{
		XElement resultList = relayXml.Descendants(Namespace + "ResultList").First();
		XElement newResultList = new(Namespace + "ResultList", resultList.Attributes());
		
		newResultList.Add(relayXml.Descendants(Namespace + "Event").First());

		foreach (XElement classResult in relayXml.Descendants(Namespace + "ClassResult"))
		{
			XElement newClassResult = new(Namespace + "ClassResult", classResult.Descendants(Namespace + "Class").First());

			foreach (XElement teamResult in classResult.Descendants(Namespace + "TeamResult"))
			{
				List<XElement> teamMemberResults = teamResult.Descendants(Namespace + "TeamMemberResult").ToList();
				if (teamMemberResults.Count > maxAllowedLegsPerTeam)
					throw new InvalidOperationException($"Maximum {maxAllowedLegsPerTeam} allowed, but there are {teamMemberResults.Count} legs per team!");

				XElement entryId = teamResult.Descendants(Namespace + "EntryId").First();

				XElement organisation = teamResult.Descendants(Namespace + "Organisation").First();
				XElement country = organisation.Descendants(Namespace + "Country").First();

				XElement nationality = new(Namespace + "Nationality", new XAttribute("code", country.Attribute("code")!.Value));
				nationality.Add(country.Value);
				country.Remove();

				foreach (XElement teamMemberResult in teamMemberResults)
				{
					XElement person = teamMemberResult.Descendants(Namespace + "Person").First();
					person.Add(nationality);

					string firstName = person.Descendants(Namespace + "Given").First().Value;
					string lastName = person.Descendants(Namespace + "Family").First().Value;

					if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
						continue;

					XElement personResult = new(Namespace + "PersonResult", entryId);
					personResult.Add(person);
					personResult.Add(organisation);

					XElement relayLegResult = teamMemberResult.Descendants(Namespace + "Result").First();
					XElement individualResult = new(Namespace + "Result");

					string? bibNumber = relayLegResult.Descendants(Namespace + "BibNumber").FirstOrDefault()?.Value;
					if (!string.IsNullOrEmpty(bibNumber))
					{
						if (bibNumber.IndexOf('.') > 0)
						{
							bibNumber = bibNumber.Substring(0, bibNumber.IndexOf('.'));
						}

						individualResult.Add(new XElement(Namespace + "BibNumber", bibNumber));
					}
					
					individualResult.Add(relayLegResult.Descendants(Namespace + "StartTime").First());
					individualResult.Add(relayLegResult.Descendants(Namespace + "FinishTime").First());
					individualResult.Add(relayLegResult.Descendants(Namespace + "Time").First());
					individualResult.Add(new XElement(Namespace + "TimeBehind", relayLegResult.Descendants(Namespace + "TimeBehind").First().Value));
					individualResult.Add(new XElement(Namespace + "Position", relayLegResult.Descendants(Namespace + "Position").First().Value));
					individualResult.Add(relayLegResult.Descendants(Namespace + "Status").First());
					individualResult.Add(relayLegResult.Descendants(Namespace + "SplitTime"));

					personResult.Add(individualResult);
					newClassResult.Add(personResult);
				}
			}

			newResultList.Add(newClassResult);
		}

		XDocument newXml = new XDocument(newResultList);
		return newXml;
	}
}