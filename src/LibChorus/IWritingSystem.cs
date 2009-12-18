
namespace Chorus
{
	/// <summary>
	/// THis is used to pass to Chorus font & keyboard information, without requiring you to
	/// use our (Palaso's) WritingSytem classes.
	/// </summary>
	public interface IWritingSystem
	{
		string Name{get;}
		string Code {get;}
		string FontName { get; }
		void ActivateKeyboard();
	}

	public class EnglishWritingSystem: IWritingSystem
	{
		#region Implementation of IWritingSystem

		public string Name
		{
			get { return "English"; }
		}

		public string Code
		{
			get { return "en"; }
		}

		public string FontName
		{
			get { return "Broadway"; }
			//   get { return SystemFonts.MessageBoxFont.FontName.Name; }
		}

		public void ActivateKeyboard()
		{
		}

		#endregion
	}
}