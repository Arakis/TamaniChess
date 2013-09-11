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
using RaspberryPiDotNet;
using larne.io.ic;
using Plossum.CommandLine;
using chess.shared;

namespace chess.application
{

	class Program
	{

		public static TApplication app;
		public static TProgrammOptions options;

		static void Main(string[] args) {
			options = new TProgrammOptions();
			var parser = new CommandLineParser(options);
			parser.Parse();

			CommandLineThread.start();

			if (options.help) {
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
				Environment.Exit(0);
			}
			else if (parser.HasErrors) {
				Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));
				Environment.Exit(0);
			}
			else if (options.temp) {
				startTemp();
				Environment.Exit(0);
			}
			else if (options.test) {
				startTest();
				Environment.Exit(0);
			}
			else {
				startApp();
				Environment.Exit(0);
			}

		}

		private static void startApp() {
			app = new TApplication();
			app.start();
		}

		private static void startTemp() {
			var test = new TTempClass() { };
			test.start();
		}

		private static void startTest() {
			var test = new TIOTest() { };
			test.start();
		}

		public static void printStack() {
			//try {
			//	throw new Exception();
			//}
			//catch (Exception e) {
			//	Console.WriteLine(e.ToString());
			//}
			var s = new System.Diagnostics.StackTrace(true);
			Console.WriteLine("### STACK ###");
			foreach (var f in s.GetFrames()) {
				Console.WriteLine("  at {0} in {1}:{2}", f.GetMethod().ToString(), f.GetFileName(), f.GetFileLineNumber());
			}
		}

	}

}
