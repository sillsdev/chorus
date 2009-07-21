using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Chorus.FileTypeHanders;
using Chorus.merge;

namespace Baton.Review.ChangedReport
{
	public partial class ChangeReportView : UserControl
	{
		private readonly ChorusFileTypeHandlerCollection _handlers;

		public ChangeReportView(ChorusFileTypeHandlerCollection handlers, Review.ChangedRecordSelectedEvent changedRecordSelectedEvent)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_handlers = handlers;
			InitializeComponent();
			_normalChangeDescriptionRenderer.Font = SystemFonts.MessageBoxFont;
			changedRecordSelectedEvent.Subscribe(r=>LoadReport(r));
			_normalChangeDescriptionRenderer.Navigated += webBrowser1_Navigated;
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
				var presenter = _handlers.GetHandlerForPresentation(report.PathToFile).GetChangePresenter(report);
				var path = Path.GetTempFileName();
				File.WriteAllText(path, presenter.GetHtml("normal"));
				this._normalChangeDescriptionRenderer.Navigate(path);
				path = Path.GetTempFileName();
				var contents = presenter.GetHtml("raw");
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
	}
}