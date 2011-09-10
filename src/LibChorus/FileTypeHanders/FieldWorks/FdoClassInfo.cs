using System.Collections.Generic;
using System.Linq;

namespace Chorus.FileTypeHanders.FieldWorks
{
	///<summary>
	/// Class that holds some basic information about FDO classes.
	///</summary>
	public sealed class FdoClassInfo
	{
		/// <summary>
		/// Get the class name.
		/// </summary>
		public string ClassName { get; private set; }
		/// <summary>
		/// Check if class is abstract.
		/// </summary>
		public bool IsAbstract { get; private set; }
		private readonly List<FdoPropertyInfo> _properties = new List<FdoPropertyInfo>();

		internal FdoClassInfo(string className, string superclassName)
			: this(className, false, superclassName)
		{
		}

		internal FdoClassInfo(string className, bool isAbstract, string superclassName)
		{
			ClassName = className;
			IsAbstract = isAbstract;
			SuperclassName = superclassName;
		}

		internal void AddProperty(FdoPropertyInfo propertyinfo)
		{
			_properties.Add(propertyinfo);
		}

		/// <summary>
		/// Get a set of zero or more properties for the class.
		/// </summary>
		public IEnumerable<FdoPropertyInfo> AllProperties
		{
			get
			{
				var results = new List<FdoPropertyInfo>();

				if (Superclass != null)
					results.AddRange(Superclass.AllProperties);

				if (_properties.Count > 0)
					results.AddRange(_properties);

				return results;
			}
		}

		///<summary>
		/// Get a set of zero or more collection properties (reference or owning).
		///</summary>
		public IEnumerable<FdoPropertyInfo> AllCollectionProperties
		{
			get
			{
				return new List<FdoPropertyInfo>(from prop in AllProperties
														where prop.DataType == DataType.OwningCollection || prop.DataType == DataType.ReferenceCollection
														select prop);
			}
		}

		/// <summary>
		/// Get the superclass name.
		/// </summary>
		public string SuperclassName { get; internal set; }

		/// <summary>
		/// Get the superclass.
		/// </summary>
		public FdoClassInfo Superclass { get; internal set; }
	}
}