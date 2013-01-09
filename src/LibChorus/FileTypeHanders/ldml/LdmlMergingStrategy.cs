using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			throw new NotImplementedException();
		}

		public HashSet<string> SuppressIndentingChildren()
		{
			return new HashSet<string>();
		}

		#endregion
	}
}
