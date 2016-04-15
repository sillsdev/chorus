// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Autofac;
using Chorus.notes;
using Chorus.Review;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace Chorus
{
	/// <summary>
	/// A ChorusSystem object hides a lot of the complexity of Chorus from the client programmer.
	/// It offers up the most common controls and services of Chorus.
	/// </summary>
	public interface IChorusSystem
	{
		/// <summary>
		/// The Autofac container.
		/// </summary>
		IContainer Container { get; }

		/// <summary>
		/// This is a special init used for functions (such as setting up a NotesBar) which do not
		/// actually require Mercurial. Crashes are likely if you use this and then try functions
		/// like Send/Receive which DO need Hg. This version must be passed a reasonable
		/// userNameForHistoryAndNotes, since there is no way to obtain a default one.
		/// </summary>
		/// <param name="userNameForHistoryAndNotes"></param>
		void InitWithoutHg(string userNameForHistoryAndNotes);

		/// <summary>
		/// Initialize system with user's name.
		/// </summary>
		/// <param name="dataFolderPath">The root of the project</param>
		/// <param name="userNameForHistoryAndNotes">This is not the same name as that used for
		/// any given network repository credentials. Rather, it's the name which will show in
		/// the history, and besides Notes that this user makes.
		///</param>
		void Init(string dataFolderPath, string userNameForHistoryAndNotes);

		/// <summary>
		/// <c>true</c> if loaded correctly
		/// </summary>
		bool DidLoadUpCorrectly { get; }

		/// <summary>
		/// The display settings.
		/// </summary>
		ChorusNotesSettings DisplaySettings { get; }

		/// <summary>
		/// Gets the NavigateToRecord event.
		/// </summary>
		NavigateToRecordEvent NavigateToRecordEvent { get; }

		/// <summary>
		/// Use this to set things like what file types to include/exclude
		/// </summary>
		ProjectFolderConfiguration ProjectFolderConfiguration { get; }

		/// <summary>
		/// Gets the Mercurial repository.
		/// </summary>
		HgRepository Repository { get; }

		/// <summary>
		/// Gets the user name for history and notes.
		/// </summary>
		string UserNameForHistoryAndNotes { get; }

		/// <summary>
		/// Ensures all notes repositories loaded.
		/// </summary>
		void EnsureAllNotesRepositoriesLoaded();

		/// <summary>
		/// Gets the notes repository.
		/// </summary>
		IAnnotationRepository GetNotesRepository(string pathToFileBeingAnnotated, IProgress progress);

		/// <summary>
		/// Gets the notes repository.
		/// </summary>
		IAnnotationRepository GetNotesRepository(string pathToPrimaryFile,
			IEnumerable<string> pathsToOtherFiles, string idAttrForOtherFiles, IProgress progress);

		/// <summary>
		/// Check in, to the local disk repository, any changes to this point.
		/// </summary>
		/// <param name="checkinDescription">A description of what work was done that you're
		/// wanting to checkin. E.g. "Delete a Book"</param>
		/// <param name="callbackWhenFinished">Code to call when the task finishes</param>
		void AsyncLocalCheckIn(string checkinDescription, Action<SyncResults> callbackWhenFinished);
	}
}

