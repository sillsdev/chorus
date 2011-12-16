using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.ldml
{
	///<summary>
	/// Implementation of IMergeStrategy that handles some special merge requirements.
	///</summary>
	public sealed class LdmlMergingStrategy : IMergeStrategy
	{
		#region Implementation of IMergeStrategy

		///<summary>
		/// Handle some special merge requirements.
		///</summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
