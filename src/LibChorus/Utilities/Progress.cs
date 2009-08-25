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
		void WriteVerbose(string message, params object[] args);
		bool ShowVerbose {set; }
		bool CancelRequested { get;  set; }
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

		public void WriteVerbose(string message, params object[] args)
		{

		}

		public bool ShowVerbose
		{
			get { return false; }
			set {  }
		}

		public bool CancelRequested { get; set; }
	}

	public class MultiProgress : IProgress, IDisposable
	{
		private readonly List<IProgress> _progressHandlers=new List<IProgress>();
		private bool _cancelRequested;

		public MultiProgress(IEnumerable<IProgress> progressHandlers)
		{
			_progressHandlers.AddRange(progressHandlers);
		}


		public bool CancelRequested
		{

			get
			{
				foreach (var handler in _progressHandlers)
				{
					if (handler.CancelRequested)
						return true;
				}
				return _cancelRequested;
			}
			set
			{
				_cancelRequested = value;
			}
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

		public void WriteVerbose(string message, params object[] args)
		{
			foreach (var handler in _progressHandlers)
			{
				handler.WriteVerbose(message, args);
			}
		}

		public bool ShowVerbose
		{
			set //review: the best policy isn't completely clear here
			{
				foreach (var handler in _progressHandlers)
				{
					handler.ShowVerbose = value;
				}
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

		public void Add(IProgress progress)
		{
			_progressHandlers.Add(progress);
		}
	}

	public class ConsoleProgress : IProgress, IDisposable
	{
		public static int indent = 0;
		private bool _verbose;

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

		public void WriteVerbose(string message, params object[] args)
		{
			if(!_verbose)
				return;
			var lines = String.Format(message, args);
			foreach (var line in lines.Split('\n'))
			{
				WriteStatus(": " + line);
			}

		}

		public bool ShowVerbose
		{
			set { _verbose = value; }
		}

		public bool CancelRequested { get; set; }

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
			try
			{
			 // if (_box.InvokeRequired)
				_box.Invoke(new Action( ()=>
				{
					_box.Text += "                          ".Substring(0, indent * 2);
					_box.Text += String.Format(message + Environment.NewLine, args);
				}));
			}
			catch (Exception)
			{

			}
//            _box.Invoke(new Action<TextBox, int>((box, indentX) =>
//            {
//                box.Text += "                          ".Substring(0, indentX * 2);
//                box.Text += String.Format(message + Environment.NewLine, args);
//            }), _box, indent);
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

	public class StatusProgress : IProgress
	{

		public string LastStatus { get; private set; }
		public string LastWarning { get; private set; }
		public string LastError { get; private set; }
		public bool CancelRequested { get; set; }
		public bool WarningEncountered { get { return !string.IsNullOrEmpty(LastWarning); } }
		public bool ErrorEncountered { get { return !string.IsNullOrEmpty(LastError); } }


	   public  void WriteStatus(string message, params object[] args)
		{
			LastStatus = string.Format(message, args);
		}
		public void WriteWarning(string message, params object[] args)
		{
			LastWarning = string.Format(message, args);
		}

		public void WriteError(string message, params object[] args)
		{
			LastError = string.Format(message, args);
		}

		public void WriteMessage(string message, params object[] args)
		{
		}

		public void WriteVerbose(string message, params object[] args)
		{
		}

		public bool ShowVerbose
		{
			set {  }
		}

		public void Clear()
		{
			LastError = LastWarning =LastStatus = string.Empty;
		}
	}

	public abstract class GenericProgress : IProgress
	{
		public int indent = 0;
		private bool _verbose;

		public GenericProgress()
		{
		}
		public bool CancelRequested { get; set; }
		public abstract void WriteStatus(string message, params object[] args);

		public void WriteMessage(string message, params object[] args)
		{
			WriteStatus(message, args);
		}

		public void WriteWarning(string message, params object[] args)
		{
			WriteStatus("Warning: "+message, args);
		}

		public void WriteError(string message, params object[] args)
		{
			WriteStatus("Error:" + message, args);
		}

		public void WriteVerbose(string message, params object[] args)
		{
			if(!_verbose)
				return;
			var lines = String.Format(message, args);
			foreach (var line in lines.Split('\n'))
			{

				WriteStatus(": " + line);
			}

		}

		public bool ShowVerbose
		{
			set { _verbose = value; }
		}
	}
}