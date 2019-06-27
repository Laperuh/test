using System;
using System.Diagnostics;

namespace AprioriAllLib
{
	/// <summary>
	/// Used to log what the algorithm does.
	/// </summary>
	internal class Log
	{
		public static bool UseTrace = false;

		public static bool UseConsoleOut = true;

		public static bool LogMsgNumber = false;

		public static bool IndicateMsgInTrace = true;

		public static bool IndicateMsgInConsoleOut = true;

		private static object _lock;

		private static string msgNumberFormat;

		private static string msgNumberWithMsgFormat;

		private static string msgNumberPlaceholder;

		private static uint msgNumber;

		static Log()
		{
			_lock = new object();
			msgNumberFormat = "[{0}]";
			msgNumberPlaceholder = "!";
			msgNumberWithMsgFormat = msgNumberFormat + "{1}";
			msgNumber = 0;
		}

		private static void HandleMsgNumberIndicator_Trace()
		{
			if (IndicateMsgInTrace)
			{
				if (LogMsgNumber)
					Trace.Write(String.Format(msgNumberFormat, msgNumber));
				else
					Trace.Write(msgNumberPlaceholder);
			}
		}

		private static void HandleMsgNumberIndicator_ConsoleOut()
		{
			if (IndicateMsgInConsoleOut)
			{
				if (LogMsgNumber)
					Console.Out.Write(msgNumberFormat, msgNumber);
				else
					Console.Out.Write(msgNumberPlaceholder);
			}
		}

		public static void Write(string s)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.Write(String.Format(msgNumberWithMsgFormat, msgNumber, s));
					else
						Trace.Write(s);
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.Write(msgNumberWithMsgFormat, msgNumber, s);
					else
						Console.Out.Write(s);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void Write(object o)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.Write(String.Format(msgNumberWithMsgFormat, msgNumber, o));
					else
						Trace.Write(o);
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.Write(msgNumberWithMsgFormat, msgNumber, o);
					else
						Console.Out.Write(o);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void Write(string format, params object[] args)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.Write(String.Format(msgNumberWithMsgFormat, msgNumber, String.Format(format, args)));
					else
						Trace.Write(String.Format(format, args));
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.Write(msgNumberWithMsgFormat, msgNumber, String.Format(format, args));
					else
						Console.Out.Write(format, args);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void WriteLine()
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.WriteLine(String.Format(msgNumberFormat, msgNumber));
					else
						Trace.WriteLine(String.Empty);
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.WriteLine(msgNumberFormat, msgNumber);
					else
						Console.Out.WriteLine();
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void WriteLine(string s)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.WriteLine(String.Format(msgNumberWithMsgFormat, msgNumber, s));
					else
						Trace.WriteLine(s);
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.WriteLine(msgNumberWithMsgFormat, msgNumber, s);
					else
						Console.Out.WriteLine(s);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void WriteLine(object o)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.WriteLine(String.Format(msgNumberWithMsgFormat, msgNumber, o));
					else
						Trace.WriteLine(o);
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.WriteLine(msgNumberWithMsgFormat, msgNumber, o);
					else
						Console.Out.WriteLine(o);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

		public static void WriteLine(string format, params object[] args)
		{
			lock (_lock)
			{
				if (LogMsgNumber)
					++msgNumber;

				if (UseTrace)
				{
					if (LogMsgNumber)
						Trace.WriteLine(String.Format(msgNumberWithMsgFormat, msgNumber, String.Format(format, args)));
					else
						Trace.WriteLine(String.Format(format, args));
				}
				else
					HandleMsgNumberIndicator_Trace();

				if (UseConsoleOut)
				{
					if (LogMsgNumber)
						Console.Out.WriteLine(msgNumberWithMsgFormat, msgNumber, String.Format(format, args));
					else
						Console.Out.WriteLine(format, args);
				}
				else
					HandleMsgNumberIndicator_ConsoleOut();
			}
		}

	}
}
