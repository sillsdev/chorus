using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHandlers.test;
using Chorus.Utilities.code;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;
using SIL.IO;
using SIL.Progress;

namespace Chorus.FileTypeHandlers
{
	/// <summary>
	/// Interface that Chorus can use for diffing and merging file(s) with one or more file extensions.
	/// </summary>
	public interface IChorusFileTypeHandler
	{
		/// <summary>
		/// Checks to see if the implementation can do a diff on the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can do the diff on the file, otherwise false.</returns>
		bool CanDiffFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can do a merge on the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can do the merge on the file, otherwise false.</returns>
		bool CanMergeFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can present the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can present the file, otherwise false.</returns>
		bool CanPresentFile(string pathToFile);

		/// <summary>
		/// Checks to see if the implementation can validate the given file.
		/// </summary>
		/// <param name="pathToFile">The file pathname to check.</param>
		/// <returns>True, if it can validate the file, otherwise false.</returns>
		bool CanValidateFile(string pathToFile);

		/// <summary>
		/// Do a 3-file merge, placing the result over the "ours" file and returning an error status
		/// </summary>
		/// <remarks>Implementations can exit with an exception, which the caller will catch and deal with.
		/// The must not have any UI, no interaction with the user.</remarks>
		void Do3WayMerge(MergeOrder mergeOrder);

		/// <summary>
		/// Do a 2-way diff.
		/// </summary>
		/// <param name="parent">The parent revision</param>
		/// <param name="child">The child revision of the parent</param>
		/// <param name="repository">The Mercurial repository</param>
		/// <returns>A collection of zero or more reports of changes between parent and child versions</returns>
		IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository);

		/// <summary>
		/// Get a presenter for the given change report.
		/// </summary>
		/// <param name="report">The change report to use to produce the presewntation</param>
		/// <param name="repository">mercurial repository</param>
		/// <returns>A presentation of the given change report</returns>
		IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository);

		/// <summary>
		/// return null if valid, otherwise nice verbose description of what went wrong
		/// </summary>
		string ValidateFile(string pathToFile, IProgress progress);

		/// <summary>
		/// This is like a diff, but for when the file is first checked in. So, for example, a dictionary
		/// handler might list any the words that were already in the dictionary when it was first checked in.
		/// </summary>
		IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file);

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
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

			var libChorusAssembly = Assembly.GetExecutingAssembly();

			//Set the codebase variable appropriately depending on the OS
			var codeBase = libChorusAssembly.CodeBase.Substring(LinuxUtils.IsUnix ? 7 : 8);

			var dirname = Path.GetDirectoryName(codeBase);
			//var baseDir = new Uri(dirname).AbsolutePath; // NB: The Uri class in Windows and Mono are not the same.
			var baseDir = dirname;
			var pluginAssemblies = new List<Assembly>
									{
										libChorusAssembly
									};
			foreach (var pluginDllPathname in Directory.GetFiles(baseDir, "*ChorusPlugin.dll", SearchOption.TopDirectoryOnly))
				pluginAssemblies.Add(Assembly.LoadFrom(Path.Combine(baseDir, pluginDllPathname)));

			foreach (var pluginAssembly in pluginAssemblies)
			{
				var fileHandlerTypes = (pluginAssembly.GetTypes()
					.Where(typeof (IChorusFileTypeHandler).IsAssignableFrom)).ToList();
				foreach (var fileHandlerType in fileHandlerTypes)
				{
					if (fileHandlerType.Name == "DefaultFileTypeHandler" || fileHandlerType.IsInterface)
						continue;
					var constInfo = fileHandlerType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
					if (constInfo == null)
						continue; // It does need at least one public or non-public default constructor.
					fileTypeHandlers.HandlersList.Add((IChorusFileTypeHandler)constInfo.Invoke(BindingFlags.Public | BindingFlags.NonPublic, null, null, null));
				}
			}

			// NB: Never add the Default handler. [RBR says: "Not to worry, since a test makes sure it is not in 'fileTypeHandlers'."]
			return fileTypeHandlers;
		}

		public static ChorusFileTypeHandlerCollection CreateWithTestHandlerOnly()
		{
			var fileTypeHandlers = new ChorusFileTypeHandlerCollection();

			fileTypeHandlers.HandlersList.Add(new ChorusTestFileHandler());

			// NB: Never add the Default handler. [RBR says: "Not to worry, since a test makes sure it is not in 'fileTypeHandlers'."]
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
