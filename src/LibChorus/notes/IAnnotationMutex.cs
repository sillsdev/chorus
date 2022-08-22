namespace Chorus.notes
{
	/// <summary>
	/// This interface wraps a named mutex for locking annotation files so we can have platform specific implementations
	/// </summary>
	public interface IAnnotationMutex
	{
		void WaitOne();
		void ReleaseMutex();
	}
}