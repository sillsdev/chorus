using System;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using OneStoryProjectEditor;

namespace Chorus.FileTypeHanders.oneStory
{
	public class OneStoryChangePresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;
		private readonly ProjectSettings _projSettings;
		private readonly TeamMembersData _teamMembers;

		public OneStoryChangePresenter(IXmlChangeReport report, ProjectSettings projSettings, TeamMembersData teamMembers)
		{
			_report = report;

			System.Diagnostics.Debug.Assert((projSettings != null) && (teamMembers != null));
			_projSettings = projSettings;
			_teamMembers = teamMembers;
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
			StoryData storyParent = new StoryData(_report.ParentNode);
			StoryData storyChild = new StoryData(_report.ChildNode);
			return storyParent.PresentationHtml(_projSettings, _teamMembers, storyChild);
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
	}
}