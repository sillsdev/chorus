// Copyright (c) 2008-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
	public class DefaultChangeReport : ChangeReport
	{
		private readonly string _label;

		public DefaultChangeReport(FileInRevision initial, string label)
			: base(null, initial)
		{
			_label = label;
		}

		public DefaultChangeReport(FileInRevision parent, FileInRevision child, string label)
			: base(parent, child)
		{
			_label = label;
		}

		public override string ActionLabel
		{
			get { return _label; }
		}
	}
}

