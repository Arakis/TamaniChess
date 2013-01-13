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
			time = DateTime.Now;
			if (updateThread == null) {
				updateThread = new Thread(() => {
					Console.WriteLine("update delayed");
					while ((DateTime.Now - time).TotalSeconds < 5) {
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
				var psi = new ProcessStartInfo("mono", "--debug " + Config.applicationExe);
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
					process.Kill();
					process = null;
				}
				catch { }
			}

			Process.Start("sudo", "pkill -f \"chess.application.exe\"");
		}

	}

}
