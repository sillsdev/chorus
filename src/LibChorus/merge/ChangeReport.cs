// Copyright (c) 2008-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Providers;

namespace Chorus.merge
{
	public abstract class ChangeReport : IChangeReport
	{
		protected Guid _guid = GuidProvider.Current.NewGuid();

		protected ChangeReport(FileInRevision parent, FileInRevision child)
		{
			ChildFileInRevision = child;
			ParentFileInRevision = parent;
		}

		public FileInRevision ChildFileInRevision { get; private set; }
		public FileInRevision ParentFileInRevision { get; private set; }

		public Guid Guid
		{
			get { return _guid; }
		}

		public virtual string ActionLabel
		{
			get { return GetType().ToString(); }
		}

		public string PathToFile
		{
			get { return ChildFileInRevision == null ? null : ChildFileInRevision.FullPath; }
		}

		public virtual string UrlOfItem
		{
			get { return string.Empty; }
		}

		public override string ToString()
		{
			return GetFullHumanReadableDescription();
		}

		public virtual string GetFullHumanReadableDescription()
		{
			return ActionLabel;
		}

	}
}
