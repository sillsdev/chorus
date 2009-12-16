using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Chorus.UI
{
	public class StyleSheet
	{
		private string _path;

		public StyleSheet(string pathToCss)
		{
			_path = pathToCss;
		}

		public string TextForInsertingIntoHmtlHeadElement
		{
			get
			{
				var contents = GetContents();
				return string.Format("<style type='text/css'><!-- {0} --></style>", contents);
			}
		}

		private string GetContents()
		{
			if (File.Exists(_path))
			{
				return File.ReadAllText(_path);
			}
			else   //TODO this is a temp hack
			{
				return
					@"body
{
margin-top: 0px;
 font-family: verdana,arial,helvetica,sans-serif; font-size: 12px;
}
hr {border: 1px solid #DCDCDC}

span.sender {color: Black; font-weight: bold}
span.when {color:Gray}
span.status {color: Black; font-weight: bold}
#span.statusChangeNotice {font-style: italic}

div.messageContents{margin-top:10px}
div.message{margin-top:9px; font-size:larger}
div.message.statusChange{font-style: italic; margin-top:6px}

div.selected {background-color: #FFFACD}


";
			}
		}

		public static string DirectoryOfTheApplicationExecutable
		{
			get
			{
				string path;
				bool unitTesting = Assembly.GetEntryAssembly() == null;
				if (unitTesting)
				{
					path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
					path = Uri.UnescapeDataString(path);
				}
				else
				{
					//was suspect in WS1156, where it seemed to start looking in the,
					//outlook express program folder after sending an email from wesay...
					//so maybe it doesn't always mean *this* executing assembly?
					//  path = Assembly.GetExecutingAssembly().Location;
					path = Application.ExecutablePath;
				}
				return Directory.GetParent(path).FullName;
			}
		}

		public static StyleSheet CreateFromDisk()
		{
			string path = DirectoryOfTheApplicationExecutable;
			//TODO: this only makes sense in the chorus dev environment?
			foreach (var s in new string[]{"UI","Notes", "Html","StyleSheet.css"})
			{
				path = Path.Combine(path, s);
			}
			return new StyleSheet(path);
		}
	}
}