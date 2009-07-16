using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chorus.VcsDrivers.Mercurial
{
	public class Revision
	{
		private readonly HgRepository _repository;
		public string UserId { get; set; }
		public RevisionNumber Number;
		public string Summary { get; set; }
		public string Tag{ get; set;}
		public string DateString { get; set; }
		public List<RevisionNumber> Parents { get; private set; }

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
			Parents = new List<RevisionNumber>();
		}

		public Revision(HgRepository repository, string name, string localRevisionNumber, string hash, string comment)
			:this(repository)
		{
			UserId = name;
			Number = new RevisionNumber(localRevisionNumber,hash);
			Summary = comment;
			Tag = "";
		}

		public void SetRevisionAndHashFromCombinedDescriptor(string descriptor)
		{
			Number = new RevisionNumber(descriptor);
		}
		public bool IsMatchingStub(Revision stub)
		{
			return stub.Summary.Contains(string.Format("({0} partial from", UserId));
		}

		public IEnumerable<RevisionNumber> GetLocalNumbersOfParents()
		{
			return Parents;
//            return _repository.GetParentsOfRevision(this.Number.LocalRevisionNumber);
		}

		public void AddParentFromCombinedNumberAndHash(string descriptor)
		{
		  Parents.Add(new RevisionNumber(descriptor));

		}

		public bool IsDirectDescendantOf(Revision revision)
		{
			//TODO: this is only checking direct descendant
			return Parents.Any(p => p.Hash == revision.Number.Hash);
		}
	}

	public class RevisionNumber
	{
		public RevisionNumber(string local, string hash)
		{
			LocalRevisionNumber = local;
			Hash = hash;
		}
		public RevisionNumber(string combinedNumberAndHash)
		{
			string[] parts = combinedNumberAndHash.Split(new char[] { ':' });
			Debug.Assert(parts.Length == 2);
			Hash = parts[1];
			LocalRevisionNumber = parts[0];

		}

		public string Hash { get; set; }
		public string LocalRevisionNumber { get; set; }
	}
}