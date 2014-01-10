using System;
using System.Diagnostics;

namespace ChorusHubApp
{
	/// <summary>
	/// Allows control of command line parsing.
	/// Attach this attribute to instance fields of types used
	/// as the destination of command line argument parsing.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ArgumentAttribute : Attribute
	{
		/// <summary>
		/// Allows control of command line parsing.
		/// </summary>
		/// <param name="type"> Specifies the error checking to be done on the argument. </param>
		public ArgumentAttribute(ArgumentTypes type)
		{
			this.type = type;
		}

		/// <summary>
		/// The error checking to be done on the argument.
		/// </summary>
		public ArgumentTypes Type
		{
			get { return type; }
		}

		/// <summary>
		/// Returns true if the argument did not have an explicit short name specified.
		/// </summary>
		public bool DefaultShortName
		{
			get { return null == shortName; }
		}

		/// <summary>
		/// The short name of the argument.
		/// Set to null means use the default short name if it does not
		/// conflict with any other parameter name.
		/// Set to String.Empty for no short name.
		/// This property should not be set for DefaultArgumentAttributes.
		/// </summary>
		public string ShortName
		{
			get { return shortName; }
			set
			{
				shortName = value;
			}
		}

		/// <summary>
		/// Returns true if the argument did not have an explicit long name specified.
		/// </summary>
		public bool DefaultLongName
		{
			get { return null == longName; }
		}

		/// <summary>
		/// The long name of the argument.
		/// Set to null means use the default long name.
		/// The long name for every argument must be unique.
		/// It is an error to specify a long name of String.Empty.
		/// </summary>
		public string LongName
		{
			get
			{
				Debug.Assert(!DefaultLongName);
				return longName;
			}
			set
			{
				Debug.Assert(value.Length > 0);
				longName = value;
			}
		}

		/// <summary>
		/// The default value of the argument.
		/// </summary>
		public object DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		/// <summary>
		/// Returns true if the argument has a default value.
		/// </summary>
		public bool HasDefaultValue
		{
			get { return null != defaultValue; }
		}

		/// <summary>
		/// Returns true if the argument has help text specified.
		/// </summary>
		public bool HasHelpText
		{
			get { return null != helpText; }
		}

		/// <summary>
		/// The help text for the argument.
		/// </summary>
		public string HelpText
		{
			get { return helpText; }
			set { helpText = value; }
		}

		private string shortName;
		private string longName;
		private string helpText;
		private object defaultValue;
		private readonly ArgumentTypes type;
	}
}