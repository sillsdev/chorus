using System.Collections.Generic;

namespace Chorus.UI.Notes
{
	/// <summary>
	/// Used to pass stuff like fonts and keyboards from the client to the ChorusNotesSystem
	/// </summary>
	public class ChorusNotesDisplaySettings
	{
		public ChorusNotesDisplaySettings()
		{
			var defaultWritingSystem = new EnglishWritingSystem();
			WritingSystems = new List<IWritingSystem> {defaultWritingSystem};
			WritingSystemForNoteContent = defaultWritingSystem;
			WritingSystemForNoteLabel = defaultWritingSystem;
		}

		/// <summary>
		/// Set this if you want something other than English.
		/// </summary>
		public IEnumerable<IWritingSystem> WritingSystems
		{
			get;
			set;
		}

		/// <summary>
		/// WeSay 1.4 uses the 1st WS of its notes field.
		/// </summary>
		public IWritingSystem WritingSystemForNoteContent
		{
			get;
			set;
		}

		/// <summary>
		/// WeSay 1.4 uses the 1st WS of its lexical form field
		/// </summary>
		public IWritingSystem WritingSystemForNoteLabel
		{
			get;
			set;
		}
	}
}
