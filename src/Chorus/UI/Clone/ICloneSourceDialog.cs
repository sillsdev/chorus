namespace Chorus.UI.Clone
{
	/// <summary>
	/// Dialogs used to select and clone new repositories to this computer
	/// </summary>
	public interface ICloneSourceDialog
	{
		string PathToNewlyClonedFolder { get; }

		/// <summary>
		/// Used to check if the repository is the right kind for your program, so that the only projects that can be chosen are ones
		/// you application is prepared to open.
		///
		/// Note: the comparison is based on how hg stores the file name/extenion, not the original form!
		/// </summary>
		/// <example>Bloom uses "*.bloom_collection.i" to test if there is a ".BloomCollection" file</example>
		void SetFilePatternWhichMustBeFoundInHgDataFolder(string pattern);
	}
}