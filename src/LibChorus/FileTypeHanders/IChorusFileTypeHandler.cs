using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHanders.adaptIt;
using Chorus.FileTypeHanders.audio;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.FileTypeHanders.FieldWorks.CustomProperties;
using Chorus.FileTypeHanders.FieldWorks.Linguistics.Reversals;
using Chorus.FileTypeHanders.FieldWorks.ModelVersion;
using Chorus.FileTypeHanders.image;
using Chorus.FileTypeHanders.ldml;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.oneStory;
using Chorus.FileTypeHanders.test;
using Chorus.FileTypeHanders.OurWord;
using Chorus.FileTypeHanders.text;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders
{
	public interface IChorusFileTypeHandler
	{
		bool CanDiffFile(string pathToFile);
		bool CanMergeFile(string pathToFile);
		bool CanPresentFile(string pathToFile);
		bool CanValidateFile(string pathToFile);

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		void Do3WayMerge(MergeOrder mergeOrder);

		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		string ValidateFile(string pathToFile, IProgress progress);

		/// <summary>
		/// This is like a diff, but for when the file is first checked in.  So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file);

		IEnumerable<string> GetExtensionsOfKnownTextFileTypes();

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		uint MaximumFileSize { get; }
	}

	public class ChorusFileTypeHandlerCollection
	{
		private List<IChorusFileTypeHandler> HandlersList { get; set; }
		public IEnumerable<IChorusFileTypeHandler> Handlers { get { return HandlersList; } }

		/// <summary>
		/// This will eventually done using MEF or some other dynamic way of finding available HandlersList
		/// </summary>
		/// <returns></returns>
		public static ChorusFileTypeHandlerCollection CreateWithInstalledHandlers()
		{
			var fileTypeHandlers = new ChorusFileTypeHandlerCollection();
			fileTypeHandlers.HandlersList.Add(new LiftFileHandler());
			fileTypeHandlers.HandlersList.Add(new OneStoryFileHandler());
			fileTypeHandlers.HandlersList.Add(new AdaptItFileHandler());
			fileTypeHandlers.HandlersList.Add(new TextFileTypeHandler());
			fileTypeHandlers.HandlersList.Add(new ChorusNotesFileHandler());
			fileTypeHandlers.HandlersList.Add(new WeSayConfigFileHandler());
			fileTypeHandlers.HandlersList.Add(new AudioFileTypeHandler());
			fileTypeHandlers.HandlersList.Add(new ImageFileTypeHandler());
			fileTypeHandlers.HandlersList.Add(new ChorusTestFileHandler());
			fileTypeHandlers.HandlersList.Add(new OurWordFileHandler());
			fileTypeHandlers.HandlersList.Add(new FieldWorksFileHandler());
			fileTypeHandlers.HandlersList.Add(new FieldWorksCustomPropertyFileHandler());
			fileTypeHandlers.HandlersList.Add(new FieldWorksModelVersionFileHandler());
			fileTypeHandlers.HandlersList.Add(new LiftRangesFileTypeHandler());
			fileTypeHandlers.HandlersList.Add(new LdmlFileHandler());
			fileTypeHandlers.HandlersList.Add(new FieldWorksReversalTypeHandler());

			//NB: never add the Default handler
			return fileTypeHandlers;
		}

		public static ChorusFileTypeHandlerCollection CreateWithTestHandlerOnly()
		{
			var fileTypeHandlers = new ChorusFileTypeHandlerCollection();
			fileTypeHandlers.HandlersList.Add(new ChorusTestFileHandler());

			//NB: never add the Default handler
			return fileTypeHandlers;
		}
		private ChorusFileTypeHandlerCollection()
		{
			HandlersList = new List<IChorusFileTypeHandler>();
		}
		public IChorusFileTypeHandler GetHandlerForMerging(string path)
		{
			var handler = HandlersList.FirstOrDefault(h => h.CanMergeFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
		public IChorusFileTypeHandler GetHandlerForDiff(string path)
		{
			var handler = HandlersList.FirstOrDefault(h => h.CanDiffFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
		public IChorusFileTypeHandler GetHandlerForPresentation(string path)
		{
			var handler = HandlersList.FirstOrDefault(h => h.CanPresentFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
	}
}
