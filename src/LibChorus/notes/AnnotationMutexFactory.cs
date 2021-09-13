using System.Threading;
using SIL.PlatformUtilities;

namespace Chorus.notes
{
	/// <summary>
	/// This factory class creates a cross process mutex for locking Annotation (ChorusNotes) write operations
	/// </summary>
	internal class AnnotationMutexFactory
	{
		public static IAnnotationMutex Create(string annotationFilePath)
		{
			var mutexName = $"ChorusAnnotationRepositoryMutex_{annotationFilePath.GetHashCode()}";
			if (Platform.IsWindows)
			{
				return new WinMutex(false, mutexName);
			}
			return new LinuxMutex(mutexName);
		}

		/// <summary>
		/// This class will create a named mutex which locks across processes
		/// </summary>
		private class WinMutex : IAnnotationMutex
		{
			private readonly Mutex _mutex;

			public WinMutex(bool initiallyOwned, string mutexName)
			{
				_mutex = new Mutex(initiallyOwned, mutexName);
			}

			public void WaitOne()
			{
				_mutex.WaitOne();
			}

			public void ReleaseMutex()
			{
				_mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Currently just a no-op interface implementation since named mutexes are not implemented in Mono
		/// Enhance: implement a platform appropriate cross process file locking mechanism
		/// </summary>
		private class LinuxMutex: IAnnotationMutex
		{
			public LinuxMutex(string mutexName)
			{
			}

			public void WaitOne()
			{
			}

			public void ReleaseMutex()
			{
			}
		}
	}
}