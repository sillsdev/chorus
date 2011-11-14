using System;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Property information for an FDO property.
	/// </summary>
	public sealed class FdoPropertyInfo
	{
		/// <summary>
		/// Get the name of the property.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Get the data type of the property.
		/// </summary>
		public DataType DataType { get; private set; }

		/// <summary>
		/// See if the property is custom or standard.
		/// </summary>
		public bool IsCustomProperty { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, DataType dataType)
			: this(propertyName, dataType, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, DataType dataType, bool isCustomProperty)
		{
			PropertyName = propertyName;
			DataType = dataType;
			IsCustomProperty = isCustomProperty;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, string dataType, bool isCustomProperty)
			: this(propertyName, (DataType)Enum.Parse(typeof(DataType), dataType), isCustomProperty)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, string dataType)
			: this(propertyName, (DataType)Enum.Parse(typeof(DataType), dataType), false)
		{
		}
	}
}