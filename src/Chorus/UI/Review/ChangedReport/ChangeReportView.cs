using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Forms;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.retrieval;
using Chorus.VcsDrivers.Mercurial;
using SIL.PlatformUtilities;

namespace Chorus.UI.Review.ChangedReport
{
	public partial class ChangeReportView : UserControl
	{
		private readonly ChorusFileTypeHandlerCollection _handlers;
		private readonly HgRepository _repository;
		private string _styleSheet;

		public ChangeReportView(ChorusFileTypeHandlerCollection handlers, ChangedRecordSelectedEvent changedRecordSelectedEvent, HgRepository repository, IEnumerable<IWritingSystem> writingSystems)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_handlers = handlers;
			_repository = repository;
			InitializeComponent();
			_normalChangeDescriptionRenderer.Font = SystemFonts.MessageBoxFont;
			changedRecordSelectedEvent.Subscribe(r=>LoadReport(r));
			if (Platform.IsWindows)
				_normalChangeDescriptionRenderer.Navigated += webBrowser1_Navigated;
			_styleSheet = CreateStyleSheet(writingSystems);
		}

		private string CreateStyleSheet(IEnumerable<IWritingSystem> writingSystems)
		{
			StringBuilder styleSheetBuilder = new StringBuilder();

			styleSheetBuilder.AppendLine("<style type='text/css'>");
			styleSheetBuilder.AppendLine("<!--");
			styleSheetBuilder.AppendLine("BODY { font-family: verdana,arial,helvetica,sans-serif; font-size: 12px;}");
			styleSheetBuilder.AppendLine("span.langid {color: gray; font-size: xx-small;position: relative;top: 0.3em;}");
			styleSheetBuilder.AppendLine("span.fieldLabel {color: gray; font-size: x-small;}");
			styleSheetBuilder.AppendLine("div.entry {color: blue;font-size: x-small;}");
			styleSheetBuilder.AppendLine("td {font-size: x-small;}");
			foreach (IWritingSystem ws in writingSystems)
			{
				string size = (ws.FontSize > 12) ? "large" : "small";
				styleSheetBuilder.AppendLine(String.Format("span.{0} {{font-family: \"{1}\";font-size: \"{2}\"}}", ws.Code, ws.FontName, size));
			}
			styleSheetBuilder.AppendLine("span.en {color: green;}");
			styleSheetBuilder.AppendLine("span.es {color: green;}");
			styleSheetBuilder.AppendLine("span.fr {color: green;}");
			styleSheetBuilder.AppendLine("span.tpi {color: purple;}");

			styleSheetBuilder.AppendLine("-->");

			styleSheetBuilder.AppendLine("</style>");

			return styleSheetBuilder.ToString();
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
				if (Platform.IsWindows)
				{
					// GECKOFX blank url does not blank page
					_normalChangeDescriptionRenderer.Navigate(string.Empty);
				}
			}
			else
			{
				var presenter = _handlers.GetHandlerForPresentation(report.PathToFile).GetChangePresenter(report, _repository);
				var path = Path.GetTempFileName();
				File.WriteAllText(path, presenter.GetHtml("normal", _styleSheet));
				try
				{
					this._normalChangeDescriptionRenderer.Navigate(path);
				}
				catch (InvalidOperationException)
				{
					System.Console.WriteLine("_normalChangeDescriptionRenderer not ready");
				}
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
					{
						System.Console.WriteLine("adding raw tab back");
						this.tabControl1.TabPages.Add(tabPageRaw);
					}
					File.WriteAllText(path, contents);
					try
					{
						this._rawChangeDescriptionRenderer.Navigate(path);
					}
					catch (InvalidOperationException)
					{
						System.Console.WriteLine("_rawChangeDescriptionRenderer not ready");
					}
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
				player.PlaySync();
			}
		}
	}
}
