using System;
using System.Collections.Generic;
using System.Text;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.merge
{
	/// <summary>
	/// gives information about a change in a file, e.g. a dictionary entry that was changed.
	/// Note that converting this raw info into human-readable, type-sensitive display
	/// is the reponsibility of implementers of IChangePresenter
	/// </summary>
	public interface IChangeReport
	{
		Guid  Guid { get; }
		string GetFullHumanReadableDescription();
		string ActionLabel { get; }
		string PathToFile { get; }
		string UrlOfItem { get; }
	}

	public abstract class ChangeReport : IChangeReport
	{
		public FileInRevision ChildFileInRevision { get; private set; }
		public FileInRevision ParentFileInRevision { get; private set; }
		protected Guid _guid = Guid.NewGuid();

//        public override bool Equals(object obj)
//        {
//            if(GetType() != obj.GetType())
//                return false;
//            IChangeReport other = obj as IChangeReport;
//            return PathToFile == other.PathToFile
//                && ActionLabel == other.ActionLabel;  //don't compare guids!
//        }
//        public override int GetHashCode()
//        {
//            return PathToFile.GetHashCode() + ActionLabel.GetHashCode();
//        }


//        protected ChangeReport(FileInRevision fileInRevision)
//        {
//            ChildFileInRevision = fileInRevision;
//        }
		protected ChangeReport(FileInRevision parent, FileInRevision child)
		{
			ChildFileInRevision = child;
			ParentFileInRevision = parent;
		}
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
			get
			{
				if(null== ChildFileInRevision)
					return null;
				else
					return ChildFileInRevision.FullPath;
			}
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
