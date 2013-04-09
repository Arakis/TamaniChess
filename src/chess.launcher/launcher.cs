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
using System.Threading.Tasks;
using chess.shared;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace chess.launcher
{

	public static class Program
	{

		private static Process process;
		private static Thread appThread;

		static void Main(string[] args) {
			Console.WriteLine("Launcher started");

			System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

			var watcher = new FileSystemWatcher(Config.updatePath);

			watcher.Changed += (a, b) => {
				startUpdate();
			};

			watcher.EnableRaisingEvents = true;

			start();

			Console.CancelKeyPress += (a, b) => stop();
			//Thread.CurrentThread.Suspend();
			while (true) {
				var line = Console.ReadLine();
				try {
					process.StandardInput.WriteLine(line);
				}
				catch { }
			}
		}

		private static DateTime time = DateTime.Now;
		public static void startUpdate() {
			bool development = true;

			time = DateTime.Now;
			if (updateThread == null) {
				updateThread = new Thread(() => {
					Console.WriteLine("update delayed");
					while ((DateTime.Now - time).TotalSeconds < (development ? 2 : 5)) {
						System.Threading.Thread.Sleep(1000);
					}

					restart = false;

					if (appThread != null) {
						appThread.Abort();
						appThread = null;
					}

					stop();
					try {
						update();
					}
					finally {
						updateThread = null;
						restart = true;
						start();
					}
				});
				updateThread.Start();
			}
		}

		private static Thread updateThread;

		public static void update() {
			Console.WriteLine("application update");
			Tools.copyDirectory(Config.updatePath, Config.applicationPath, true, true);
			Process.Start("chmod", "+x " + Config.applicationExe).WaitForExit();
			Console.WriteLine("calling pdb2mdb");
			Process.Start(new ProcessStartInfo("pdb2mdb", "chess.application.exe") { WorkingDirectory = Config.applicationPath }).WaitForExit();
		}

		private static bool restart = true;
		public static void start() {
			stop();
			appThread = new Thread(() => {
				var args = new List<string>(Environment.GetCommandLineArgs());
				args.RemoveAt(0);
				var psi = new ProcessStartInfo("mono", "--debug " + Config.applicationExe + " " + string.Join(" ", args));
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardInput = true;
				process = new Process();
				process.StartInfo = psi;
				process.OutputDataReceived += (sender, e) => {
					Console.Write(e.Data);
				};
				process.Start();
				Console.WriteLine("application started");

				var buf = new char[1];
				while (!process.HasExited) {
					var count = process.StandardOutput.Read(buf, 0, 1);
					if (count > 0)
						Console.Write(buf[0]);
					else
						System.Threading.Thread.Sleep(50);
				}

				process.WaitForExit();
				if (restart) {
					Console.WriteLine("restart");
					start();
				}

			});
			appThread.Start();
		}

		public static void stop() {
			if (process != null) {
				Console.WriteLine("application stopped");
				try {
					if (!process.HasExited) {
						Console.WriteLine("send quit signal");
						process.StandardInput.WriteLine("quit");
						process.WaitForExit(5000);
						Console.WriteLine("terminate child process");
					}
					process.Kill();
					process = null;
				}
				catch { }
			}

			Process.Start("sudo", "pkill -KILL -f \"chess.application.exe\"").WaitForExit();
		}

	}

}
