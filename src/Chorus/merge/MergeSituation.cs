using System;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge
{
	/// <summary>
	/// for unit tests that don't need this
	/// </summary>
	public class NullMergeSituation: MergeSituation
	{
		public NullMergeSituation()
			: base(null, null, null, null, null)
		{
		}
	}
	/// <summary>
	/// The context for the conflict that occurred.
	/// This information will be useful to help the user later determined exactly what happened.
	/// </summary>
	public class MergeSituation
	{
		//we don't have access to this yet: public const string kAncestorRevision = "ChorusAncestorRevision";
		public const string kUserXId= "ChorusUserXId";
		public const string kUserYId = "ChorusUserYId";
		public const string kUserXRevision = "ChorusUserXRevision";
		public const string kUserYRevision = "ChorusUserYRevision";
		//public const string kPathToFileInRepository = "ChorusPathToFileInRepository";

		/// <summary>
		/// A relative path
		/// </summary>
		public string PathToFileInRepository{ get; set;}
	   //we don't have access to this yet: public string AncestorRevision { get; set; }
		public string UserXId { get; set; }
		public string UserYId { get; set; }
		public string UserXRevision { get; set; }
		public string UserYRevision { get; set; }


		public void WriteAsXml(XmlWriter writer)
		{
			writer.WriteStartElement("MergeSituation");
			writer.WriteAttributeString("userXId", UserXId);
			writer.WriteAttributeString("userYId", UserYId);
			writer.WriteAttributeString("userXRevision", UserXRevision);
			writer.WriteAttributeString("userYRevision", UserYRevision);
			writer.WriteAttributeString("path", PathToFileInRepository);
			writer.WriteEndElement();
		}

		public MergeSituation(string relativePathToFile, string userXId, string userXRevision, string userYId, string userYRevision/*, string ancestorRevision*/)
		{
			PathToFileInRepository = relativePathToFile;
			UserXId = userXId;
			UserYId = userYId;
			UserYRevision = userYRevision;
			UserXRevision = userXRevision;
			//we don't have access to this yet:   AncestorRevision = ancestorRevision;
		}

		public static MergeSituation CreateFromEnvironmentVariables(string pathToFileInRepository)
		{
			//NB: these aren't needed to do the merge; we're given the actual 3 files.  But they are needed
			//for the conflict record, so that we can later look up exactly what were the 3 inputs at the time of merging.
		   //string pathToFileInRepository = Environment.GetEnvironmentVariable(MergeSituation.kPathToFileInRepository);
			//we don't have access to this yet: string ancestorRevision = Environment.GetEnvironmentVariable(MergeSituation.kAncestorRevision);
			string userXId = Environment.GetEnvironmentVariable(MergeSituation.kUserXId);
			string userYId = Environment.GetEnvironmentVariable(MergeSituation.kUserYId);
			string userXRevision = Environment.GetEnvironmentVariable(MergeSituation.kUserXRevision);
			string userYRevision = Environment.GetEnvironmentVariable(MergeSituation.kUserYRevision);

			return new MergeSituation( pathToFileInRepository, userXId, userXRevision, userYId,
									  userYRevision/*, ancestorRevision*/);

		}

		/// <summary>
		/// this isn't all we want to know, but it's all the guy telling hg to merge two branches knows.
		/// This is called to put this info into env vars where we can retrieve it once chorus.exe is called by hg
		/// </summary>
		/// <param name="userXRevision"></param>
		/// <param name="userYRevision"></param>
		public static void PushRevisionsToEnvironmentVariables(string userXId, string userXRevision, string userYId, string userYRevision)
		{
			Environment.SetEnvironmentVariable(kUserXId, userXId);
			Environment.SetEnvironmentVariable(kUserXRevision, userXRevision);

			Environment.SetEnvironmentVariable(kUserYId, userYId);
			Environment.SetEnvironmentVariable(kUserYRevision, userYRevision);
		}
//
//        public void WriteXml(XmlWriter writer)
//        {
//            writer.WriteStartElement("MergeSituation");
//            writer.WriteAttributeString("PathToFileInRepository",this.PathToFileInRepository);
//            writer.WriteEndElement();
//        }



	}
}