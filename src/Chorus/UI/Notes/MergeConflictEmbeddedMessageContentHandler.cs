using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.notes;
using L10NSharp;

namespace Chorus.UI.Notes
{
	/// <summary>
	///
	/// </summary>
	public class MergeConflictEmbeddedMessageContentHandler : IEmbeddedMessageContentHandler
	{
		/// <summary>
		/// This class is currently only used to keep track of a few links shown in a single small page.
		/// To keep it from accumulating forever, we record only the most recent few
		/// </summary>
		List<LinkData> _recentLinks = new List<LinkData>();

		private int _nextKey;

		private const string fakeHost = "mergeconflict"; // keep lower case, something somewhere converts it anyway.

		public virtual string GetHyperLink(string cDataContent)
		{
			var key = "K" + _nextKey++;
			_recentLinks.Add(new LinkData() { Key = key, Data = cDataContent });
			// for now just keep the most recent 20 links. This is why it is a list not a dictionary.
			if (_recentLinks.Count > 20)
				_recentLinks.RemoveAt(0);
			// Note that here we are using an ID which makes the text of the link the same as the text of the window title.
			return string.Format("<a href={0}>{1}</a>", "http://" + fakeHost + "?data=" + key,
				LocalizationManager.GetString("ConflictDetailsForm.WindowTitle", "Conflict Details") + "...");

			// Old approach, fails with IE if cDataContent is more than about 2038 characters (http://www.codingforums.com/showthread.php?t=18499).
			//NB: this is ugly, pretending it's http and all, but when I used a custom scheme,
			//the resulting url that came to the navigating event had a bunch of junk prepended,
			//so for now, who cares.
			//
			//Anyhow, what we're doing here is taking the cdata contents, making that
			//safe to stick in a giant URL, and making a link of it.
			//THat URL is then decoded in HandleUrl()
			//var encodedData= HttpUtility.UrlEncode(cDataContent);
			//return string.Format("<a href={0}>{1}</a>", "http://mergeConflict?data="+encodedData, "Conflict Details...");
		}

		public bool CanHandleUrl(Uri uri)
		{
			return uri.Host == fakeHost;
		}

		/// <summary>
		/// This functor may be set in order to adjust the HtmlDetails of a Conflict before they are used in setting the
		/// Details link of the conflict report. Flex uses this to substitute the actual project ID for "database=current".
		/// </summary>
		public Func<string, string> HtmlAdjuster { get; set; }

		public void HandleUrl(Uri uri)
		{
			var key = uri.Query.Substring(uri.Query.IndexOf('=') + 1);
			var data = (from item in _recentLinks where item.Key == key select item).FirstOrDefault();
			if (data == null)
			{
				Debug.Fail("page has more links than we can currently handle");
				return; // give up.
			}
			var content = data.Data;
			try
			{
				var doc = new XmlDocument();
				var conflict = Conflict.CreateFromConflictElement(XmlUtilities.GetDocumentNodeFromRawXml(content, doc));
				var html = @"<html>" + conflict.HtmlDetails + @"</html>";
				if (HtmlAdjuster != null)
					html = HtmlAdjuster(html);
				if (string.IsNullOrEmpty(html))
				{
					MessageBox.Show(LocalizationManager.GetString("Messages.NoDetailsRecorded", "Sorry, no conflict details are recorded for this conflict (it might be an old one). Here's the content:") + "\r\n" + content);
					return;
				}
				using (var conflictForm = new ConflictDetailsForm())
				{
					conflictForm.SetDocumentText(html);
					conflictForm.ShowDialog(Form.ActiveForm);
					return;
				}
			}
			catch (Exception)
			{
			}
			MessageBox.Show(LocalizationManager.GetString("Messages.DetailsNotWorking", "Sorry, conflict details aren't working for this conflict (it might be an old one). Here's the content:") + "\r\n" + content);//uri.ToString());
		}

		public bool CanHandleContent(string cDataContent)
		{
			return cDataContent.TrimStart().StartsWith(@"<conflict");
		}
	}

	class LinkData
	{
		public string Key;
		public string Data;
	}
}
