using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Palaso.Progress.LogBox;

namespace Chorus.Utilities
{
	public static class XmlValidation
	{
		/// <summary>
		/// This simply checks that the file is well-formed
		/// </summary>
		public static string ValidateFile(string pathToFile, IProgress progress)
		{
			XmlReaderSettings settings = new XmlReaderSettings { ValidationType = ValidationType.None };
			XmlReader reader = null;
			try
			{
				reader = XmlReader.Create(pathToFile, settings);
				while (reader.Read())
				{
				}
			}
			catch (Exception error)
			{
				return error.Message;
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
			return null;
		}

	}
}
