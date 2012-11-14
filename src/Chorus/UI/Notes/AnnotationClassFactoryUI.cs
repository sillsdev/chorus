using System.Windows.Forms;
using Chorus.merge.xml.generic;

namespace Chorus.UI.Notes
{
	public static class AnnotationClassFactoryUI
	{
		public static ImageList CreateImageListContainingAnnotationImages()
		{
			var list = new ImageList();
			list.ColorDepth = ColorDepth.Depth32Bit;
			list.Images.Add("question", Chorus.Properties.AnnotationImages.question16x16);
			list.Images.Add("questionClosed", Chorus.Properties.AnnotationImages.question16x16Closed);
			list.Images.Add(Conflict.ConflictAnnotationClassName, Chorus.Properties.AnnotationImages.MergeConflict16x16);
			list.Images.Add(Conflict.ConflictAnnotationClassName + "Closed", Chorus.Properties.AnnotationImages.MergeConflict16x16Closed);
			list.Images.Add("note", Chorus.Properties.AnnotationImages.note16x16);
			list.Images.Add("noteClosed", Chorus.Properties.AnnotationImages.note16x16);
			return list;
		}


	}
}
