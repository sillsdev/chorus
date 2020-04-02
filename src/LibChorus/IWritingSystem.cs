using System.Drawing;

namespace Chorus
{
	/// <summary>
	/// This is used to pass font &amp; keyboard information to Chorus without requiring clients to
	/// use our (Palaso's) WritingSystem classes.
	/// </summary>
	public interface IWritingSystem
	{
		string Name{get;}
		string Code {get;}
		string FontName { get; }
		int FontSize { get; }
		void ActivateKeyboard();
	}

	/// <summary>
	/// Implement the Chorus idea of a minimal writing system.
	/// </summary>
	public class ChorusWritingSystem : IWritingSystem
	{
		/// <summary>
		/// Initializes a new instance of the ChorusWritingSystem class.
		/// </summary>
		public ChorusWritingSystem(string name, string code, string fontname, int fontsize)
		{
			Name = name;
			Code = code;
			FontName = fontname;
			FontSize = fontsize;
		}
		/// <summary></summary>
		public string Name { get; private set; }
		/// <summary></summary>
		public string Code { get; private set; }
		/// <summary></summary>
		public string FontName { get; private set; }
		/// <summary></summary>
		public int FontSize { get; private set; }
		/// <summary></summary>
		public virtual void ActivateKeyboard()
		{
		}
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
			//get { return "Broadway"; }
			get { return SystemFonts.MessageBoxFont.Name; }
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