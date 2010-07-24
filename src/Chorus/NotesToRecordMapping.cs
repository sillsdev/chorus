using System;

namespace Chorus
{
	/// <summary>
	/// This class is used to hold the information needed to map between a note and a piece
	/// of data in the application.
	/// </summary>
	public class NotesToRecordMapping
	{

		static internal NotesToRecordMapping SimpleForTest()
		{
			var m = new NotesToRecordMapping();
			m.FunctionToGoFromObjectToItsId = DefaultIdGeneratorUsingObjectToStringAsId;
			m.FunctionToGetCurrentUrlForNewNotes = DefaultUrlGenerator;
			return m;
		}

		public delegate string UrlGeneratorFunction(object target, string escapedId);

		public delegate string IdGeneratorFunction(object targetOfAnnotation);


		public static IdGeneratorFunction DefaultIdGeneratorUsingObjectToStringAsId = (target) => target.ToString();
		internal static UrlGeneratorFunction DefaultUrlGenerator = (unused, id) => string.Format("chorus://object?id={0}", id);

		/// <summary>
		/// Used to figure out which existing notes to show
		/// The Id is what normally comes after the "id=" in the url
		/// </summary>
		public IdGeneratorFunction FunctionToGoFromObjectToItsId = o => { throw new ArgumentNullException("FunctionToGoFromObjectToItsId", "You need to supply a function for FunctionToGoFromObjectToItsId."); };

		/// <summary>
		/// Used to make new annotations with a url refelctign the current object/insertion-point/whatever
		/// Note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGeneratorFunction do that.
		/// <example>(escapedId) => string.Format("myimages://image?id={0}&amp;type=jpg",escapedId)</example>
		/// <example>(ignoreIt) => string.Format("myimages://image?id={0}&amp;type={1}",_currentImage.Guid, _currentImage.FileType)</example>
		public UrlGeneratorFunction FunctionToGetCurrentUrlForNewNotes = DefaultUrlGenerator;
	}
}