using System;
using System.IO;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	public class FieldWorksMerger
	{
		private readonly IMergeStrategy m_mergeStrategy;
		private string m_winnerXml;
		private string m_loserXml;
		private string m_commonAncestorXml;

		public FieldWorksMerger(IMergeStrategy mergeStrategy, string pathToWinner, string pathToLoser, string pathToCommonAncestor)
		{
			m_mergeStrategy = mergeStrategy;

			m_winnerXml = File.ReadAllText(pathToWinner);
			m_loserXml = File.ReadAllText(pathToLoser);
			m_commonAncestorXml = File.ReadAllText(pathToCommonAncestor);
		}

		/// <summary>Used by tests, which prefer to give us raw contents rather than paths</summary>
		public FieldWorksMerger(string winnerXml, string loserXml, string commonAncestorXml, IMergeStrategy mergeStrategy)
		{
			m_winnerXml = winnerXml;
			m_loserXml = loserXml;
			m_commonAncestorXml = commonAncestorXml;
			m_mergeStrategy = mergeStrategy;
		}

		public string GetMergedContents()
		{
			throw new NotImplementedException();
		}

		public IMergeEventListener EventListener
		{ get; set; }
	}
}