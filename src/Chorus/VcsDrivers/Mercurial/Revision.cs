using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chorus.VcsDrivers.Mercurial
{
	public class Revision
	{
		private readonly HgRepository _repository;
		public string UserId { get; set; }
		public string LocalRevisionNumber { get; set; }
		public string Hash{ get; set;}
		public string Summary { get; set; }
		public string Tag{ get; set;}
		public string DateString { get; set; }

		/// <summary>
		/// The very first revision has no parent, most have 1, merges have 2
		/// </summary>
		public bool HasAtLeastOneParent
		{
			get { return GetLocalNumbersOfParents().Count() > 0; }
		}

		public Revision(HgRepository repository)
		{
			_repository = repository;
		}

		public Revision(HgRepository repository, string name, string localRevisionNumber, string hash, string comment)
		{
			_repository = repository;
			UserId = name;
			LocalRevisionNumber = localRevisionNumber;
			Hash = hash;
			Summary = comment;
			Tag = "";
		}

		public void SetRevisionAndHashFromCombinedDescriptor(string descriptor)
		{
			string[] parts = descriptor.Split(new char[] { ':' });
			Debug.Assert(parts.Length == 2);
			Hash = parts[1];
			LocalRevisionNumber = parts[0];
		}
		public bool IsMatchingStub(Revision stub)
		{
			return stub.Summary.Contains(string.Format("({0} partial from", UserId));
		}

		public IEnumerable<string> GetLocalNumbersOfParents()
		{
			return _repository.GetParentsOfRevision(this.LocalRevisionNumber);
		}
	}
}