using System;
using System.Collections.Generic;

namespace ChorusHub
{
	/// <summary>
	/// This class serializes repository name and id into a JSON string or
	/// deserializes such a string into a ChorusHubRepositoryInformation object.
	/// </summary>
	/// <example>{"name": "fooProject", "id": "123abc"}</example>
	internal static class ImitationHubJSONService
	{
		private const string format1 = "{\"name\": \"";
		private const string format2 = "\", \"id\": \"";
		private const string format3 = "\"}";

		// Serialize
		internal static string MakeJsonString(string name, string id)
		{
			return format1 + name + format2 + id + format3;
		}

		// Deserialize one
		internal static ChorusHubRepositoryInformation DechipherJsonString(string jsonString)
		{
			// Probably not the best way to do this, but...
			// If the parse fails for any reason, will throw ArgumentException.
			if (string.IsNullOrEmpty(jsonString) || !jsonString.StartsWith(format1) ||
				!jsonString.EndsWith(format3))
			{
				throw new ArgumentException("JSON object begins or ends with wrong format.");
			}
			var strippedString = jsonString.Substring(10, jsonString.Length - 12); // strip off 'format1' and 'format3'
			var endOfNameIndex = strippedString.IndexOf(format2, StringComparison.CurrentCulture);
			var name = strippedString.Substring(0, endOfNameIndex);
			var begOfIdIndex = endOfNameIndex + 10;
			var id = strippedString.Substring(begOfIdIndex, strippedString.Length - begOfIdIndex);
			return new ChorusHubRepositoryInformation(name, id);
		}

		// Deserialize a bunch
		internal static IEnumerable<ChorusHubRepositoryInformation> ParseJsonStringsToChorusHubRepoInfos(IEnumerable<string> jsonStrings)
		{
			var result = new List<ChorusHubRepositoryInformation>();
			foreach (var jsonString in jsonStrings)
			{
				try
				{
					result.Add(DechipherJsonString(jsonString));
				}
				catch (ArgumentException e)
				{
					continue;
				}
			}
			return result;
		}
	}
}
