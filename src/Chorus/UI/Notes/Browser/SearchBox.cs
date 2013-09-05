using System;
using System.Windows.Forms;

namespace Chorus.UI.Notes.Browser
{
	public partial class SearchBox : UserControl
	{
		public event EventHandler SearchTextChanged;

		public SearchBox()
		{
			InitializeComponent();
		}

		public string SearchText
		{
			get { return _searchText.Text; }
			set { _searchText.Text = value; }
		}

		private void _searchText_TextChanged(object sender, EventArgs e)
		{
			if(SearchTextChanged !=null)
				SearchTextChanged.Invoke(this._searchText.Text, new EventArgs());
		}
	}
}