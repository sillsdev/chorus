using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;

namespace Chorus.FileTypeHanders.oneStory
{
	public class OneStoryChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;
		private readonly XmlNode _projFile;
		private readonly string _strProjectPath;

		public OneStoryChangePresenter(IXmlChangeReport report, XmlNode projFile, string strProjectPath)
		{
			_report = report;

			System.Diagnostics.Debug.Assert(_projFile != null);
			_projFile = projFile;
			_strProjectPath = strProjectPath;
		}

		public string GetActionLabel()
		{
			return ((IChangeReport)_report).ActionLabel;
		}

		public string GetDataLabel()
		{
			string strStoryName = FirstNonNullNode.Attributes["name"].Value;
			if (String.IsNullOrEmpty(strStoryName))
			{
				//if the child was marked as deleted, we actually need to look to the parent node
				if (_report.ParentNode != null)
				{
					strStoryName = _report.ParentNode.Attributes["name"].Value;
				}
			}
			if (String.IsNullOrEmpty(strStoryName))
			{
				return "??";
			}

			return strStoryName;
		}

		public string GetTypeLabel()
		{
			if (FirstNonNullNode.Name == "story")
				return "story";

			return "?";
		}

		public string GetIconName()
		{
			return "onestory";
		}


		public string GetHtml(string style, string styleSheet)
		{
			try
			{
				var method = RetrieveRemoteMethod("OneStoryProjectEditor.StoryData", "GetPresentationHtmlForChorus");
				return (string)method.Invoke(null, new object[] { _projFile, _strProjectPath, _report.ParentNode, _report.ChildNode });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}

		private XmlNode FirstNonNullNode
		{
			get
			{
				if (_report.ChildNode == null)
					return _report.ParentNode;
				return _report.ChildNode;
			}
		}

		static MethodInfo RetrieveRemoteMethod(string strRemoteClass, string remoteMethodName)
		{
			var ourWordPath = Path.Combine(
				ExecutionEnvironment.DirectoryOfExecutingAssembly, OneStoryFileHandler.CstrAppName);
			var ourWordAssembly = Assembly.LoadFrom(ourWordPath);

			var mergerType = ourWordAssembly.GetType(strRemoteClass);

			return mergerType.GetMethod(remoteMethodName);
		}
	}
}