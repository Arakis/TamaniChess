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

namespace ConsoleApplication1
{

	class Program
	{

		static void Main(string[] args) {
			Console.WriteLine("huhu2");
			//var p1 = new GPIOFile(GPIO.GPIOPins.GPIO02, GPIO.DirectionEnum.IN);
			//while (true) {
			//		Console.Write(p1.Read() ? "1" : "0");
			//		System.Threading.Thread.Sleep(1000);
			//}

			var device = new RPI();

			var namedPins = new TNamedPins();

			namedPins.Add("LOW", device.createPin(GPIOPins.V2_GPIO_03, GPIODirection.Out, false));
			namedPins.Add("HI", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, true));

			namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_02, GPIODirection.Out, false));
			namedPins.Add("OE", null);
			namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_04, GPIODirection.Out, false));
			namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, false));
			namedPins.Add("SRCLR", null);

			namedPins.Add("O7", device.createPin(GPIOPins.V2_GPIO_10, GPIODirection.In));
			namedPins.Add("CP", device.createPin(GPIOPins.V2_GPIO_09, GPIODirection.Out, false));
			namedPins.Add("PL", device.createPin(GPIOPins.V2_GPIO_11, GPIODirection.Out, false));

			//while (true)
			//	if (namedPins["O7"].Read()) {
			//		Console.Write("1");
			//		System.Threading.Thread.Sleep(100);
			//	}

			var sipo = new TSIPO(namedPins["SER"], namedPins["OE"], namedPins["RCLK"], namedPins["SRCLK"], namedPins["SRCLR"]);
			sipo.setBits(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
			//return;
			var piso = new TPISO(namedPins["O7"], namedPins["PL"], namedPins["CP"]);

			var sipoArray = new TSIPOArray(sipo, 16);
			var ledMapping = new TBitMapping(8);
			ledMapping.setMapping(0, 1, 7, 6, 2, 3, 4, 5);
			var outMapping = new TBitMapping(8);
			//outMapping.setMapping(0, 2, 3, 4, 6, 1, 5, 7);
			outMapping.setMapping(4, 0, 1, 2, 5, 6, 3, 7);

			var readBitCount = 64;
			var lastBits = new bool[readBitCount];
			var ledBits = new bool[88];

			while (true) {

				//sipo.setBits(new int[] { 1, 0, 1, 0, 1, 0, 1, 0 });
				//System.Threading.Thread.Sleep(200);
				//sipo.setBits(new int[] { 0, 1, 0, 1, 0, 1, 0, 1 });
				//System.Threading.Thread.Sleep(200);
				//continue;
				
				piso.load();
				piso.readBits(8*0, false);
				var tmpBits = outMapping.convert(piso.readBits(8, false)).ToArray();
				for (var i = 0; i < 8; i++) {
					if (tmpBits[i] != lastBits[i]) {
						foreach (var bit in tmpBits) {
							Console.Write(bit ? "1" : "0");
						}
						Console.Write(" ");
						break;
					}
				}

				lastBits = tmpBits;

				Array.Copy(lastBits, ledBits, 8);


				//foreach (var bit in tmpBits) {
				//	Console.Write(bit ? "1" : "0");
				//}
				//Console.Write(" ");

				//if (namedPins["O7"].Read()) ledBits[0] = true;

				//for (var i = 0; i < readBitCount; i++) {
				//	if (tmpBits[i])
				//		switch (i) {
				//			case 0:
				//				ledBits[2] = true;
				//				break;
				//			case 1:
				//				ledBits[1] = true;
				//				break;
				//			case 2:
				//				ledBits[0] = true;
				//				break;
				//			case 3:
				//				ledBits[7] = true;
				//				break;
				//			case 4:
				//				ledBits[3] = true;
				//				break;
				//			case 5:
				//				ledBits[4] = true;
				//				break;
				//			case 6:
				//				ledBits[5] = true;
				//				break;
				//			case 7:
				//				ledBits[6] = true;
				//				break;
				//		}
				//}

				//ledBits[6] = true;
				//sipoArray.setBits(ledMapping.convert(ledBits), 0);
				sipoArray.flush();

				//var rowCount = 2;
				//var columnCount = 4;// 4;
				//var inputArray = new bool[columnCount, rowCount];

				//for (var column = 0; column < columnCount; column++) {
				//	var columnBits = new bool[8];
				//	columnBits[column] = true;
				//	sipoArray.setBits(columnBits);
				//	sipoArray.flush();

				//	var row = 0;
				//	foreach (var bit in piso.readBits(rowCount)) {
				//		Console.Write(bit ? "1" : "0");
				//		inputArray[column, row] = bit;
				//		ledBits[(row * columnCount) + column] = bit;
				//		row++;
				//	}
				//	Console.WriteLine();
				//}

				//Console.WriteLine();
				//sipoArray.setBits(ledMapping.convert(ledBits), 8);
				//sipoArray.flush();
				//System.Threading.Thread.Sleep(500);

				//Console.Write(" ");

				//namedPins["PL"].Write(true);
				//System.Threading.Thread.Sleep(100);
				//namedPins["PL"].Write(false);
				//System.Threading.Thread.Sleep(100);

				//namedPins["CP"].Write(false);
				//System.Threading.Thread.Sleep(100);
				//printPin(namedPins["O7"]);
				//System.Threading.Thread.Sleep(100);
				//namedPins["CP"].Write(true);
				//System.Threading.Thread.Sleep(100);

				//namedPins["CP"].Write(false);
				//System.Threading.Thread.Sleep(100);
				//printPin(namedPins["O7"]);
				//System.Threading.Thread.Sleep(100);
				//namedPins["CP"].Write(true);
				//System.Threading.Thread.Sleep(100);

				//namedPins["CP"].Write(false);
				//System.Threading.Thread.Sleep(100);
				//printPin(namedPins["O7"]);
				//System.Threading.Thread.Sleep(100);
				//namedPins["CP"].Write(true);
				//System.Threading.Thread.Sleep(100);

				//namedPins["CP"].Write(false);
				//System.Threading.Thread.Sleep(100);
				//printPin(namedPins["O7"]);
				//System.Threading.Thread.Sleep(100);
				//namedPins["CP"].Write(true);
				//System.Threading.Thread.Sleep(100);

				//System.Threading.Thread.Sleep(1000);
			}

		}

		static void printPin(GPIO pin) {
			Console.Write(pin.Read() ? "1" : "0");
		}

	}




}
