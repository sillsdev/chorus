using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHandlers.text
{
	public class TextEditChangeReport : ChangeReport
	{
		private readonly string _before;
		private readonly string _after;

		public TextEditChangeReport(FileInRevision fileInRevision, string before, string after)
			: base(null, fileInRevision)
		{
			_before = before;
			_after = after;
		}

		public TextEditChangeReport(FileInRevision parent, FileInRevision child, string before, string after)
			: base(parent, child)
		{
			_before = before;
			_after = after;
		}


		//when merging, the eventual revision is unknown
		public TextEditChangeReport(string fullPath, string before, string after)
			: this(new FileInUnknownRevision(fullPath, FileInRevision.Action.Modified), before, after)
		{

		}

		public override string ActionLabel
		{
			get { return "Edited"; }
		}

		public override string GetFullHumanReadableDescription()
		{
			return string.Format("Changed '{0}' to '{1}'", _before, _after);
		}
	}
}