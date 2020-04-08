using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Chorus.Utilities;
using SIL.Code;
using SIL.Progress;
using SIL.Xml;

namespace Chorus.notes
{
	public class AnnotationRepository : IDisposable, IAnnotationRepository
	{
		private XDocument _doc;
		private readonly string _annotationFilePath;
		private DateTime _lastAnnotationFileWriteTime;
		private static int kCurrentVersion=0;
		public static string FileExtension = "ChorusNotes";
		private List<IAnnotationRepositoryObserver> _observers = new List<IAnnotationRepositoryObserver>();
		private AnnotationIndex _indexOfAllAnnotationsByKey;
		private bool _isDirty;
		private FileSystemWatcher _watcher = null;
		private bool _writingFileOurselves = false;

		public string AnnotationFilePath
		{
			get { return _annotationFilePath; }
		}

		public static AnnotationRepository FromFile(string primaryRefParameter, string path, IProgress progress)
		{
			try
			{
				if(!File.Exists(path))
				{
					RequireThat.Directory(Path.GetDirectoryName(path)).Exists();
					File.WriteAllText(path, string.Format("<notes version='{0}'/>", kCurrentVersion.ToString()));
				}
				var doc = XDocument.Load(path);
				ThrowIfVersionTooHigh(doc, path);
				var result = new AnnotationRepository(primaryRefParameter, doc, path, progress);
				// Set up a watcher so that if something else...typically FLEx...modifies our file, we update our display.
				// This is useful in its own right for keeping things consistent, but more importantly, if we don't do
				// this and then the user does something in FlexBridge that changes the file, we could overwrite the
				// changes made elsewhere (LT-20074).
				result.UpateAnnotationFileWriteTime();
				result._watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
				result._watcher.NotifyFilter = NotifyFilters.LastWrite;
				result._watcher.Changed += result.UnderlyingFileChanged;
				result._watcher.EnableRaisingEvents = true;
				return result;
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty, error);
			}
		}

		private void UpateAnnotationFileWriteTime()
		{
			_lastAnnotationFileWriteTime = new FileInfo(AnnotationFilePath).LastWriteTime;
		}

		private void UnderlyingFileChanged(object sender, FileSystemEventArgs e)
		{
			// We check both these things because there seems to be some async behavior involved in exactly
			// when the change notification fires. If it fires before we note the new modify time,
			// _writingFileOurselves will still be true. If it fires after we noted the new write time
			// when we wrote it ourselves and cleared _writingFileOurselves, we will see that the write
			// time has not changed.
			if (!_writingFileOurselves  && new FileInfo(AnnotationFilePath).LastWriteTime > _lastAnnotationFileWriteTime)
			{
				UpateAnnotationFileWriteTime();
				_doc = XDocument.Load(AnnotationFilePath);
				SetupDocument();
				_observers.ForEach(observer => observer.NotifyOfStaleList());
			}
		}

		public static AnnotationRepository FromString(string primaryRefParameter, string contents)
		{
			try
			{
				XDocument doc = XDocument.Parse(contents);
				ThrowIfVersionTooHigh(doc, "unknown");
				return new AnnotationRepository(primaryRefParameter, doc, string.Empty, new NullProgress());
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty,error);
			}
		}

		public AnnotationRepository(string primaryRefParameter, XDocument doc, string path, IProgress progress)
		{
			_doc = doc;
			_annotationFilePath = path;

			SetupDocument();
			SetPrimaryRefParameter(primaryRefParameter, progress);
		}

		private void SetupDocument()
		{
			if (_doc.Root != null)
			{
				foreach (var element in _doc.Root.Elements())
				{
					//nb: this is not going to catch a change to, say a message. The current model
					// is that the internals of a message never change.
					element.Changed += new EventHandler<XObjectChangeEventArgs>(AnnotationElement_Changed);
				}
			}
		}

		/// <summary>
		/// The repository defaults to using "id" as the parameter to look for in the ref url of an annotation.
		/// </summary>
		private void SetPrimaryRefParameter(string primaryRefParameter, IProgress progress)
		{
			if (_indexOfAllAnnotationsByKey != null)
			{
				_observers.Remove(_indexOfAllAnnotationsByKey);
			}
			if(!string.IsNullOrEmpty(primaryRefParameter))
			{
				_indexOfAllAnnotationsByKey = new IndexOfAllAnnotationsByKey(primaryRefParameter);
				AddObserver(_indexOfAllAnnotationsByKey, progress);
			}
		}


		public void Dispose()
		{
			if (_doc.Root != null)
			{
				foreach (var element in _doc.Root.Elements())
				{
					element.Changed -= AnnotationElement_Changed;
				}
			}

			if (_watcher != null)
			{
				_watcher.Dispose();
			}
			SaveNowIfNeeded(new NullProgress());
			_doc = null;
		}

		/// <summary>
		/// a typical observer is an index or a user-interface element
		/// </summary>
		public void AddObserver(IAnnotationRepositoryObserver observer, IProgress progress)
		{
			if (_observers.Exists(i => i.GetType() == observer.GetType()))
			{
				//fail fast.
				throw new ApplicationException("An observer of the type " + observer.GetType().ToString() + " is already in the repository.");
			}
			_observers.Add(observer);
			observer.Initialize(GetAllAnnotations, progress);
		}

		public void RemoveObserver(IAnnotationRepositoryObserver observer)
		{
			if(_observers.Contains(observer))
				_observers.Remove(observer);
		}


		public IEnumerable<Annotation> GetAllAnnotations()
		{
			return from a in _doc.Root.Elements() select new Annotation(a) {AnnotationFilePath = AnnotationFilePath};
		}

		public IEnumerable<Annotation> GetByCurrentStatus(string status)
		{
			return from a in _doc.Root.Elements()
				   where Annotation.GetStatusOfLastMessage(a) == status
				   select new Annotation(a);
		}

//        public void SaveAs(string path)
//        {
//            _doc.Save(path);
//        }

		public void Save(IProgress progress)
		{
			try
			{
				if (string.IsNullOrEmpty(AnnotationFilePath))
				{
					throw new InvalidOperationException(
						"Cannot save if the repository was created from a string");
				}

				progress.WriteStatus("Saving Chorus Notes...");
				_writingFileOurselves = true;
				using (var writer = XmlWriter.Create(AnnotationFilePath,
					CanonicalXmlSettings.CreateXmlWriterSettings())
				)
				{
					_doc.Save(writer);
				}

				progress.WriteStatus("");
				_isDirty = false;
				UpateAnnotationFileWriteTime();
			}
			catch(Exception e)
			{
				SIL.Reporting.ErrorReport.NotifyUserOfProblem(e, "Chorus has a problem saving notes for {0}.",
																 _annotationFilePath);
			}
			finally
			{
				_writingFileOurselves = false;
			}
		}

		public void AddAnnotation(Annotation annotation)
		{
			_doc.Root.Add(annotation.Element);
			_observers.ForEach(index => index.NotifyOfAddition(annotation));
			annotation.Element.Changed += new EventHandler<XObjectChangeEventArgs>(AnnotationElement_Changed);
			_isDirty = true;
		}

		void AnnotationElement_Changed(object sender, XObjectChangeEventArgs e)
		{
			//nb: the e.ObjectChange arg appears to be about what happened inside the element, not
			//really what we care about here. So we just say its modified and don't worry about
			//what it was

			var element = sender as XElement;
			XElement annotationElement = element.AncestorsAndSelf("annotation").First();
			_observers.ForEach(index => index.NotifyOfModification(new Annotation(annotationElement)));
			_isDirty = true;
		}

		private static void ThrowIfVersionTooHigh(XDocument doc, string path)
		{
			var version = doc.Element("notes").Attribute("version").Value;
			if (Int32.Parse(version) > kCurrentVersion)
			{
				throw new AnnotationFormatException(
					"The notes file {0} is of a newer version ({1}) than this version of the program supports ({2}).",
					path, version, kCurrentVersion.ToString());
			}
		}

		public IEnumerable<Annotation> GetMatches(Func<Annotation,  bool> predicate)
		{
			return from a in _doc.Root.Elements()
				   where predicate(new Annotation(a))    //enhance... very ineffienct making these constantly
				   select new Annotation(a);
		}

		public IEnumerable<Annotation> GetMatches(Func<Annotation, string, bool> predicate, string parameter)
		{
			return from a in _doc.Root.Elements()
				   where predicate(new Annotation(a), parameter)    //enhance... very ineffienct making these constantly
				   select new Annotation(a);
		}

		public void Remove(Annotation annotation)
		{
			 annotation.Element.Changed -= new EventHandler<XObjectChangeEventArgs>(AnnotationElement_Changed);
			 _observers.ForEach(index => index.NotifyOfDeletion(annotation));
			 annotation.Element.Remove();
		   _isDirty = true;
		}

		public IEnumerable<Annotation> GetMatchesByPrimaryRefKey(string key)
		{
			if(_indexOfAllAnnotationsByKey ==null)
			{
				throw new ArgumentException("The index was not created... make sure you specified a non-empty primaryRefParameter");
			}
			return _indexOfAllAnnotationsByKey.GetMatchesByKey(key);
		}

		public void SaveNowIfNeeded(IProgress progress)
		{
			if(_isDirty && !string.IsNullOrEmpty(AnnotationFilePath))
				Save(progress);
		}

		public static IEnumerable<AnnotationRepository> CreateRepositoriesFromFolder(string folderPath, IProgress progress)
		{
			foreach (var path in GetChorusNotesFilePaths(folderPath))
			{
				yield return AnnotationRepository.FromFile("id", path, progress);
			}
		}

		private static IEnumerable<string> GetChorusNotesFilePaths(string path)
		{
			var mercurialFolders = Directory.GetDirectories(path, ".hg", SearchOption.AllDirectories).ToList();
			var mainRepositoryPath = Path.Combine(path, ".hg");
			mercurialFolders.Remove(mainRepositoryPath);
			foreach (var mercurialFolder in mercurialFolders.ToList())
			{
				mercurialFolders.Remove(mercurialFolder);
				mercurialFolders.Add(Directory.GetParent(mercurialFolder).FullName);
			}

			var chorusNotesPathnames = Directory.GetFiles(path, "*." + FileExtension, SearchOption.AllDirectories).ToList();
			foreach (var chorusNotesPathname in chorusNotesPathnames.ToList())
			{
				if (mercurialFolders.Any(mercurialFolder => chorusNotesPathname.Contains(mercurialFolder)))
				{
					chorusNotesPathnames.Remove(chorusNotesPathname);
				}
			}
			return chorusNotesPathnames;
		}


		public bool ContainsAnnotation(Annotation annotation)
		{
			return null!= _doc.Root.Elements().FirstOrDefault(e => e.GetAttributeValue("guid") == annotation.Guid);
		}
	}

	public class AnnotationFormatException : ApplicationException
	{
		public AnnotationFormatException(string message, Exception exception)
			: base(message, exception)
		{
		}
		public AnnotationFormatException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

	}
}