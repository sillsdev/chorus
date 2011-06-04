namespace Chorus.sync
{
	///<summary>
	/// Class that filters out large, binary, files from being put into the repo,
	/// or if a binary file already is in the repo, then don't commit it if it exceeds the maximum allowablew size.
	///
	/// JohnH comments:
	///		"we could use a “large file” filter in Chorus.
	///		Not one based in the “CommitCop class” (which would be easy but all-or-nothing),
	///		but rather something more complicated. We need something which
	///			1)	Decides whether to include the file based on its size
	///			2)	Decides to “forget” a file when it was previously small but is now too big.
	///
	///		And of course we’d have to have really good reporting,
	///		including perhaps a place-holder file or annotation or something,
	///		so that what happened is communicated throughout the team.
	///
	///		I don’t know if that’s worth the effort to you, but I think those would be the requirements
	///		before we allow video or add audio which is likely to be story-length."
	///
	///		"As it stands, we cannot allow code in Chorus which allows video files,
	///		nor software which uses audio beyond utterance length.
	///		If a project wants to allow either of those, Chorus will need a max-file size limit...
	///
	///		Ideally, it would also look at the whole changeset, and be able to break a too-large one into smaller sets.
	///		Lacking that, perhaps Chorus clients need to do a checkin after each media addition of any large size.
	///		For that matter, all clients should be doing their own filtering, in order to give a good user experience.
	///		If a too-large file finds its way all the way to chorus and gets rejected, that will be confusing.
	///		Chorus should still have the check, though, in case the media file grows without the client noticing."
	///
	/// Cambell comments:
	///		"Well I'd like to suggest absolutely not (well more than suggest) allowing the user to add large files to the repo.
	///		I'd go so far as to say we shouldn't allow video file extensions out right.
	///		And it would be better if we went as far as restricting the max file size of a binary.
	///		At this stage we don't, and it hasn't been a problem.
	///
	///		The key point is, if even one large file is committed to the repo,
	///		that repo is completely dead for all users of that project.
	///		I'm sure that is unacceptable to everyone.
	///
	///		I'd err on the side of precision, and our policies rather than the users discretion and what they could really try."
	///
	/// How to set the limits:
	///		1. hard-coded
	///		2. project manager determined
	///		3. Registry setting
	///		4. ??
	///</summary>
	public class LargeFileFilter
	{

	}
}