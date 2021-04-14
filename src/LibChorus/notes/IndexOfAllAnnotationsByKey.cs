using Chorus.Utilities;

namespace Chorus.notes
{
	public class IndexOfAllAnnotationsByKey : AnnotationIndex
	{
		public IndexOfAllAnnotationsByKey(string nameOfParameterInRefToIndex):
			base( a => true, a=> ExtractKeyOutOfRef(a, nameOfParameterInRefToIndex))
		{
		}

		private static string ExtractKeyOutOfRef(Annotation annotation, string nameOfParameterInRefToIndex)
		{
			if(string.IsNullOrEmpty(annotation.RefStillEscaped))
				return string.Empty;
			return UrlHelper.GetValueFromQueryStringOfRef(annotation.RefStillEscaped,nameOfParameterInRefToIndex, string.Empty);
//            var parse =HttpUtility.ParseQueryString(annotation.RefStillEscaped);
//            var values = parse.GetValues(nameOfParameterInRefToIndex);
//            if(values==null || values.Length == 0)
//                return string.Empty;
//            Debug.Assert(values.Length == 1, "Will ignore all but first match (seeing this in debug mode only)");
//            return values[0];
		}
	}
}