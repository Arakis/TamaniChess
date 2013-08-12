/******************************************************************************************************

  TamaniChess is a chess computer
  Copyright (C) 2013  Tamani UG

  Redistribution and use of the TamaniChess source code, TamaniChess constructions plans or any
  derivative works are permitted provided that the following conditions are met:

  * Redistributions may not be sold, nor may they be used in a commercial product or activity.

  * Redistributions that are modified from the original source must include the complete source code,
    including the source code for all components used by a binary built from the modified sources.
    However, as a special exception, the source code distributed need not include anything that is
    normally distributed (in either source or binary form) with the major components (compiler,
    kernel, and so on) of the operating system on which the executable runs, unless that component
    itself accompanies the executable.

  * Redistributions must reproduce the above copyright notice, this list of conditions and the
    following disclaimer in the documentation and/or other materials provided with the distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
  IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
  OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
******************************************************************************************************/

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

		public bool validate(string oldFEN, out string newFEN, out ECheckState checkState, string move = "") {
			var foundFen = "";
			var tmpCheckState = ECheckState.none;
			bool wait = true;
			var func = new Action<string>((s) => {
				if (s.Contains("Checkers: ") && s.Length > "Checkers: ".Length) tmpCheckState = ECheckState.check;
				if (s.Contains("Legal moves: ") && s.Length == "Legal moves: ".Length) tmpCheckState = ECheckState.mate;
				if (s.Contains("Fen: ")) {
					foundFen = s.Replace("Fen: ", "");
					wait = false;
				}
			});
			try {
				onNewLine += func;
				send("position fen " + oldFEN + (move == "" ? "" : " moves " + move));
				send("d");

				while (wait) System.Threading.Thread.Sleep(10);
			}
			finally {
				onNewLine -= func;
			}
			newFEN = foundFen;
			Console.WriteLine("Parsed FEN: " + foundFen);
			checkState = tmpCheckState;
			return oldFEN != newFEN || move == ""; //TODO
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

		public int thinkingTime = 50; //1000;
		public int depth = 1; //20;

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
					send("go movetime " + thinkingTime.ToString() + " depth " + depth.ToString());
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
