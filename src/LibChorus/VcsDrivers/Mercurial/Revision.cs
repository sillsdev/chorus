using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chorus.VcsDrivers.Mercurial
{
	[Serializable]
	public class Revision
	{
		[NonSerialized]
		private readonly HgRepository _repository;
		public string UserId { get; set; }
		public RevisionNumber Number;
		public string Summary { get; set; }
		public string Tag{ get; set;}
		public string DateString { get; set; }
		public string Branch { get; set; }
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
			Tag = string.Empty;
			Branch = string.Empty;
			Summary = string.Empty;
			UserId = string.Empty;
		}

		public Revision(HgRepository repository, string name, string localRevisionNumber, string hash, string comment)
			:this(repository)
		{
			UserId = name;
			Number = new RevisionNumber(repository, localRevisionNumber, hash);
			Summary = comment;
		}

		public Revision(HgRepository repository, string branchName, string userName, string localRevisionNumber, string hash, string comment)
			:this(repository, userName, localRevisionNumber, hash, comment)
		{
			Branch = branchName;
		}

		public void SetRevisionAndHashFromCombinedDescriptor(string descriptor)
		{
			Number = new RevisionNumber(_repository, descriptor);
		}
		public bool IsMatchingStub(Revision stub)
		{
			return stub.Summary.Contains(string.Format("({0} partial from", UserId));
		}

		public IEnumerable<RevisionNumber> GetLocalNumbersOfParents()
		{
			return Parents;
//            return Repository.GetParentsOfRevision(this.Number.LocalRevisionNumber);
		}

		public void AddParentFromCombinedNumberAndHash(string descriptor)
		{
			Parents.Add(new RevisionNumber(_repository, descriptor));

		}

		public bool IsDirectDescendantOf(Revision revision)
		{
			EnsureParentRevisionInfo();
			//TODO: this is only checking direct descendant
			return Parents.Any(p => p.Hash == revision.Number.Hash);
		}

	 /// <summary>
		/// I can't for the life of me get hg to indicate parentage in the "hg log" (even with templates
		/// asking for parents), if the revision is not the result of a merge.  And yet, it's expensive
		/// to ask again for every single one.  So as a hack, for now, this  can be called on a revision
		/// where we really need to know the parent.
		/// </summary>
		public void EnsureParentRevisionInfo()
		{
			if (this.Parents.Count == 0)
			{
				Parents.AddRange(_repository.GetParentsRevisionNumbers(this.Number.LocalRevisionNumber));
			}
		}

		public bool GetMatchesLocalOrHash(string localOrHash)
		{
			return Number.Hash == localOrHash || Number.LocalRevisionNumber == localOrHash;
		}
	}

	[Serializable]
	public class RevisionNumber
	{
		internal RevisionNumber()
		{
			LocalRevisionNumber = "-1";
			LongHash = HgRepository.EmptyRepoIdentifier;
			Hash = LongHash.Substring(0, 12);
		}

		public RevisionNumber(HgRepository repository, string local, string hash)
			: this()
		{
			LocalRevisionNumber = local;
			Hash = hash;

			SetLongHash(repository);
		}
		public RevisionNumber(HgRepository repository, string combinedNumberAndHash)
			: this()
		{
			string[] parts = combinedNumberAndHash.Split(new char[] { ':' });
			Debug.Assert(parts.Length == 2);
			Hash = parts[1].Trim();
			LocalRevisionNumber = parts[0];

			SetLongHash(repository);
		}

		public string LongHash { get; set; }
		public string Hash { get; set; }
		public string LocalRevisionNumber { get; set; }

		private void SetLongHash(HgRepository repository)
		{
			if (repository == null)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(repository.Identifier))
			{
				// No commits yet.
				return;
			}

			var result = repository.Execute(repository.SecondsBeforeTimeoutOnLocalOperation, string.Format("log -r{0} --template {1}", LocalRevisionNumber, HgRepository.SurroundWithQuotes("{node}"))).StandardOutput.Trim();
			var strArray = result.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
			LongHash = strArray[checked(strArray.Length - 1)];
		}
	}
}