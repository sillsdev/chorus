using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Chorus;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using SIL.IO;
using SampleApp.Properties;

namespace SampleApp
{
	/// <summary>
	/// This form lets you pretend to be one of two users.  You can edit some data, Send/Receive the data to
	/// a central repository which lives in a folder in your temp directory. Switching users moves you to
	/// a repository belonging to just that user.  In this way, we can simulate collaboration.
	///
	/// ChangeSimulatedUser() contains code that you'll want in your application (adding the notes browser and history controls).
	/// OnSendReceiveClick() shows a typical Send/Receive action.
	/// The rest of the code here is just setting up the test.  Look in the DataEditor class for more useful example code.
	/// </summary>
	public partial class Form1 : Form
	{
		private readonly string _testDirectory;
		private ChorusSystem _chorusSystem;
		private readonly string[] _userNames = new string[]{"Bob","Sue"};
		private DataEditor _dataEditor;
		private NotesBrowserPage _notesBrowserControl;
		private HistoryPage _historyControl;
		private RepositoryAddress _serverRepository;

		public Form1(string testDirectory)
		{
			_testDirectory = testDirectory;
			InitializeComponent();

			var serverDir = Path.Combine(testDirectory, "server");
			UnZipToDirectory(serverDir);
			_serverRepository = new DirectoryRepositorySource("server", Path.Combine(serverDir,"ShoppingList"), false);
			_userPicker.SelectedIndex = 0;
		}

		private void ChangeSimulatedUser(string userName)
		{
			ClearOutInAnticipationOfSwitchingUsers();

			 var dir = Path.Combine(_testDirectory, userName);
			 if (!Directory.Exists(dir))
			 {
				 UnZipToDirectory(dir);
			 }

			var shoppingListDir = Path.Combine(dir, "ShoppingList");

			//note: if you don't have a user name, you can just let chorus try to figure one out.
			//Also note that this is not the same name as that used for any given network repository credentials;
			//Rather, it's the name which will show in the history, and besides Notes that this user makes.
			_chorusSystem = new ChorusSystem(shoppingListDir);
			_chorusSystem.DisplaySettings = new ChorusNotesDisplaySettings()
			{
				WritingSystemForNoteLabel = new TestWritingSystem("Algerian"),
				WritingSystemForNoteContent = new TestWritingSystem("Bradley Hand ITC")
			};

			_chorusSystem.Init(userName);


			_chorusSystem.Repository.SetKnownRepositoryAddresses(new RepositoryAddress[] {_serverRepository});

			_chorusSystem.ProjectFolderConfiguration.IncludePatterns.Add("*.xml");

			_dataEditor = new DataEditor(_chorusSystem, Path.Combine(shoppingListDir, "shopping.xml"));
			_dataEditor.Dock = DockStyle.Fill;
			_frontPage.Controls.Add(_dataEditor);

			_notesBrowserControl = _chorusSystem.WinForms.CreateNotesBrowser();
			_notesBrowserControl.Dock = DockStyle.Fill;
			_notesPage.Controls.Add(_notesBrowserControl);

			_historyControl = _chorusSystem.WinForms.CreateHistoryPage();
			_historyControl.Dock = DockStyle.Fill;
			_historyPage.Controls.Add(_historyControl);
		}

		private void UnZipToDirectory(string dir)
		{
			using (var tempZipFile = new TempFile())
			{
				File.WriteAllBytes(tempZipFile.Path, Resources.ShoppingList);
				FastZip zip = new FastZip();
				Directory.CreateDirectory(dir);
				zip.ExtractZip(tempZipFile.Path, dir, null);
			}
		}

		private void ClearOutInAnticipationOfSwitchingUsers()
		{
			if(_dataEditor !=null)
			{
				_frontPage.Controls.Remove(_dataEditor);
				_dataEditor.Dispose();
			}
			if (_notesBrowserControl != null)
			{
				_notesPage.Controls.Remove(_notesBrowserControl);
				_notesBrowserControl.Dispose();
			}
			if (_historyControl != null)
			{
				_historyPage.Controls.Remove(_historyControl);
				_historyControl.Dispose();
			}
			if (_chorusSystem != null)
			{
				_chorusSystem.Dispose();
			}
		}

		private void _userPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(_userPicker.SelectedIndex>-1)
				ChangeSimulatedUser(_userNames[_userPicker.SelectedIndex]);
		}

		private void _viewTestDataDirectory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(_testDirectory);
		}

		private void OnSendReceiveClick(object sender, EventArgs e)
		{
			_dataEditor.SaveNow();
			using (var dlg = _chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				if(DialogResult.OK == dlg.ShowDialog(this))
				{
					//we do this because some components (notably the notes browser as of Oct 2010) do not yet
					//support loading in fresh data which may have come from a merge.
					ChangeSimulatedUser(_userNames[_userPicker.SelectedIndex]);
				}
			}
		}


		private void button1_Click(object sender, EventArgs e)
		{
			_dataEditor.SaveNow();
			_chorusSystem.AsyncLocalCheckIn("background checkin", null);
		}

		internal class TestWritingSystem : IWritingSystem
		{
			private readonly string _fontName;

			public TestWritingSystem(string fontName)
			{
				_fontName = fontName;
			}

			public string Name
			{
				get { return "test"; }
			}

			public string Code
			{
				get { return "tst"; }
			}

			public string FontName
			{
				get { return _fontName; }
			}

			public int FontSize
			{
				get { return 24; }
			}

			public void ActivateKeyboard()
			{

			}
		}
	}
}
