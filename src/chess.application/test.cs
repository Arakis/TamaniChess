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

		public void start() {
			try {
				while (true) {
					CommandLineThread.processEvents(onConsoleLine);

					Thread.Sleep(10);
				}
			}
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		public void onConsoleLine(TConsoleLineEvent e) {
			GC.Collect(2);

			var notFound = false;

			switch (e.command) {
				case "all-pins-on":
					allPinsOn();
					break;
				case "pon":
				case "pin-on":
					setPin(int.Parse(e.args[0]), true);
					break;
				case "poff":
				case "pin-off":
					setPin(int.Parse(e.args[0]), false);
					break;
				case "all-leds-on":
					allLEDsOn();
					break;
				case "switches":
					switchTest();
					break;
				case "all-sipo-on":
				case "ason":
					if (e.args.Length == 0)
						setAllSipo(true);
					else
						setAllSipo(true, int.Parse(e.args[0]));
					break;
				case "all-sipo-off":
				case "asoff":
					if (e.args.Length == 0)
						setAllSipo(false);
					else
						setAllSipo(false, int.Parse(e.args[0]));
					break;
				case "leddemo":
					ledDemo();
					break;
				case "t1":
					test1();
					break;
				default:
					notFound = true;
					break;
			}

			if (notFound)
				Console.WriteLine("invalid command");
			else
				Console.WriteLine("command processed");
		}

		public void allLEDsOn() {
			var hw = new TIOHardware();
			hw.init();
			hw.loadingLED = false;
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					hw.ledBitArray[x, y] = true;
				}
			}
			hw.updateLeds();
		}

		public void setAllSipo(bool state, int exceptIndex = -1) {
			var device = new RPI();
			var namedPins = new TNamedPins();

			namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, false));
			namedPins.Add("OE", null);
			namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_22, GPIODirection.Out, false));
			namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, false));
			namedPins.Add("SRCLR", null);

			var sipo = new TSIPO(namedPins["SER"], namedPins["OE"], namedPins["RCLK"], namedPins["SRCLK"], namedPins["SRCLR"]);

			var bits = new List<bool>();
			for (var i = 128 - 1; i >= 0; i--)
				bits.Add(i == exceptIndex ? !state : state);

			sipo.setBits(bits);
		}

		public void test1() {
			var device = new RPI();
			var namedPins = new TNamedPins();

			namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, false));
			namedPins.Add("OE", null);
			namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_22, GPIODirection.Out, false));
			namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, false));
			namedPins.Add("SRCLR", null);

			//var sipo = new TSIPO(namedPins["SER"], namedPins["OE"], namedPins["RCLK"], namedPins["SRCLK"], namedPins["SRCLR"]);

			//var bits = new List<bool>();
			//for (var i = 128 - 1; i >= 0; i--)
			//	bits.Add(i == exceptIndex ? !state : state);

			//sipo.setBits(bits);
			for (var i = 0; i < 3; i++) {
				namedPins["SER"].Write(true);
				System.Threading.Thread.Sleep(750);

				namedPins["SRCLK"].Write(true);
				System.Threading.Thread.Sleep(750);
				namedPins["SRCLK"].Write(false);
				System.Threading.Thread.Sleep(750);

				namedPins["RCLK"].Write(true);
				System.Threading.Thread.Sleep(750);
				namedPins["RCLK"].Write(false);
				System.Threading.Thread.Sleep(750);
			}
		}

		public void ledDemo() {
			Console.WriteLine("led demo");
			var hw = new TIOHardware();
			hw.init();

			for (var y = 0; y < 9; y++) {
				hw.clearBoardLeds();
				for (var x = 0; x < 9; x++) {
					hw.ledBitArray[x, y] = true;
				}
				hw.updateLeds();
				System.Threading.Thread.Sleep(1000);
			}

			for (var x = 0; x < 9; x++) {
				hw.clearBoardLeds();
				for (var y = 0; y < 9; y++) {
					hw.ledBitArray[x, y] = true;
				}
				hw.updateLeds();
				System.Threading.Thread.Sleep(1000);
			}

		}

		public void switchTest() {
			Console.WriteLine("switch test");
			//var hw = new TIOHardware();
			//hw.init();

			//var ret = false;
			//CommandLineThread.processEvents((e) => {
			//	ret = true;
			//	onConsoleLine(e);
			//});


			//while (true) {
			//	hw.clearBoardLeds();

			//	for (var y = 0; y < 8; y++) {
			//		for (var x = 0; x < 8; x++) {
			//			if (hw.figureSwitchesNew[x, y]) {
			//				hw.ledBitArray[x, y] = true;
			//				hw.ledBitArray[x + 1, y] = true;
			//				hw.ledBitArray[x, y + 1] = true;
			//				hw.ledBitArray[x + 1, y + 1] = true;
			//			}
			//		}
			//	}

			//	if (ret) return;

			//	hw.updateLeds();
			//}

			var ec = new TEventController();
			new TSwitchTest().install(ec);
			ec.eventLoop();
		}

		public class TSwitchTest : THandler
		{

			public override void onButtonChanged(TButtonChangeEvent e) {
				base.onButtonChanged(e);
				Console.WriteLine(e.button.ToString());
			}
			public override void onPieceChangedDelay(TSwitchChangeEvent e) {
				Console.WriteLine((e.state ? "" : "             ") + e.pos.ToString() + " " + e.state.ToString());
				//boardLeds.clear();
				//if (e.state)
				//	boardLeds[e.pos].state = e.state;
			}

			public override void onPiecesChangedDelay(TSwitchesChangesEvent e) {
				base.onPiecesChangedDelay(e);
				boardLeds.clear();
				for (var y = 0; y < 8; y++) {
					for (var x = 0; x < 8; x++) {
						if (e.newSwitches[x, y]) {
							//boardLeds[x, y].state = true;
							//boardLeds[x, y + 1].state = true;
							foreach (var led in boardLeds.getAllFieldLeds(x, y))
								led.on();
						}
						Console.Write(e.newSwitches[x, y] ? "1" : "0");
					}
					Console.WriteLine();
				}
			}

		}

		public void allPinsOn() {
			var namedPins = new TNamedPins();
			var device = new RPI();

			namedPins.Add("LOW", device.createPin(GPIOPins.V2_GPIO_03, GPIODirection.Out, true));
			namedPins.Add("HI", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, true));

			namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_02, GPIODirection.Out, true));
			namedPins.Add("OE", null);
			namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_04, GPIODirection.Out, true));
			namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, true));
			namedPins.Add("SRCLR", null);

			namedPins.Add("O7", device.createPin(GPIOPins.V2_GPIO_10, GPIODirection.Out));
			namedPins.Add("CP", device.createPin(GPIOPins.V2_GPIO_09, GPIODirection.Out, true));
			namedPins.Add("PL", device.createPin(GPIOPins.V2_GPIO_11, GPIODirection.Out, true));

			namedPins.Add("D16_SDI", device.createPin(GPIOPins.V2_GPIO_25, GPIODirection.Out, true));
			namedPins.Add("D17_CLK", device.createPin(GPIOPins.V2_GPIO_08, GPIODirection.Out, true));
			namedPins.Add("CS", device.createPin(GPIOPins.V2_GPIO_07, GPIODirection.Out, true));

			namedPins.Add("RST", device.createPin(GPIOPins.V2_GPIO_23, GPIODirection.Out, true));
			namedPins.Add("RS", device.createPin(GPIOPins.V2_GPIO_24, GPIODirection.Out, true));

			foreach (var entry in namedPins) {
				if (entry.Value != null) {
					entry.Value.PinDirection = GPIODirection.Out;
					entry.Value.Write(true);
				}
			}
		}

		public void setPin(int gpio, bool state) {
			var device = new RPI();
			using (var pin = device.createPin((GPIOPins)gpio, GPIODirection.Out)) {
				pin.Write(state);
			}
		}

	}

}