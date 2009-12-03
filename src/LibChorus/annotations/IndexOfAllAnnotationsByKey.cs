using System.Diagnostics;
using System.Web;

namespace Chorus.annotations
{
	public class IndexOfAllAnnotationsByKey : AnnotationIndex
	{
		public IndexOfAllAnnotationsByKey(string label):
			base( a => true, a=> ExtractKeyOutOfRef(a, label))
		{
		}

		private static string ExtractKeyOutOfRef(Annotation annotation, string name)
		{
			if(string.IsNullOrEmpty(annotation.Ref))
				return string.Empty;
			return annotation.GetValueFromQueryStringOfRef(name, string.Empty);
//            var parse =HttpUtility.ParseQueryString(annotation.Ref);
//            var values = parse.GetValues(name);
//            if(values==null || values.Length == 0)
//                return string.Empty;
//            Debug.Assert(values.Length == 1, "Will ignore all but first match (seeing this in debug mode only)");
//            return values[0];
		}
	}
}