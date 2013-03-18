/* From http://xmlunit.sourceforge.net/ Moved here because the original library is for testing, and
 * it is tied to nunit, which we don't want to ship in production
 */

namespace Chorus.merge.xml.generic.xmldiff
{
	public class Differences {
		private Differences() { }

		public static bool isMajorDifference(DifferenceType differenceType) {
			switch (differenceType) {
				case DifferenceType.EMPTY_NODE_ID: // Fall through
				case DifferenceType.ATTR_SEQUENCE_ID: // Fall through
				case DifferenceType.HAS_XML_DECLARATION_PREFIX_ID:
					return false;
				default:
					return true;
			}
		}

	}
}