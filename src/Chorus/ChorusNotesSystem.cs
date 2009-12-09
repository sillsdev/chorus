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

		public delegate string IdGeneratorFunction(object targetOfAnnotation);

		/// <summary>
		/// Note, this could be callback which also takes into account, say, the current cursor position.
		/// But then, we don't really need to have any arguments to it, do we?  <-- TODO
		/// </summary>
		public delegate string UrlGeneratorFunction(object targetOfAnnotation, string key);

		/// <summary>
		/// set this if you want something other than a default, chorus-generated URL for your objects
		/// note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGeneratorFunction do that.
		/// </summary>
		/// <example>(image) => string.Format("myimages://image?id={0}&amp;type=jpg",image.Guid)</example>
		public UrlGeneratorFunction UrlGenerator { get; set; }

		/// <summary>
		/// The Id is what normally comes after the "id=" in the url
		/// </summary>
		public IdGeneratorFunction IdGenerator { get; set; }

		public static UrlGeneratorFunction DefaultUrlGenerator = (target, id) => string.Format("chorus://object?id={0}", id);
		public static IdGeneratorFunction DefaultIdGeneratorUsingObjectToStringAsId = (target) => target.ToString();

		private readonly Autofac.IContainer _container;
		private readonly AnnotationRepository _repository;

		public ChorusNotesSystem(IContainer parentContainer, AnnotationRepository repository,string pathToFileBeingAnnotated, IProgress progress)
		{
			_container = parentContainer;
			_repository = repository;
			UrlGenerator = DefaultUrlGenerator;
			IdGenerator = DefaultIdGeneratorUsingObjectToStringAsId;
		}

		public NotesBarView CreateNotesBarView()
		{
			var model = _container.Resolve<NotesBarModel.Factory>()(_repository);
			model.UrlGenerator = UrlGenerator;
			model.IdGenerator = IdGenerator;
			return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
		}

		public NotesBrowserPage CreateNotesBrowserPage()
		{
			return _container.Resolve<NotesBrowserPage.Factory>()();
		}
	}
}