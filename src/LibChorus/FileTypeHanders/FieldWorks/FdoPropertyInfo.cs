using System;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Property information for an FDO property.
	/// </summary>
	public sealed class FdoPropertyInfo
	{
		/// <summary>
		/// Get the name opf the property.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Get the data type of the property.
		/// </summary>
		public DataType DataType { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, DataType dataType)
		{
			PropertyName = propertyName;
			DataType = dataType;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FdoPropertyInfo(string propertyName, string dataType)
			: this(propertyName, (DataType)Enum.Parse(typeof(DataType), dataType))
		{
		}
	}
}