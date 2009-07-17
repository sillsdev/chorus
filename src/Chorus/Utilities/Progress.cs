using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Chorus.Utilities
{
	public interface IProgress
	{
		void WriteStatus(string message, params object[] args);
		void WriteMessage(string message, params object[] args);
		void WriteWarning(string message, params object[] args);
		void WriteError(string message, params object[] args);
	}

	public class NullProgress : IProgress
	{
		public void WriteStatus(string message, params object[] args)
		{

		}

		public void WriteMessage(string message, params object[] args)
		{
		}

		public void WriteWarning(string message, params object[] args)
		{
		}

		public void WriteError(string message, params object[] args)
		{

		}
	}

	public class MultiProgress : IProgress, IDisposable
	{
		private readonly IEnumerable<IProgress> _progressHandlers;

		public MultiProgress(IEnumerable<IProgress> progressHandlers)
		{
			_progressHandlers = progressHandlers;
		}

		public void WriteStatus(string message, params object[] args)
		{
			foreach (var handler in _progressHandlers)
			{
				handler.WriteStatus(message, args);
			}
		}

		public void WriteMessage(string message, params object[] args)
		{
			foreach (var handler in _progressHandlers)
			{
				handler.WriteMessage(message, args);
			}
		}

		public void WriteWarning(string message, params object[] args)
		{
			foreach (var handler in _progressHandlers)
			{
				handler.WriteWarning(message, args);
			}
		}

		public void WriteError(string message, params object[] args)
		{
			foreach (var handler in _progressHandlers)
			{
				handler.WriteError(message, args);
			}
		}

		public void Dispose()
		{
			foreach (var handler in _progressHandlers)
			{
				var d = handler as IDisposable;
				if(d!=null)
					d.Dispose();
			}
		}
	}

	public class ConsoleProgress : IProgress, IDisposable
	{
		public static int indent = 0;
		public ConsoleProgress()
		{
		}

		public ConsoleProgress(string mesage, params string[] args)
		{
			WriteStatus(mesage, args);
			indent++;
		}

		public void WriteStatus(string message, params object[] args)
		{
			Debug.Write("                          ".Substring(0, indent*2));
			Debug.WriteLine(string.Format(message, args));
		}

		public void WriteMessage(string message, params object[] args)
		{
			WriteStatus(message, args);

		}


		public void WriteWarning(string message, params object[] args)
		{
			WriteStatus("Warning: "+ message, args);
		}

		public void WriteError(string message, params object[] args)
		{
			WriteStatus("Error: "+ message, args);

		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			indent--;
		}



	}


	public class TextBoxProgress : GenericProgress
	{
		private RichTextBox _box;

		public TextBoxProgress(RichTextBox box)
		{
			_box = box;
			_box.Multiline = true;
		}

		public override void WriteStatus(string message, params object[] args)
		{
			_box.Text += "                          ".Substring(0, indent * 2);
			_box.Text += String.Format(message + Environment.NewLine, args);
		}
	}

	public class StringBuilderProgress : GenericProgress
	{
		private StringBuilder _builder = new StringBuilder();

		public override void WriteStatus(string message, params object[] args)
		{
			_builder.Append("                          ".Substring(0, indent * 2));
			_builder.AppendFormat(message+Environment.NewLine, args);
		}

		public string Text
		{
			get { return _builder.ToString(); }
		}

		public void Clear()
		{
			_builder = new StringBuilder();
		}
	}


	public abstract class GenericProgress : IProgress
	{
		public int indent = 0;

		public GenericProgress()
		{
		}

		public abstract void WriteStatus(string message, params object[] args);

		public void WriteMessage(string message, params object[] args)
		{
			WriteStatus(message, args);
		}

		public void WriteWarning(string message, params object[] args)
		{
			WriteStatus(message, args);
		}

		public void WriteError(string message, params object[] args)
		{
			WriteStatus("Error:" + message, args);
		}
	}
}