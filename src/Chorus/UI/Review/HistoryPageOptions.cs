using System;
using System.Collections.Generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Review
{
	/// <summary>
	/// Used to customize the history page (columns, filters, etc)
	/// </summary>
	public class HistoryPageOptions
	{
		public RevisionListOptions RevisionListOptions = new RevisionListOptions();

		//in the future, we could add other options, e.g. for the details panes
	}


	/// <summary>
	/// Used to customize the history page (columns, filters, etc)
	/// </summary>
	public class RevisionListOptions
	{
		/// <summary>
		/// When false, the user can choose a single revision and see what change in that revision.
		/// When true, the user can choose any two revisions, and will see what changed
		/// between them.
		/// </summary>
		public bool ShowRevisionChoiceControls;

		/// <summary>
		/// Supply a function here which returns false for any revisions you want to hide.
		/// </summary>
		public Func<Revision, bool> RevisionsToShowFilter = (revision) => true;

		/// <summary>
		/// Set this to a new list of columns, if you need some custom ones.
		/// </summary>
		public IEnumerable<HistoryColumnDefinition> ExtraColumns = new List<HistoryColumnDefinition>();
	}


	/// <summary>
	/// Used to add application-specific columns to the history grid
	/// </summary>
	public class HistoryColumnDefinition
	{
		public string ColumnLabel;
		public Func<Revision, string> StringSupplier = (revision) => "----";
	}
}