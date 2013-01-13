using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using chess.shared;
using System.IO;

namespace chess.application
{

	public class TConsoleProcess
	{

		Thread appThread;
		Process process;

		public TConsoleProcess(string path, string args) {
			var wait = true;

			appThread = new Thread(() => {
				var psi = new ProcessStartInfo(path, args);
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardInput = true;
				process = new Process();
				process.StartInfo = psi;
				//process.OutputDataReceived += (sender, e) => {
				//	Console.Write(e.Data);
				//};
				Console.WriteLine("starting app");
				process.Start();

				var buf = new char[1];
				var sb = new StringBuilder();
				while (!process.HasExited) {
					var count = process.StandardOutput.Read(buf, 0, 1);
					if (count > 0) {
						wait = false;
						//Console.Write(buf[0]);
						if (buf[0].ToString() == Environment.NewLine) {
							Console.WriteLine("APP: " + sb.ToString());
							//if (onNewLine != null)
							//	onNewLine(sb.ToString());
							//sb.Clear();
						}
						else {
							sb.Append(buf[0]);
						}
					}
					else {
						Thread.Sleep(50);
					}
				}

				process.WaitForExit();
			});
			appThread.Start();

			while (wait) Thread.Sleep(10);
			Console.WriteLine("app started");
		}
	}

	public class TEngine
	{
		private Process process;
		private static Thread appThread;

		public void start() {
			stop();

			var wait = true;

			appThread = new Thread(() => {
				var psi = new ProcessStartInfo(Path.Combine(Config.applicationPath, "stockfish", "stockfish"));
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardInput = true;
				process = new Process();
				process.StartInfo = psi;
				//process.OutputDataReceived += (sender, e) => {
				//	Console.Write(e.Data);
				//};
				Console.WriteLine("starting chess engine");
				process.Start();
				process.PriorityClass = ProcessPriorityClass.BelowNormal;

				var buf = new char[1];
				var sb = new StringBuilder();
				while (!process.HasExited) {
					var count = process.StandardOutput.Read(buf, 0, 1);
					if (count > 0) {
						//Console.Write(buf[0]);
						if (buf[0].ToString() == Environment.NewLine) {
							Console.WriteLine("ENGINE: " + sb.ToString());
							if (onNewLine != null)
								onNewLine(sb.ToString());
							sb.Clear();
							wait = false;
						}
						else {
							sb.Append(buf[0]);
						}
					}
					else {
						Thread.Sleep(50);
					}
				}

				process.WaitForExit();
			});
			appThread.Start();

			while (wait) Thread.Sleep(10);
			Console.WriteLine("chess engine started");
			setOption("OwnBook", true);
			setOption("Book File", Path.Combine(Config.applicationPath, "stockfish", "Book.bin"));
			setOption("Best Book Move", true); //Needed?
			setOption("Skill Level", 0);

			uci();
		}

		public event Action<string> onNewLine;

		public void stop() {
			if (process == null) return;
			process.Kill();
			process = null;

			Process.Start("sudo", "pkill -f \"stockfish\"");
		}

		public void newGame() {
			position(TChessBoard.startFEN);
		}

		public bool validateMove(string oldFEN, string move, out string newFEN, out bool isCheck) {
			var foundFen = "";
			var tmpIsCheck = false;
			bool wait = true;
			var func = new Action<string>((s) => {
				if (s.Contains("Checkers: ")) tmpIsCheck = true;
				if (s.Contains("Fen: ")) {
					foundFen = s.Replace("Fen: ", "");
					wait = false;
				}
			});
			try {
				onNewLine += func;
				send("position fen " + oldFEN + " moves " + move);
				send("d");

				while (wait) System.Threading.Thread.Sleep(10);
			}
			finally {
				onNewLine -= func;
			}
			newFEN = foundFen;
			Console.WriteLine("Parsed FEN: " + foundFen);
			isCheck = tmpIsCheck;
			return oldFEN != newFEN;
		}

		public void position(string fenStr) {
			send("position fen " + fenStr);
		}

		public void uci() {
			//send("uci");
		}

		public void debug() {
			send("d");
		}

		public void setOption(string name, string value) {
			send("setoption name " + name + " value " + value);
		}

		public void setOption(string name, int value) {
			send("setoption name " + name + " value " + value.ToString());
		}

		public void setOption(string name, bool value) {
			send("setoption name " + name + " value " + value.ToString().ToLower());
		}

		public void go(Action<string> cb) {
			System.Threading.ThreadPool.QueueUserWorkItem((state) => {
				var wait = true;
				var func = new Action<string>((s) => {
					if (s.StartsWith("bestmove")) {
						cb(s.Split(' ')[1]);
						wait = false;
					}
				});
				try {
					onNewLine += func;
					send("go movetime 5000");
					while (wait) Thread.Sleep(50);
				}
				finally {
					onNewLine -= func;
				}
			});
		}

		private void send(string cmd) {
			Console.WriteLine("SEND: " + cmd);
			process.StandardInput.WriteLine(cmd);
		}

	}

}
