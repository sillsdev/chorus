using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.Utilities;

namespace Chorus.UI.Misc
{
	public partial class LogBox : UserControl, IProgress
	{
		public LogBox()
		{
			InitializeComponent();
		}

		public void WriteStatus(string message, params object[] args)
		{
			WriteMessage(message, args);
		}

		public void WriteMessage(string message, params object[] args)
		{
			Write(Color.Black, message, args);
		}

		private void Write(Color color, string message, object[] args)
		{
//            try
//            {
				_box.Invoke(new Action(() =>
									  {
								_box.SelectionStart = _box.Text.Length;
								_box.SelectionColor = color;
								_box.AppendText(String.Format(message + Environment.NewLine, args));
									   }));

				_verboseBox.Invoke(new Action(() =>
				{
					_verboseBox.SelectionStart = _verboseBox.Text.Length;
					_verboseBox.SelectionColor = color;
					_verboseBox.AppendText(String.Format(message + Environment.NewLine, args));
				}));
//            }
//            catch (Exception)
//            {
//
//            }
		}

		public void WriteWarning(string message, params object[] args)
		{
			Write(Color.Blue, "Warning: " + message, args);
		}

		public void WriteError(string message, params object[] args)
		{
			Write(Color.Red,"Error:" + message, args);
		}

		public void WriteVerbose(string message, params object[] args)
		{
			_verboseBox.Invoke(new Action(() =>
			{
				_verboseBox.SelectionStart = _verboseBox.Text.Length;
				_verboseBox.SelectionColor = Color.DarkGray;
				_verboseBox.AppendText(String.Format(message + Environment.NewLine, args));
			}));
		}

		public bool ShowVerbose
		{
			set { _showDetailsMenu.Checked = value; }
		}

		public bool CancelRequested
		{
			get { return false; }
			set {  }
		}

		private void _showDetails_CheckedChanged(object sender, EventArgs e)
		{
			_verboseBox.Visible = _showDetailsMenu.Checked;
			_box.Visible = !_showDetailsMenu.Checked;

#if MONO  //mono (2.0?) doesn't update the size of the box when invisible, apparently
			if (_showDetailsMenu.Checked)
			{
				_verboseBox.Bounds = _box.Bounds;
			}
			else
			{
				_box.Bounds = _verboseBox.Bounds;
			}
#endif
		}

		private void _copyToClipboardLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
#if MONO
//at least on Xubuntu, getting some rtf on the clipboard would mean that when you pasted, you'd see rtf
			Clipboard.SetText(_verboseBox.Text);
#else
			Clipboard.SetText(_verboseBox.Rtf, TextDataFormat.Rtf);
#endif
		}

		public void Clear()
		{
			_box.Text = "";
			_verboseBox.Text = "";
		}



		private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_copyToClipboardLink_LinkClicked(sender, null);
		}

		private void LogBox_BackColorChanged(object sender, EventArgs e)
		{
			this._menu.BackColor =  this.BackColor;
		}

		private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}


	}
}
