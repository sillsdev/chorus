using System;
using System.Text;
using Chorus.FileTypeHandlers;
using Chorus.VcsDrivers.Mercurial;
using SIL.Progress;

namespace Chorus.sync
{
	/// <summary>
	/// Use this class to bracket a Commit.  Currently, it will validate any added/modified files it can,
	/// and if they are invalid, it will backout the commit and leave a record what failed so that support
	/// personnel/developers can have easy access to the problem file.
	///
	/// NB: It is crucial that the commit take place.
	///
	/// NB: if you're using the Chorus UI components, or the Synchronizer, this class will be used for you.
	/// You only need to use it explicitly if you're directly committing using calls to the HgRepository.
	///
	/// NB: as of 2 Nov 2009, this cannot handle merge situations (because it seems to me backing out
	/// of a merge would mean losing more data than necessary & we need some other more general
	/// merge-failure handling mechanism, anyhow).
	///
	/// </summary>
	/// <example>
	///   using(var commitCop = new CommitCop(_repository, _progress))
	///    {
	///          Repository.AddAndCheckinFiles(...);
	///          validationResult = commitCop.ValidationResult;
	///    }
	/// </example>
	public class CommitCop:IDisposable
	{
		private readonly HgRepository _repository;
		private readonly ChorusFileTypeHandlerCollection _handlerCollection;
		private readonly IProgress _progress;
		private string _validationResult;

		public CommitCop(HgRepository repository, ChorusFileTypeHandlerCollection handlers, IProgress progress)
		{
			_repository = repository;
			_handlerCollection = handlers;
			_progress = progress;
		}

		public string ValidationResult { get { ValidateModifiedFiles(); return _validationResult; } }

		private void ValidateModifiedFiles()
		{
			var files = _repository.GetFilesInRevisionFromQuery(null, "status --change tip");
			var builder = new StringBuilder();

			foreach (var file in files)
			{
				if (file.ActionThatHappened == FileInRevision.Action.Modified
					|| file.ActionThatHappened == FileInRevision.Action.Added)
				{
					foreach (var handler in _handlerCollection.Handlers)
					{
						if (handler.CanValidateFile(file.FullPath))
						{
							_progress.WriteVerbose("Validating {0}", file);
							var result = handler.ValidateFile(file.FullPath, _progress);
							if (!string.IsNullOrEmpty(result))
							{
								_progress.WriteVerbose("Validation Failed: {0}", result);
								builder.AppendFormat("Validation failed on {0}\r\n", file.FullPath);
								builder.Append(result+"\r\n");
							}
							break;
						}
					}
				}
			}
			_validationResult = builder.ToString();
		}

		public void Dispose()
		{
			if (String.IsNullOrEmpty(_validationResult))
				return;
			_repository.BackoutHead(_repository.GetRevisionWorkingSetIsBasedOn().Number.LocalRevisionNumber, "[Backout due to validation failure]\r\n"+_validationResult);
		}
	}
}