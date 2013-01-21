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
using System.IO;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace chess.application
{

	public class TIOTest
	{

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_commands")]
		static extern void bcm2835_commands(IntPtr ptr);

		unsafe static void testFunc() {
			uint[] ar = new uint[100];
			ar[0] = 1;
			ar[1] = 12;
			ar[2] = 13;
			fixed (uint* p = ar) {
				bcm2835_commands((IntPtr)p);
			}
		}

		public void handleConsole() {
			lock (TCommandLineThread.consoleCommandQueue)
				while (TCommandLineThread.consoleCommandQueue.Count > 0) {
					string line = TCommandLineThread.consoleCommandQueue.Dequeue();
					if (line != null) {
						var parts = line.Split(' ');
						var args = new List<string>(parts);
						args.RemoveAt(0);
						onConsoleLine(new TConsoleLineEvent() { line = line, command = parts[0], args = args.ToArray() });
					}
				}
		}

		public void onConsoleLine(TConsoleLineEvent e) {
			Console.WriteLine("cmd:"+e.command);
			if (e.command == "p") {
				if (File.Exists("debug.png")) File.Delete("debug.png");
				adapter.saveCacheFile("debug.png");
			}
		}

		TOLEDDisplayAdapter adapter;

		public void start() {
			var cmdThread = new TCommandLineThread();
			cmdThread.start();

			Console.WriteLine("starte test");
			//testFunc();
			Console.WriteLine("beende test");
			//unsafe fixed(int* p = &ar) {

			//}

			var bits = larne.io.ic.IOUtils.getBits(1);
			foreach (var bit in bits) Console.Write(bit ? 1 : 0);
			Console.WriteLine();
			//var bytes = larne.io.ic.IOUtils.ToByteArray(bits);
			//foreach (var b in bytes) Console.WriteLine(b);
			//Console.WriteLine();

			var device = new RPI();

			var D16_SDI = new GPIOMem(GPIOPins.V2_GPIO_25, GPIODirection.Out, false);
			var D17_CLK = new GPIOMem(GPIOPins.V2_GPIO_08, GPIODirection.Out, false);
			var CS = new GPIOMem(GPIOPins.V2_GPIO_07, GPIODirection.Out, false);

			var RST = device.createPin(GPIOPins.V2_GPIO_23, GPIODirection.Out, false);
			var RS = new GPIOMem(GPIOPins.V2_GPIO_24, GPIODirection.Out, false);

			var spi = new TSPIEmulator(D16_SDI, null, D17_CLK, CS);
			var rnd = new Random();
			var watch = new System.Diagnostics.Stopwatch();

			var bus = new TOLEDSPIFastDataBus(spi, RST, RS);
			var lcd = new TOLEDDisplay(bus);
			lcd.orientation(3);
			lcd.background(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)));

			var bg = (Bitmap)Image.FromFile(chess.shared.Config.applicationPath + "tmp/test.bmp");

			//lcd.drawImage(bmp, 0, 0, bmp.Width, bmp.Height);
			lcd.cls();
			 
			adapter = new TOLEDDisplayAdapter(lcd);
			//adapter.update(bg, 0, 0, lcd.width, lcd.height);

			var bmp = new Bitmap(lcd.width, lcd.height);
			var gfx = Graphics.FromImage(bmp);
			gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			var st = new List<string>();
			st.Add("Erster Eintrag");
			st.Add("Zweiter Eintrag");
			st.Add("Dritter Eintrag");
			st.Add("Vierter Eintrag");
			st.Add("Fünfter Eintrag");
			st.Add("Sechster Eintrag");
			st.Add("Siebter Eintrag");
			st.Add("Achter Eintrag");
			st.Add("Neunter Eintrag");
			st.Add("Zehnter Eintrag");

			while (true) {
				handleConsole();
				//gfx.Clear(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)));
				gfx.DrawImage(bg, 0, 0);

				st.Add(st[0]);
				st.RemoveAt(0);
				//gfx.DrawString(string.Join("\n", st.ToArray()), new Font(FontFamily.GenericSansSerif, 11), new SolidBrush(Color.DarkBlue), new PointF(0, 0));

				gfx.DrawString("|", new Font(FontFamily.GenericSansSerif, 18), new SolidBrush(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))), new PointF(rnd.Next(120) - 10, rnd.Next(130) - 10));

				watch.Restart();
				adapter.update(bmp, 0, 0, lcd.width, lcd.height);
				//lcd.background(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)));
				//lcd.cls();
				Console.WriteLine(watch.ElapsedMilliseconds);
				System.Threading.Thread.Sleep(400);
			}
			Console.ReadLine();
		}

	}

}
