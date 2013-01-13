using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaspberryPiDotNet;
using larne.io.ic;

namespace chess.application
{

	class Program
	{

		public static TApplication app;

		static void Main(string[] args) {
			startApp();
			//startTest();
		}

		private static void startApp() {
			app = new TApplication();
			app.start();
		}

		private static void startTest() {
			var test = new TIOTest() { };
			test.start();
		}

	}

}
