#if MONO
using Gecko;
#else
using System.Windows.Forms;
#endif

namespace Chorus.UI
{
	/// <summary>
	/// Cross-platform web browser control--encapsulates basic browser functionality.
	/// This will save keeping up with #if MONO tags each time a .Designer.cs is regenerated.
	/// </summary>
#if MONO
	public partial class BetterBrowser : GeckoWebBrowser
#else
	public partial class BetterBrowser : WebBrowser
#endif
	{
		/// <summary/>
		public BetterBrowser()
		{
			InitializeComponent();

			// no right context menu needed:
			IsWebBrowserContextMenuEnabled = false;
#if !MONO
			// no need to allow the user to drag and drop a web page on the control:
			AllowWebBrowserDrop = false;
#endif
		}

		//----------------------------------------------------------------------
		// Implement methods and properties from the opposite platform's Browser
		//----------------------------------------------------------------------
#if MONO
		/// <summary>Sets the HTML contents of the page displayed </summary>
		public string DocumentText
		{
			get { return @"<!DOCTYPE HTML></HTML>"; /* TODO: GeckoFX implementation */ }
			set { LoadHtml(value); }
		}

		/// <summary>Whether the shortcut menu is enabled</summary>
		public bool IsWebBrowserContextMenuEnabled
		{
			get { return !NoDefaultContextMenu; }
			set { NoDefaultContextMenu = !value; }
		}
#else
		/// <summary>Sets the HTML contents of the page displayed </summary>
		public void LoadHtml(string value)
		{
			DocumentText = value;
		}

		/// <summary>Whether the shortcut menu is enabled</summary>
		public bool NoDefaultContextMenu
		{
			get { return !IsWebBrowserContextMenuEnabled; }
			set { IsWebBrowserContextMenuEnabled = !value; }
		}
#endif
	}
}
