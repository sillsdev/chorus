using Chorus.notes;

namespace Chorus
{
	public interface IChorusUser
	{
		string Name { get; }

		bool CanAddAnnotationClass(string annotationClass);
		bool CanCloseAnnotation(Annotation annotation);
		bool CanViewAnnotation(Annotation annotation);
		bool CanDeleteAnnotation(Annotation annotation);
	}

	public class ChorusUser : IChorusUser
	{
//        public ChorusUser CreateFromRepositoryOrOperatingSystemUserName()
//        {
//            return new ChorusUser(c.Resolve<HgRepository>().GetUserIdInUse());
//        }

		public ChorusUser(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
		public bool CanAddAnnotationClass(string annotationClass)
		{
			return true;
		}

		public bool CanCloseAnnotation(Annotation annotation)
		{
			return true;
		}

		public bool CanViewAnnotation(Annotation annotation)
		{
			return true;
		}

		public bool CanDeleteAnnotation(Annotation annotation)
		{
			return true;
		}
	}
}