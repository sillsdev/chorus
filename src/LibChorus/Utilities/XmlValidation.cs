using System;
using System.Xml;
using Palaso.Progress;
using Palaso.Xml;

namespace Chorus.Utilities
{
	public static class XmlValidation
	{
		/// <summary>
		/// This simply checks that the file is well-formed
		/// </summary>
		public static string ValidateFile(string pathToFile, IProgress progress)
		{
			XmlReader reader = null;
			try
			{
				reader = XmlReader.Create(pathToFile, CanonicalXmlSettings.CreateXmlReaderSettings());
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
