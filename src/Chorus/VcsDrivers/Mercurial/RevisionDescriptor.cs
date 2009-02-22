using System.Collections.Generic;
using System.Diagnostics;

namespace Chorus.VcsDrivers.Mercurial
{
	public class RevisionDescriptor
	{
		public string UserId { get; set; }
		public string Revision { get; set; }
		public string Hash{ get; set;}
		public string Summary { get; set; }
		public string Tag{ get; set;}
		public string DateString { get; set; }

		public RevisionDescriptor()
		 {
		 }

		public void SetRevisionAndHashFromCombinedDescriptor(string descriptor)
		{
			string[] parts = descriptor.Split(new char[] { ':' });
			Debug.Assert(parts.Length == 2);
			Hash = parts[1];
			Revision = parts[0];
		}

		public RevisionDescriptor(string name, string revision, string hash, string comment)
		{
			UserId = name;
			Revision = revision;
			Hash = hash;
			Summary = comment;
			Tag = "";
		}

		public static List<RevisionDescriptor>  GetRevisionsFromQueryOutput(string result)
		{
			//Debug.WriteLine(result);
			string[] lines = result.Split('\n');
			List<Dictionary<string, string>> rawChangeSets = new List<Dictionary<string, string>>();
			Dictionary<string, string> rawChangeSet = null;
			foreach (string line in lines)
			{
				if (line.StartsWith("changeset:"))
				{
					rawChangeSet = new Dictionary<string, string>();
					rawChangeSets.Add(rawChangeSet);
				}
				string[] parts = line.Split(new char[] { ':' });
				if (parts.Length < 2)
					continue;
				//join all but the first back together
				string contents = string.Join(":", parts, 1, parts.Length-1);
				rawChangeSet[parts[0].Trim()] = contents.Trim();
			}

			List<RevisionDescriptor> revisions = new List<RevisionDescriptor>();
			foreach (Dictionary<string, string> d in rawChangeSets)
			{
				string[] revisionParts = d["changeset"].Split(':');
				string summary = string.Empty;
				if (d.ContainsKey("summary"))
				{
					summary = d["summary"];
				}
				RevisionDescriptor revision = new RevisionDescriptor(d["user"], revisionParts[0], /*revisionParts[1]*/"unknown", summary);
				if(d.ContainsKey("tag"))
				{
					revision.Tag = d["tag"];
				}
				revisions.Add(revision);

			}
			return revisions;
		}

		public bool IsMatchingStub(RevisionDescriptor stub)
		{
			return stub.Summary.Contains(string.Format("({0} partial from", UserId));
		}
	}
}