using Chorus.merge.xml.generic;

namespace Chorus.notes
{
	public static class AnnotationClassFactory
	{
		public static AnnotationClass GetClassOrDefault(string name)
		{
			switch (name)
			{
				case "question":
					return new QuestionAnnotationClass();
				case "note":
					return new NoteAnnotationClass();
				case Conflict.ConflictAnnotationClassName:
					return new ConflictAnnotationClass();
				case Conflict.NotificationFakeClassName:
					return new NotificationAnnotationClass();
				default:
					return new AnnotationClass(name);
			}
		}
	}
}