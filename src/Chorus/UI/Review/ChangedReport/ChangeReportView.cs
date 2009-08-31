using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.retrieval;

namespace Chorus.UI.Review.ChangedReport
{
	public partial class ChangeReportView : UserControl
	{
		private readonly ChorusFileTypeHandlerCollection _handlers;
		private readonly RevisionInspector _revisionInspector;
		private string _styleSheet;

		public ChangeReportView(ChorusFileTypeHandlerCollection handlers, ChangedRecordSelectedEvent changedRecordSelectedEvent, RevisionInspector revisionInspector)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_handlers = handlers;
			_revisionInspector = revisionInspector;
			InitializeComponent();
			_normalChangeDescriptionRenderer.Font = SystemFonts.MessageBoxFont;
			changedRecordSelectedEvent.Subscribe(r=>LoadReport(r));
			_normalChangeDescriptionRenderer.Navigated += webBrowser1_Navigated;

			_styleSheet = @"<style type='text/css'><!--

BODY { font-family: verdana,arial,helvetica,sans-serif; font-size: 12px;}

span.langid {color: 'gray'; font-size: xx-small;position: relative;
	top: 0.3em;
}

span.fieldLabel {color: 'gray'; font-size: x-small;}

div.entry {color: 'blue';font-size: x-small;}

td {font-size: x-small;}

span.en {
color: 'green';
}
span.es {
color: 'green';
}
span.fr {
color: 'green';
}
span.tpi {
color: 'purple';
}

--></style>";
		}

		private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			//didn't work, 'cuase is't actually still being held by the browser
			//  File.Delete(e.URI.AbsoluteUri.Replace(@"file:///", string.Empty));
		}

		public void LoadReport(IChangeReport report)
		{
			if (report == null)
			{
				_normalChangeDescriptionRenderer.Navigate(string.Empty);
			}
			else
			{
				var presenter = _handlers.GetHandlerForPresentation(report.PathToFile).GetChangePresenter(report, _revisionInspector.Repository);
				var path = Path.GetTempFileName();
				File.WriteAllText(path, presenter.GetHtml("normal", _styleSheet));
				this._normalChangeDescriptionRenderer.Navigate(path);
				path = Path.GetTempFileName();
				string contents;
				try
				{
					contents = presenter.GetHtml("raw", _styleSheet);
				}
				catch (Exception error)
				{
					contents = error.Message;
				}

				if (!string.IsNullOrEmpty(contents))
				{
					if(!tabControl1.TabPages.Contains(tabPageRaw))
						this.tabControl1.TabPages.Add(tabPageRaw);
					File.WriteAllText(path, contents);
					this._rawChangeDescriptionRenderer.Navigate(path);
				}
				else
				{
					this.tabControl1.TabPages.Remove(tabPageRaw);
				}
			}
		}

		private void _normalChangeDescriptionRenderer_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			if (e.Url.Scheme == "playaudio")
			{
				e.Cancel = true;

				string url = e.Url.LocalPath;
				var player = new SoundPlayer(e.Url.LocalPath);
				player.Play();
			}

		}
	}
}