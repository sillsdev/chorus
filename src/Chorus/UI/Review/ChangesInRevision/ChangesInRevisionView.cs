using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge;

namespace Chorus.UI.Review.ChangesInRevision
{
	public partial class ChangesInRevisionView : UserControl
	{
		private readonly ChangesInRevisionModel _model;

		public ChangesInRevisionView(ChangesInRevisionModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			InitializeComponent();
			_model = model;
			_model.UpdateDisplay += OnUpdateDisplay;
		}

		void OnUpdateDisplay(object sender, EventArgs e)
		{
			var items = new List<ListViewItem>();
			listView1.Items.Clear();
			if (_model.ChangeReports != null)
			{
				foreach (var report in _model.ChangeReports)
				{
					IChangePresenter presenter = _model.GetChangePresenterForDataType(report);
					var row = new ListViewItem(new string[] {presenter.GetTypeLabel(), presenter.GetDataLabel(), presenter.GetActionLabel()});
					row.Tag = report;
					row.ImageKey = presenter.GetIconName();
					items.Add(row);
				}

				listView1.Items.AddRange(items.ToArray());
				if (items.Count > 0)
				{   //select the first one if there is one
					listView1.Items[0].Selected = true;
				}
				else
				{   //nothing to show, cuase the detail pain to clear also
					_model.SelectedChangeChanged(null);
				}
			}
		}


		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			_model.SelectedChangeChanged(CurrentRecord);
		}

		protected IChangeReport CurrentRecord
		{
			get
			{
				if (listView1.SelectedItems.Count == 1)
				{
					return listView1.SelectedItems[0].Tag as IChangeReport;
				}
				else
				{
					return null;
				}
			}
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if(CurrentRecord == null)
				return;

			if(string.IsNullOrEmpty(CurrentRecord.UrlOfItem))
				return;

			_model.NavigationRequested(CurrentRecord.UrlOfItem);
		}
	}
}