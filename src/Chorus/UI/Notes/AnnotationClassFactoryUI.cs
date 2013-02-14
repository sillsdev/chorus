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
			// These images are used (at least) in ListMessage.SetListViewImage. There should be an image for each
			// class of note, plus any special ones indicated in Annotation.IconClassName. For each, there should also be
			// one with "Closed" added to the name, used when the note is closed (resolved).
			list.Images.Add("question", Chorus.Properties.AnnotationImages.question16x16);
			list.Images.Add("questionClosed", Chorus.Properties.AnnotationImages.question16x16Closed);
			list.Images.Add(Conflict.NotificationAnnotationClassName, Chorus.Properties.AnnotationImages.Warning16x16);
			list.Images.Add(Conflict.NotificationAnnotationClassName + "Closed", Chorus.Properties.AnnotationImages.Warning16x16Closed);
			list.Images.Add("note", Chorus.Properties.AnnotationImages.note16x16);
			list.Images.Add("noteClosed", Chorus.Properties.AnnotationImages.note16x16);
			list.Images.Add(Conflict.ConflictAnnotationClassName, Chorus.Properties.AnnotationImages.DataLossMerge16x16);
			list.Images.Add(Conflict.ConflictAnnotationClassName + "Closed", Chorus.Properties.AnnotationImages.DataLossMerge16x16Closed);
			return list;
		}
	}
}
