
using System;

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
		int FontSize { get; }
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

		public int FontSize
		{
			get { return 12; }
		}

		public void ActivateKeyboard()
		{
		}

		#endregion
	}
	public class ThaiWritingSystem : IWritingSystem
	{
		#region Implementation of IWritingSystem

		public string Name
		{
			get { return "Thai"; }
		}

		public string Code
		{
			get { return "th"; }
		}

		public string FontName
		{
			get { return "Angsana New"; }
			//   get { return SystemFonts.MessageBoxFont.FontName.Name; }
		}

		public int FontSize
		{
			get { return 16; }
		}

		public void ActivateKeyboard()
		{
		}

		#endregion
	}
}