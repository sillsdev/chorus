using System.Collections.Generic;
using System.Linq;
using Chorus.merge;

namespace Chorus.FileTypeHanders
{
	public interface IChorusFileTypeHandler
	{
		bool CanHandleFile(string pathToFile);

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		void Do3WayMerge(merge.MergeOrder mergeOrder);

		IEnumerable<IChangeReport> Find2WayDifferences(string pathToParent, string pathToChild);
		IChangePresenter GetChangePresenter(IChangeReport report);
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
			fileTypeHandlers.Handlers.Add(new TextFileTypeHandler());
			fileTypeHandlers.Handlers.Add(new ConflictFileTypeHandler());
			//NB: never add the Default handler
			return fileTypeHandlers;
		}

		private ChorusFileTypeHandlerCollection()
		{
			Handlers = new List<IChorusFileTypeHandler>();
		}
		public IChorusFileTypeHandler GetHandler(string path)
		{
			var handler = Handlers.FirstOrDefault(h => h.CanHandleFile(path));
			if (handler == null)
			{
				return new DefaultFileTypeHandler();
			}
			return handler;
		}
	}
}
