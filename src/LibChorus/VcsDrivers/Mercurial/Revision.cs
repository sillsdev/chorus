using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chorus.VcsDrivers.Mercurial
{
	/// <summary>
	/// this class is json serialized, so don't change the names of the properties and don't store service objects in it that can't be serialized
	/// </summary>
	public class Revision
	{
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


		public Revision()
		{
			Parents = new List<RevisionNumber>();
			Tag = string.Empty;
			Branch = string.Empty;
			Summary = string.Empty;
			UserId = string.Empty;
		}

		public Revision(HgRepository repository, string name, string localRevisionNumber, string hash, string comment)
			:this()
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

		public void SetRevisionAndHashFromCombinedDescriptor(string descriptor, HgRepository repository)
		{
			Number = new RevisionNumber(repository, descriptor);
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

		public void AddParentFromCombinedNumberAndHash(string descriptor, HgRepository repository)
		{
			Parents.Add(new RevisionNumber(repository, descriptor));
		}

		public bool IsDirectDescendantOf(Revision revision, HgRepository repository)
		{
			EnsureParentRevisionInfo(repository);
			//TODO: this is only checking direct descendant
			return Parents.Any(p => p.Hash == revision.Number.Hash);
		}

		/// <summary>
		/// I can't for the life of me get hg to indicate parentage in the "hg log" (even with templates
		/// asking for parents), if the revision is not the result of a merge.  And yet, it's expensive
		/// to ask again for every single one.  So as a hack, for now, this  can be called on a revision
		/// where we really need to know the parent.
		/// </summary>
		public void EnsureParentRevisionInfo(HgRepository repository)
		{
			if (this.Parents.Count == 0)
			{
				Parents.AddRange(repository.GetParentsRevisionNumbers(this.Number.LocalRevisionNumber));
			}
		}

		public bool GetMatchesLocalOrHash(string localOrHash)
		{
			return Number.Hash == localOrHash || Number.LocalRevisionNumber == localOrHash;
		}
	}

	public class RevisionNumber
	{
		private const int FullHashLength = 40;

		// Private, so Newtonsoft's default contract (public properties and public fields) leaves them out
		// of revisioncache.json; the serialized shape is unchanged.
		private string _longHash;
		private HgRepository _repositoryForLazyLongHashLookup;

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

			InitializeLongHash(repository);
		}
		public RevisionNumber(HgRepository repository, string combinedNumberAndHash)
			: this()
		{
			string[] parts = combinedNumberAndHash.Split(new char[] { ':' });
			Debug.Assert(parts.Length == 2);
			Hash = parts[1].Trim();
			LocalRevisionNumber = parts[0];

			InitializeLongHash(repository);
		}

		/// <summary>
		/// The full 40-character node. Nearly always supplied by whoever built this: hg's log template
		/// includes longhash:{node}, and some queries (a parent's {p1node}, hg debugancestor) hand us the
		/// full node as the hash to begin with. Only when neither is true do we ask hg, and then only on
		/// first read - which used to happen in the constructor, costing one hg process per revision.
		/// </summary>
		public string LongHash
		{
			get
			{
				var repository = _repositoryForLazyLongHashLookup;
				if (repository != null)
				{
					_repositoryForLazyLongHashLookup = null; // one attempt, ever
					LookUpLongHash(repository);
				}
				return _longHash;
			}
			set
			{
				_longHash = value;
				_repositoryForLazyLongHashLookup = null;
			}
		}

		public string Hash { get; set; }
		public string LocalRevisionNumber { get; set; }

		private void InitializeLongHash(HgRepository repository)
		{
			if (Hash != null && Hash.Length == FullHashLength)
			{
				LongHash = Hash;
				return;
			}
			_repositoryForLazyLongHashLookup = repository; // null repository means no lookup is possible
		}

		private void LookUpLongHash(HgRepository repository)
		{
			if (string.IsNullOrWhiteSpace(repository.Identifier))
			{
				// No commits yet.
				return;
			}

			var result = repository.Execute(repository.SecondsBeforeTimeoutOnLocalOperation, string.Format("log -r{0} --template {1}", LocalRevisionNumber, HgRepository.SurroundWithQuotes("{node}"))).StandardOutput.Trim();
			var strArray = result.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
			if (strArray.Length > 0)
			{
				_longHash = strArray[strArray.Length - 1];
			}
		}
	}
}