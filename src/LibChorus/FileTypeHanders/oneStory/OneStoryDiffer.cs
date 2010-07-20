using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using OneStoryProjectEditor;


namespace Chorus.FileTypeHanders.oneStory
{
	public class OneStoryDiffer
	{
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _childDom;
		private readonly XmlDocument _parentDom;
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private readonly string _strProjectFolder;
		private IMergeEventListener EventListener;

		public static OneStoryDiffer CreateFromFiles(FileInRevision parent, FileInRevision child,
			string strProjectFolder, string ancestorOneStoryProjectFilePath, string ourOneStoryProjectFilePath,
			IMergeEventListener eventListener)
		{
			return new OneStoryDiffer(parent, child, strProjectFolder,
				File.ReadAllText(ancestorOneStoryProjectFilePath),
				File.ReadAllText(ourOneStoryProjectFilePath), eventListener);
		}

		private OneStoryDiffer(FileInRevision parentFileInRevision, FileInRevision childFileInRevision,
			string strProjectFolder, string parentXml, string childXml, IMergeEventListener eventListener)
		{
			_childDom = new XmlDocument();
			_parentDom = new XmlDocument();

			_childDom.LoadXml(childXml);
			_parentDom.LoadXml(parentXml);

			_parentFileInRevision = parentFileInRevision;
			_childFileInRevision = childFileInRevision;
			_strProjectFolder = strProjectFolder;
			EventListener = eventListener;
		}

		public void ReportDifferencesToListener(out ProjectSettings projSettings, out TeamMembersData teamMembers)
		{
			XmlNode eStoryProject = _childDom.SelectSingleNode("/StoryProject");
			projSettings = new ProjectSettings(eStoryProject, _strProjectFolder);
			teamMembers = new TeamMembersData(eStoryProject);
			foreach (XmlNode e in _childDom.SafeSelectNodes("/StoryProject/stories[@SetName = 'Stories']/story"))
			{
				ProcessEntry(projSettings.ProjectName, e);
			}
		}

		private string[] _astrXPath = new [] { "/", "/verses/verse[@first = 'true']" };
		private string[] _astrAttributeToIgnore = new [] { StoryData.CstrAttributeTimeStamp, null };

		/// <summary>
		/// Process the differences for a particular story
		/// </summary>
		/// <param name="child">this node represents a 'story' element in the StoryProject/stories[SetName='Stories'] set</param>
		private void ProcessEntry(string strProjectName, XmlNode child)
		{
			string id = GetGuid(child);
			XmlNode parent = FindMatch(_parentDom, id);
			string url = GetUrl(strProjectName, child);
			if (parent == null) //it's new
			{
				EventListener.ChangeOccurred(new XmlAdditionChangeReport(_childFileInRevision, child, url));
			}
			else if (XmlUtilities.AreXmlElementsEqual(child.OuterXml, parent.OuterXml, _astrXPath, _astrAttributeToIgnore))
			{
				//unchanged or both made same change
			}
			else //one or both changed
			{
				EventListener.ChangeOccurred(new XmlChangedRecordReport(_parentFileInRevision, _childFileInRevision, parent, child, url));
			}

			_processedIds.Add(id);
		}

		public static string GetGuid(XmlNode e)
		{
			return e.Attributes["guid"].Value;
		}

		public static string GetUrl(string strProjectName, XmlNode e)
		{
			// OneStory URL syntax
			// onestory://<projectname>?StoryId=<storyguid>...
			// where:
			//  projectname = e.g. snwmtn-test
			//  StoryId = story[@guid]
			string strStoryGuid = e.Attributes["guid"].Value;
			return String.Format("onestory://{0}?StoryId={1}",
				strProjectName,
				strStoryGuid);
		}

		public static XmlNode FindMatch(XmlNode doc, string guid)
		{
			return doc.SelectSingleNode("/StoryProject/stories[@SetName = 'Stories']/story[@guid='" + guid + "']");
		}
	}
}