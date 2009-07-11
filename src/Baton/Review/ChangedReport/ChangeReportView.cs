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
			_handlers = handlers;
			InitializeComponent();
			changedRecordSelectedEvent.Subscribe(r=>Load(r));
			_changeDescriptionRenderer.Navigated += webBrowser1_Navigated;
		}

		private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			//didn't work, 'cuase is't actually still being held by the browser
			//  File.Delete(e.URI.AbsoluteUri.Replace(@"file:///", string.Empty));
		}

		public void Load(IChangeReport report)
		{
			if (report == null)
			{
			   _changeDescriptionRenderer.Navigate(string.Empty);
			}
			else
			{
				var presenter = _handlers.GetHandlerForPresentation(report.PathToFile).GetChangePresenter(report);
				var path = Path.GetTempFileName();
				File.WriteAllText(path, presenter.GetHtml());
				this._changeDescriptionRenderer.Navigate(path);
			}
		}
	}
}