using System;
using System.Collections;
using System.Collections.Generic;

namespace Chorus
{
	///<summary>
	/// If you use the Notes Bar, you need to create one of these. You need to give it
	/// two methods. It uses these to get ids and full urls of your objects. That way
	/// notes can point you back to which object was the object of the note, and the
	/// UI (Notes Bar) knows when to show a button leading to an existing note which
	/// is related to what you're showing on screen.
	/// See the SampleApp for an example.
	///</summary>
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

		public delegate IEnumerable<string> AdditionalIdGeneratorFunction(object targetOfAnnotation);


		public static IdGeneratorFunction DefaultIdGeneratorUsingObjectToStringAsId = (target) => target.ToString();
		public static UrlGeneratorFunction DefaultUrlGenerator = (unused, id) => string.Format("chorus://object?id={0}", id);
		public static AdditionalIdGeneratorFunction DefaultAdditionalIdGeneratorFunction = (target) => new string[0];

		/// <summary>
		/// Used to figure out which existing notes to show
		/// The Id is what normally comes after the "id=" in the url
		/// </summary>
		public IdGeneratorFunction FunctionToGoFromObjectToItsId = o => { throw new ArgumentNullException("FunctionToGoFromObjectToItsId", "You need to supply a function for FunctionToGoFromObjectToItsId."); };

		/// <summary>
		/// If the notes bar should show notes about additional object IDs (e.g., conflict reports for owned objects in FLEx),
		/// supply a function that can generate them. If not, the default adds none and is fine.
		/// </summary>
		public AdditionalIdGeneratorFunction FunctionToGoFromObjectToAdditionalIds = DefaultAdditionalIdGeneratorFunction;

		/// <summary>
		/// Used to make new annotations with a url reflecting the current object/insertion-point/whatever.
		/// You must include and "id" part, and normally you'll include your own "label" part too. You can add
		/// other parts to the url as suits you; for example, you might have the id lead you to the right paragraph, but
		/// then add other attributes to tell you which word(s) to highlight.
		/// Note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGeneratorFunction do that.
		/// <example>(escapedId) => string.Format("myimages://image?id={0}&amp;label=myMugShot&amp;type=jpg",escapedId)</example>
		/// <example>(ignoreIt) => string.Format("myimages://image?id={0}&amp;type={1}",_currentImage.Guid, _currentImage.FileType)</example>
		/// </summary>
		public UrlGeneratorFunction FunctionToGetCurrentUrlForNewNotes = DefaultUrlGenerator;
	}
}
