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
				return string.Format("<style type='text/css'><!-- {0} --></style>", File.ReadAllText(_path));
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
			foreach (var s in new string[]{"UI","Notes", "Html","StyleSheet.css"})
			{
				path = Path.Combine(path, s);
			}
			return new StyleSheet(path);
		}
	}
}