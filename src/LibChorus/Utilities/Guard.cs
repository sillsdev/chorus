using System;
using System.IO;

//two of these methods originally from RhinoCommons

namespace Chorus.Utilities
{
	/// <summary>
	/// Helper class for guard statements, which allow prettier
	/// code for guard clauses
	/// </summary>
	public static class Guard
	{
		/// <summary>
		/// Will throw a <see cref="InvalidOperationException"/> if the assertion
		/// is true, with the specificied message.
		/// </summary>
		/// <param name="assertion">if set to <c>true</c> [assertion].</param>
		/// <param name="message">The message.</param>
		/// <example>
		/// Sample usage:
		/// <code>
		/// Guard.Against(string.IsNullOrEmpty(name), "Name must have a value");
		/// </code>
		/// </example>
		public static void Against(bool assertion, string message)
		{
			if (assertion == false)
				return;
			throw new InvalidOperationException(message);
		}


		public static void AgainstNull(object value, string valueName)
		{
			if (value == null)
				throw new ArgumentNullException(valueName);
		}

		/// <summary>
		/// Will throw exception of type <typeparamref name="TException"/>
		/// with the specified message if the assertion is true
		/// </summary>
		/// <typeparam name="TException"></typeparam>
		/// <param name="assertion">if set to <c>true</c> [assertion].</param>
		/// <param name="message">The message.</param>
		/// <example>
		/// Sample usage:
		/// <code>
		/// <![CDATA[
		/// Guard.Against<ArgumentException>(string.IsNullOrEmpty(name), "Name must have a value");
		/// ]]>
		/// </code>
		/// </example>
		public static void Against<TException>(bool assertion, string message) where TException : Exception
		{
			if (assertion == false)
				return;
			throw (TException)Activator.CreateInstance(typeof(TException), message);
		}
	}
	public static class Require
	{

		public static void That(bool assertion, string message)
		{
			if (assertion)
				return;
			throw new InvalidOperationException(message);
		}
	}
}