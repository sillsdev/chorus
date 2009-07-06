using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Chorus.merge;

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
		/// The very first revision has no parent
		/// </summary>
		public bool HasParentRevision
		{
			get { return !string.IsNullOrEmpty(GetParentLocalNumber()); }
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

		public string GetParentLocalNumber()
		{
			return _repository.GetLocalNumberForParentOfRevision(this.LocalRevisionNumber);
		}
	}
}