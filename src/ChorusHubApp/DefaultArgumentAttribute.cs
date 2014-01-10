using System;

namespace ChorusHubApp
{
	/// <summary>
	/// Indicates that this argument is the default argument.
	/// '/' or '-' prefix only the argument value is specified.
	/// The ShortName property should not be set for DefaultArgumentAttribute
	/// instances. The LongName property is used for usage text only and
	/// does not affect the usage of the argument.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DefaultArgumentAttribute: ArgumentAttribute
	{
		/// <summary>
		/// Indicates that this argument is the default argument.
		/// </summary>
		/// <param name="type"> Specifies the error checking to be done on the argument. </param>
		public DefaultArgumentAttribute(ArgumentTypes type): base(type) {}
	}
}