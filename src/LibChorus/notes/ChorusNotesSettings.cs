// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;

namespace Chorus.notes
{
	/// <summary>
	/// Used to pass stuff like fonts &amp; keyboards from the client to the ChorusNotesSystem
	/// </summary>
	public class ChorusNotesSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Chorus.notes.ChorusNotesSettings"/> class.
		/// </summary>
		public ChorusNotesSettings()
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
