using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHanders.adaptIt;
using Chorus.FileTypeHanders.audio;
using Chorus.FileTypeHanders.image;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.oneStory;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	public interface IChorusFileTypeHandler
	{
		bool CanDiffFile(string pathToFile);
		bool CanMergeFile(string pathToFile);
		bool CanPresentFile(string pathToFile);

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		void Do3WayMerge(merge.MergeOrder mergeOrder);

		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);


		/// <summary>
		/// This is like a diff, but for when the file is first checked in.  So, for example, a dictionary
		/// hanlder might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision,TempFile file);

		IEnumerable<string> GetExtensionsOfKnownTextFileTypes();
	}

	public class ChorusFileTypeHandlerCollection
	{
		public List<IChorusFileTypeHandler> Handlers { get; private set; }

		/// <summary>
		/// THis will eventually done using MEF or some other dynamic way of finding available handlers
		/// </summary>
		/// <returns></returns>
		public static ChorusFileTypeHandlerCollection CreateWithInstalledHandlers()
		{
			var fileTypeHandlers = new ChorusFileTypeHandlerCollection();
			fileTypeHandlers.Handlers.Add(new LiftFileHandler());
			fileTypeHandlers.Handlers.Add(new OneStoryFileHandler());
			fileTypeHandlers.Handlers.Add(new AdaptItFileHandler());
			fileTypeHandlers.Handlers.Add(new TextFileTypeHandler());
			fileTypeHandlers.Handlers.Add(new ConflictFileTypeHandler());
			fileTypeHandlers.Handlers.Add(new WeSayConfigFileHandler());
			fileTypeHandlers.Handlers.Add(new AudioFileTypeHandler());
			fileTypeHandlers.Handlers.Add(new ImageFileTypeHandler());

			//NB: never add the Default handler
			return fileTypeHandlers;
		}

		private ChorusFileTypeHandlerCollection()
		{
			Handlers = new List<IChorusFileTypeHandler>();
		}
		public IChorusFileTypeHandler GetHandlerForMerging(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanMergeFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
		public IChorusFileTypeHandler GetHandlerForDiff(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanDiffFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
		public IChorusFileTypeHandler GetHandlerForPresentation(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanPresentFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
	}
}
