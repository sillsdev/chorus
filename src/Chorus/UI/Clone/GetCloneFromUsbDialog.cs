using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Chorus.clone;
using System.Linq;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromUsbDialog : Form
	{
		private readonly string _parentDirectoryToPutCloneIn;
		private CloneFromUsb _model;
		private IProgress _progress;
		private enum State { LookingForUsb, FoundUsbButNoProjects, WaitingForUserSelection, MakingClone, Success, Error }

		private State _state;
		private string _failureMessage;

		public GetCloneFromUsbDialog(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
			Font = SystemFonts.MessageBoxFont;

			InitializeComponent();
			_model = new CloneFromUsb();
			UpdateDisplay(State.LookingForUsb);
			_progress = _logBox;
		}
		public CloneFromUsb Model { get { return _model; } }

		private void UpdateDisplay(State newState)
		{
			_state = newState;
			switch (_state)
			{
				case State.LookingForUsb:
					_statusLabel.Text = "Please insert a USB Flash Drive..." ;
					_statusImage.Visible   =true;
					_statusImage.ImageIndex  =0;
					_logBox.Visible = false;
					_okButton.Visible = false;
					listView1.Visible = false;
					break;
				case State.FoundUsbButNoProjects:
					_statusLabel.Left = listView1.Left;
					_statusImage.Visible = false;
					_statusLabel.Text = "No projects were found on the Usb Flash Drive." ;
					listView1.Visible = false;
					break;
				case State.WaitingForUserSelection:
					_statusLabel.Left = listView1.Left;
					listView1.Visible = true;
					_statusImage.Visible = false;
					_statusLabel.Text = "Select one of the following:";
					break;
				case State.MakingClone:
					_statusImage.Visible   =false;//we don't have an icond for this yet
					_copyToComputerButton.Visible = false;

					_statusLabel.Text = "Copying project";
					listView1.Visible = false;

					_logBox.Location = listView1.Location;
					_logBox.Bounds = listView1.Bounds;
					_logBox.Visible = true;
					//_progress.ShowVerbose = true;
					break;
				case State.Success:
					_statusLabel.Left = _statusImage.Right +10;
					_statusImage.Visible = true;
					_statusImage.ImageKey="Success";
					_statusLabel.Text = string.Format("Finished copying {0} to this computer at {1}", Path.GetFileName(SelectedPath), _parentDirectoryToPutCloneIn);
					_okButton.Visible = true;
					_cancelButton.Enabled = false;
					_logBox.Visible = false;
					break;
				case State.Error:
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.ImageKey = "Error";
					_statusImage.Visible = true;
					_statusLabel.Text = _failureMessage;
					_logBox.Visible = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_copyToComputerButton.Enabled = listView1.SelectedItems.Count == 1;
		}

		private void GetCloneFromUsbDialog_Load(object sender, EventArgs e)
		{
			if (_model.GetHaveOneOrMoreUsbDrives())
			{
				LoadChoices();
			}
			else
			{
				UpdateDisplay(State.LookingForUsb);
				_lookingForUsbTimer.Enabled = true;
			}

		}

		private void LoadChoices()
		{
			var paths = _model.GetDirectoriesWithMecurialRepos();
			if (paths.Count() == 0)
			{
				UpdateDisplay(State.FoundUsbButNoProjects);
				return;
			}
			foreach (string path in paths)
			{
				var item = new ListViewItem(System.IO.Path.GetFileName(path));
				item.Tag = path;
				var last = File.GetLastWriteTime(path);
				item.SubItems.Add(last.ToShortDateString() + " " + last.ToShortTimeString());
				item.ToolTipText = path;
				item.ImageIndex = 0;
				listView1.Items.Add(item);
			}
			UpdateDisplay(State.WaitingForUserSelection);
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay(State.WaitingForUserSelection);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   DialogResult = DialogResult.OK;
			Close();
		}

		protected string SelectedPath
		{
			get
			{
				if(listView1.SelectedItems == null)
					return null;
				if(listView1.SelectedItems.Count == 0)
					return null;
				return listView1.SelectedItems[0].Tag as string;
			}
		}

		public string PathToNewProject { get; private set; }

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void OnMakeCloneClick(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(SelectedPath))
				return;

			if (!Directory.Exists(_parentDirectoryToPutCloneIn))
			{
				MessageBox.Show(
					string.Format(
						@"Sorry, the calling program told Chorus to place the new project inside {0}, but that directory does not exist.",
						_parentDirectoryToPutCloneIn), "Problem", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			var target = Path.Combine(_parentDirectoryToPutCloneIn, Path.GetFileName(SelectedPath));
			if (Directory.Exists(target))
			{
				MessageBox.Show(string.Format(@"Sorry, a project with the same name already exists on this computer at the default location ({0}).
This tool is only for getting the project there in the first place, not for synchronizing with it.
If you want to use the version on the USB flash drive you will need to first delete, move, or rename the copy that is on your computer.",
_parentDirectoryToPutCloneIn), "Problem", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}
			try
			{
				UpdateDisplay(State.MakingClone);

				PathToNewProject = _model.MakeClone(SelectedPath, _parentDirectoryToPutCloneIn, _progress);

				UpdateDisplay(State.Success);


				using (SoundPlayer player = new SoundPlayer(Properties.Resources.finishedSound))
				{
					player.PlaySync();
				}

			}
			catch (Exception error)
			{
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.errorSound))
				{
					player.PlaySync();
				}
				_failureMessage = error.Message;
				UpdateDisplay(State.Error);
			}
		}

		private void _lookingForUsbTimer_Tick(object sender, EventArgs e)
		{
			if (_model.GetHaveOneOrMoreUsbDrives())
			{
				_lookingForUsbTimer.Enabled = false;
				LoadChoices();
			}
		}


	}
}
