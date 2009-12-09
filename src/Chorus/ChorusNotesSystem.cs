using Autofac;
using Chorus.annotations;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.Utilities;

namespace Chorus
{
	public class ChorusNotesSystem
	{
		public delegate ChorusNotesSystem Factory(AnnotationRepository repository, string pathToFileBeingAnnotated, IProgress progress);//autofac uses this

		public delegate string UrlGenerator(string key);

		/// <summary>
		/// set this if you want something other than a default, chorus-generated URL for your objects
		/// note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGenerator do that.
		/// </summary>
		/// <example>(key) => string.Format("myimages://image?id={0}&amp;type=jpg", key)</example>
		public UrlGenerator UrlGenerater { get; set; }

		public static UrlGenerator DefaultGenerator = (key) => string.Format("chorus://object?id={0}", key);

		private readonly Autofac.IContainer _container;
		private readonly AnnotationRepository _repository;

		public ChorusNotesSystem(IContainer parentContainer, AnnotationRepository repository,string pathToFileBeingAnnotated, IProgress progress)
		{
			_container = parentContainer;
			_repository = repository;
			UrlGenerater = DefaultGenerator;
		}

		public NotesBarView CreateNotesBarView()
		{
			var model = _container.Resolve<NotesBarModel.Factory>()(_repository);
			model.UrlGenerater = UrlGenerater;
			return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
		}

		public NotesBrowserPage CreateNotesBrowserPage()
		{
			return _container.Resolve<NotesBrowserPage.Factory>()();
		}
	}
}