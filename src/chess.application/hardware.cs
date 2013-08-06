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
using Newtonsoft.Json.Linq;
using chess.shared;

namespace chess.application
{

	public class TIOHardware : IOLEDDisplayPowerUpDown
	{

		private TPISO piso;
		private TSIPO sipo;
		private TBitMapping ledMapping;
		private TBitMapping ledMappingBottom;
		private TBitMapping ledMappingRight;
		private TBitMapping ledMappingSpecial;
		private TBitMapping outMapping;
		//private TBitMapping outMappingBugFix;
		private TBitMapping sideMapping;

		public bool[,] figureSwitchesOld = new bool[8, 8];
		public bool[,] figureSwitchesNew = new bool[8, 8];

		public bool[,] figureSwitchesOldDelay = new bool[8, 8];
		public bool[,] figureSwitchesNewDelay = new bool[8, 8];

		public int sideSwitchCount = 8;

		public bool[] sideSwitchesOld;
		public bool[] sideSwitchesNew;

		public bool[] sideSwitchesOldDelay;
		public bool[] sideSwitchesNewDelay;
		public TOLEDDisplay lcd;

		//public bool[,] copySwitches() {
		//	var bits = new bool[8, 8];
		//	for (var y = 0; y < 9; y++)
		//		for (var x = 0; x < 9; x++)
		//			bits[x, y] = figureSwitchesNew[x, y];

		//	return bits;
		//}

		public void updateDelaySwitches() {
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					figureSwitchesOldDelay[x, y] = figureSwitchesNewDelay[x, y];
					figureSwitchesNewDelay[x, y] = figureSwitchesNew[x, y];
				}
			}

			for (var i = 0; i < sideSwitchCount; i++) {
				sideSwitchesOldDelay[i] = sideSwitchesNewDelay[i];
				sideSwitchesNewDelay[i] = sideSwitchesNew[i];
				if (sideSwitchesNew[i]) Console.Write(i);
			}
		}

		public bool[,] ledBitArray = new bool[9, 9];

		public void clearBoardLeds() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					ledBitArray[x, y] = false;
				}
			}
		}

		public void init() {
			var device = new RPI();

			var namedPins = new TNamedPins();

			//namedPins.Add("LOW", device.createPin(GPIOPins.V2_GPIO_03, GPIODirection.Out, false));
			//namedPins.Add("HI", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, true));

			//namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_02, GPIODirection.Out, false));
			//namedPins.Add("OE", null);
			//namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_04, GPIODirection.Out, false));
			//namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, false));
			//namedPins.Add("SRCLR", null);

			//namedPins.Add("O7", device.createPin(GPIOPins.V2_GPIO_10, GPIODirection.In));
			//namedPins.Add("CP", device.createPin(GPIOPins.V2_GPIO_09, GPIODirection.Out, false));
			//namedPins.Add("PL", device.createPin(GPIOPins.V2_GPIO_11, GPIODirection.Out, false));

			//---

			namedPins.Add("SER", device.createPin(GPIOPins.V2_GPIO_17, GPIODirection.Out, false));
			namedPins.Add("OE", null);
			namedPins.Add("RCLK", device.createPin(GPIOPins.V2_GPIO_22, GPIODirection.Out, false));
			namedPins.Add("SRCLK", device.createPin(GPIOPins.V2_GPIO_27, GPIODirection.Out, false));
			namedPins.Add("SRCLR", null);

			namedPins.Add("O7", device.createPin(GPIOPins.V2_GPIO_23, GPIODirection.In));
			namedPins.Add("CP", device.createPin(GPIOPins.V2_GPIO_24, GPIODirection.Out, false));
			namedPins.Add("PL", device.createPin(GPIOPins.V2_GPIO_25, GPIODirection.Out, false));

			sipo = new TSIPO(namedPins["SER"], namedPins["OE"], namedPins["RCLK"], namedPins["SRCLK"], namedPins["SRCLR"]);
			//sipo.setBits(new int[200]); //clear all //WARNING: CAN DESTROY SCREEN! USE POWER UP / DOWN SEQUENCE!!
			piso = new TPISO(namedPins["O7"], namedPins["PL"], namedPins["CP"]);

			ledMapping = new TBitMapping(8);
			ledMapping.setMapping(0, 1, 2, 3, 7, 6, 5, 4);
			ledMappingBottom = new TBitMapping(8);
			ledMappingBottom.setMapping(0, 1, 2, 3, 7, 6, 5, 4); //OK?
			ledMappingRight = new TBitMapping(8);
			ledMappingRight.setMapping(0, 1, 2, 3, 7, 6, 5, 4); //OK
			//1000 0000

			ledMappingSpecial = new TBitMapping(8);
			ledMappingSpecial.setMapping(0, 1, 2, 3, 7, 6, 5, 4); //CHECK

			outMapping = new TBitMapping(8);
			//outMapping.setMapping(4, 0, 1, 2, 5, 6, 7, 3);
			//outMapping.setMapping(3, 7, 6, 5, 2, 1, 0, 4);
			outMapping.setMapping(7, 6, 5, 4, 3, 2, 1, 0);
			//0000
			//0001

			//outMappingBugFix = new TBitMapping(8);
			//outMappingBugFix.setMapping(4, 0, 1, 2, 5, 7, 6, 3);

			sideMapping = new TBitMapping(8);
			//sideMapping.setMapping(7, 0, 1, 2, 3, 4, 5, 6);
			sideMapping.setMapping(0, 1, 2, 3, 4, 5, 6, 7);

			sideSwitchesOld = new bool[sideSwitchCount];
			sideSwitchesNew = new bool[sideSwitchCount];
			sideSwitchesOldDelay = new bool[sideSwitchCount];
			sideSwitchesNewDelay = new bool[sideSwitchCount];

			//screen
			var D16_SDI = new GPIOMem(GPIOPins.V2_GPIO_10, GPIODirection.Out, false);
			var D17_CLK = new GPIOMem(GPIOPins.V2_GPIO_11, GPIODirection.Out, false);
			var CS = new GPIOMem(GPIOPins.V2_GPIO_08, GPIODirection.Out, false);

			var RST = device.createPin(GPIOPins.V2_GPIO_18, GPIODirection.Out, false);
			var RS = new GPIOMem(GPIOPins.V2_GPIO_04, GPIODirection.Out, false);

			var spi = new TSPIEmulator(D16_SDI, null, D17_CLK, CS);
			var watch = new System.Diagnostics.Stopwatch();

			var bus = new TOLEDSPIFastDataBus(spi, RST, RS);
			lcd = new TOLEDDisplay(bus, this);
			lcd.orientation(3);
			lcd.background(Color.Black);

			displayPowerOff();
		}

		public void updateSwitches() {
			piso.load();

			sideSwitchesOld = sideSwitchesNew;
			sideSwitchesNew = new bool[sideSwitchCount];
			readSideSwitchBits(0, 8, sideSwitchesNew);

			figureSwitchesOld = figureSwitchesNew;
			figureSwitchesNew = new bool[8, 8];
			readFieldSwitchBits(4, 0, figureSwitchesNew);
			readFieldSwitchBits(4, 2, figureSwitchesNew);
			readFieldSwitchBits(4, 4, figureSwitchesNew);
			readFieldSwitchBits(4, 6, figureSwitchesNew);
			readFieldSwitchBits(0, 6, figureSwitchesNew);
			readFieldSwitchBits(0, 4, figureSwitchesNew);
			readFieldSwitchBits(0, 2, figureSwitchesNew);
			readFieldSwitchBits(0, 0, figureSwitchesNew);
		}

		public bool loadingLED = true;
		public bool ScreenICOn;
		public bool ScreenLightOn;

		public void printLeds() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					Console.Write(ledBitArray[x, y] ? "1" : "0");
				}
				Console.WriteLine();
			}
		}

		private List<bool> oldLedBits = new List<bool>();
		public void updateLeds() {
			var tmpBits = new bool[8];

			//printLeds();

			//set led-pins
			var bitList = new List<bool>();
			if (ledBitArray[8, 8]) tmpBits[0] = true;
			bitList.InsertRange(0, ledMappingBottom.convert(tmpBits));

			tmpBits = new bool[8];
			for (var i = 0; i < 8; i++) {
				tmpBits[i] = ledBitArray[8, i];
			}
			bitList.InsertRange(0, ledMappingRight.convert(tmpBits));

			addLedBits(4, 0, ledBitArray, bitList);
			addLedBits(4, 2, ledBitArray, bitList);
			addLedBits(4, 4, ledBitArray, bitList);
			addLedBits(4, 6, ledBitArray, bitList);

			tmpBits = new bool[8];
			for (var i = 0; i < 8; i++) {
				tmpBits[i] = ledBitArray[i, 8];
			}
			bitList.InsertRange(0, ledMappingBottom.convert(tmpBits));

			addLedBits(0, 6, ledBitArray, bitList);
			addLedBits(0, 4, ledBitArray, bitList);
			addLedBits(0, 2, ledBitArray, bitList);
			addLedBits(0, 0, ledBitArray, bitList);

			tmpBits = new bool[8];
			tmpBits[0] = ScreenICOn;
			tmpBits[1] = ScreenLightOn;
			tmpBits[2] = !loadingLED;
			//tmpBits[0] = true;
			//tmpBits[1] = true;
			//tmpBits[2] = true;
			//tmpBits[3] = true;
			//tmpBits[4] = true;
			//tmpBits[5] = true;
			//tmpBits[6] = true;
			//tmpBits[7] = true;
			bitList.InsertRange(0, tmpBits);

			bool changed = false;
			if (oldLedBits.Count != bitList.Count) changed = true;
			else {
				for (var i = 0; i < bitList.Count; i++)
					if (bitList[i] != oldLedBits[i]) {
						changed = true;
						break;
					}
			}

			if (changed) {
				oldLedBits = bitList;
				sipo.setBits(bitList);
				//Thread.Sleep(3000);
				//foreach (var bit in bitList) Console.Write(bit ? "1" : "0"); Console.WriteLine();
			}
		}

		private void readFieldSwitchBits(int x, int y, bool[,] figureSwitches, TBitMapping mapping = null) {
			if (mapping == null) mapping = outMapping;
			var tmpBits = mapping.convert(piso.readBits(8, false)).ToArray();
			for (var i = 0; i < 4; i++) {
				figureSwitches[x + i, y] = tmpBits[i];
				figureSwitches[x + i, y + 1] = tmpBits[i + 4];
			}
		}

		private void readSideSwitchBits(int index, int count, bool[] sideSwitches, TBitMapping mapping = null) {
			if (mapping == null) mapping = sideMapping;
			var tmpBits = mapping.convert(piso.readBits(8, false)).ToArray();
			Array.Copy(tmpBits, 0, sideSwitches, index, count);
		}

		private void addLedBits(int x, int y, bool[,] ledBits, List<bool> bitList) {
			var tempBits = new bool[8];
			for (var i = 0; i < 4; i++) {
				tempBits[i] = ledBits[x + i, y];
				tempBits[i + 4] = ledBits[x + i, y + 1];
			}
			bitList.InsertRange(0, ledMapping.convert(tempBits));
		}


		public void displayPowerOn() {
			Console.WriteLine("Display power up");
			//IC on
			if (ScreenLightOn) return;
			ScreenICOn = true; //enable IC
			ScreenLightOn = false; //ensure, it's off
			updateLeds();
			Thread.Sleep(100);

			Thread.Sleep(1);
			lcd.init();

			ScreenLightOn = true;
			updateLeds();
			Thread.Sleep(1000);

			lcd.screenOn();
			Console.WriteLine("Display enabled");
		}

		public void displayPowerOff() {
			Console.WriteLine("Display power down");
			lcd.screenOff();

			ScreenICOn = true;
			ScreenLightOn = false;
			updateLeds();

			Thread.Sleep(2000);
			ScreenICOn = false;
			updateLeds();
			Thread.Sleep(100);
			Console.WriteLine("Display disabled");
		}

	}

	public class TOLEDDisplayAdapter
	{

		private TOLEDDisplay dsp;
		public TOLEDDisplayAdapter(TOLEDDisplay dsp) {
			this.dsp = dsp;
			deviceImage = new int[dsp.width, dsp.height];
		}

		private int[,] deviceImage;

		public void saveCacheFile(string file) {
			var bmp = new Bitmap(dsp.width, dsp.height);
			for (var y = 0; y < dsp.height; y++)
				for (var x = 0; x < dsp.width; x++)
					bmp.SetPixel(x, y, Color.FromArgb(deviceImage[x, y]));

			bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
		}

		public void update(Bitmap bmp, int rectX, int rectY, int rectWidth, int rectHeight) {
			//dsp.drawImage(bmp, new Point(rectX, rectY), new Point(rectX, rectY), new Size(rectWidth, rectHeight));
			//return;

			const int skipUnchanged = 5;


			for (int y = rectY; y < rectHeight; y++) {
				var newColorCount = 0;
				var unchangedColorCount = 0;

				var start = -1;
				for (int x = rectX; x < rectWidth; x++) {

					var curColor = deviceImage[x, y];
					var newColor = bmp.GetPixel(x, y).ToArgb();
					if (curColor != newColor) {
						deviceImage[x, y] = newColor;
						newColorCount++;

						if (start == -1) {
							start = x;
							unchangedColorCount = 0;
						}

						if (unchangedColorCount <= skipUnchanged) { //zu wenige ungeänderte pixel einfach mitzeichnen
							newColorCount += unchangedColorCount;
							unchangedColorCount = 0;
						}
					}
					else {
						unchangedColorCount++;

						if (unchangedColorCount > skipUnchanged && start != -1) {
							sendPart(bmp, start, y, newColorCount, 1);
							start = -1;
							newColorCount = 0;
						}

					}
				}

				if (newColorCount != 0) {
					sendPart(bmp, start, y, newColorCount, 1);
				}

			}
		}

		private void sendPart(Bitmap bmp, int rectX, int rectY, int rectWidth, int rectHeight) {
			dsp.drawImage(bmp, new Point(rectX, rectY), new Point(rectX, rectY), new Size(rectWidth, rectHeight));
		}

	}

	public abstract class TOLEDDataBus
	{

		protected GPIO RST;
		protected GPIO RS;

		public void reset() {
			//Initialize interface and reset display driver chip
			RST.Write(false);
			RST.Write(true);

		}

		public abstract void command(uint value);

		public abstract void data(uint value);

		public abstract void rgbdot(int r, int g, int b);

	}

	public class TOLEDSPIDataBus : TOLEDDataBus
	{

		public TOLEDSPIDataBus(TSPIDevice spi, GPIO RST, GPIO RS) {
			this.RS = RS;
			this.RST = RST;
			this.spi = spi;
		}

		private TSPIDevice spi;

		private bool _rs = false;
		public override void command(uint value) {
			RS.Write(false);
			spi.writeByte(value);
			RS.Write(true);
		}

		public override void data(uint value) {
			//RS.Write(true);
			spi.writeByte(value);
		}

		private bool[] tmp8Bits = new bool[8];
		private bool[] tmp6Bits = new bool[6];

		public override void rgbdot(int r, int g, int b) {
			//RS.Write(true);
			//spi.writeByte(r);
			//spi.writeByte(g);
			//spi.writeByte(b);

			var bits = tmp6Bits;

			getColorBits((byte)r, bits);
			spi.writeBits(bits);

			getColorBits((byte)g, bits);
			spi.writeBits(bits);

			getColorBits((byte)b, bits);
			spi.writeBits(bits);
		}

		public static void getColorBits(byte b, bool[] bits) {
			//bits[0] = (b & (1 << 0)) != 0;
			//bits[1] = (b & (1 << 1)) != 0;
			bits[0] = (b & (1 << 2)) != 0;
			bits[1] = (b & (1 << 3)) != 0;
			bits[2] = (b & (1 << 4)) != 0;
			bits[3] = (b & (1 << 5)) != 0;
			bits[4] = (b & (1 << 6)) != 0;
			bits[5] = (b & (1 << 7)) != 0;
		}

	}

	public unsafe class TOLEDSPIFastDataBus : TOLEDDataBus
	{

		public TOLEDSPIFastDataBus(TSPIEmulator spi, GPIO RST, GPIO RS) {
			this.RS = RS;
			this.RST = RST;
			this.spi = spi;

			set1 = GPIOMem.SetAddr;
			set0 = GPIOMem.ClrAddr;

			CSMask = (uint)spi.CS.Mask;
			SDIMask = (uint)spi.SDI.Mask;
			SCKMask = (uint)spi.SCK.Mask;
			RSMask = (uint)RS.Mask;
		}

		private TSPIDevice spi;
		private static uint* set1;
		private static uint* set0;

		private uint CSMask;
		private uint RSMask;
		private uint SCKMask;
		private uint SDIMask;

		public override void command(uint value) {
			var SDIMask = this.SDIMask;
			var SCKMask = this.SCKMask;

			int d = 0;

			*set0 = RSMask; d++; d++; d++; d++;

			//var w = new System.Diagnostics.Stopwatch();
			//w.Restart();
			//int n;
			//int wait = 1;
			//for (var i = 0; i < 1000000; i++) {
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;
			//	if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask;

			//}
			//Console.WriteLine("DDD: " + (((100*1000000)/w.ElapsedMilliseconds)*1000)/1000/1000);

			*set0 = CSMask; d++; d++; d++; d++;
			if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			*set1 = CSMask; d++; d++; d++; d++;

			*set1 = RSMask; d++; d++; d++; d++;
		}

		public override void data(uint value) {
			var SDIMask = this.SDIMask;
			var SCKMask = this.SCKMask;

			int d = 0;

			*set0 = CSMask; d++; d++; d++; d++;
			if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((value & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			*set1 = CSMask; d++; d++; d++; d++;
		}

		private bool[] tmp8Bits = new bool[8];
		private bool[] tmp6Bits = new bool[6];

		public override void rgbdot(int r, int g, int b) {
			var set0 = GPIOMem.ClrAddr;
			var set1 = GPIOMem.SetAddr;

			int d = 0;

			var SDIMask = this.SDIMask;
			var SCKMask = this.SCKMask;

			*set0 = CSMask; d++; d++; d++; d++;
			if ((r & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((r & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((r & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((r & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((r & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((r & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			*set1 = CSMask; d++; d++; d++; d++;

			*set0 = CSMask;
			if ((g & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((g & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((g & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((g & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((g & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((g & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			*set1 = CSMask; d++; d++; d++; d++;

			*set0 = CSMask; d++; d++; d++; d++;
			if ((b & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((b & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((b & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((b & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((b & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			if ((b & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; d++; d++; d++; d++; *set1 = SCKMask; d++; d++; d++; d++; *set0 = SCKMask; d++; d++; d++; d++;
			*set1 = CSMask; d++; d++; d++; d++;
		}

	}

	public class TOLED9BitDataBus : TOLEDDataBus
	{

		private GPIO[] pins;
		private GPIOPinMask mask;
		private GPIO CS;

		public TOLED9BitDataBus(GPIO RST, GPIO RS, GPIO CS, GPIO[] pins) {
			this.RS = RS;
			this.RST = RST;
			this.CS = CS;
			this.pins = pins;
			Array.Reverse(pins);
		}

		public override void command(uint value) {
			RS.Write(false);

			CS.Write(false);
			setBits(IOUtils.getBits((byte)value));
			CS.Write(true);

			RS.Write(true);
		}

		public override void data(uint value) {
			RS.Write(true);

			CS.Write(false);
			setBits(IOUtils.getBits((byte)value));
			CS.Write(true);
		}

		public override void rgbdot(int r, int g, int b) {
			RS.Write(true);

			CS.Write(false);
			setBits(IOUtils.getBits((byte)r));
			CS.Write(true);

			CS.Write(false);
			setBits(IOUtils.getBits((byte)g));
			CS.Write(true);

			CS.Write(false);
			setBits(IOUtils.getBits((byte)b));
			CS.Write(true);
		}

		private void setBits(bool[] bits) {  //max. 9 bits
			Array.Reverse(bits);
			var len = bits.Length;
			for (var i = 0; i < len; i++) {
				pins[i].Write(bits[i]);
			}
		}

	}

	public interface IOLEDDisplayPowerUpDown
	{
		void displayPowerOn();
		void displayPowerOff();
	}

	public class TOLEDDisplay
	{

		private IOLEDDisplayPowerUpDown hw;
		private const int _physical_width = 160;
		private const int _physical_height = 128;

		private int _row, _column, _rows, _columns, _tablength, _foreground, _background, _width, _height, _rotation;
		private bool _writing_pixels;

		private TOLEDDataBus bus;

		public TOLEDDisplay(TOLEDDataBus bus, IOLEDDisplayPowerUpDown hw) {
			this.bus = bus;
			this.hw = hw;

			_row = 0;
			_column = 0;
			_width = _physical_width;
			_height = _physical_height;
			_rotation = 0;
			_columns = _width / 8;
			_rows = _height / 8;
			_tablength = 4;
			_writing_pixels = false;
			foreground(0x00FFFFFF);
			background(0x00000000);
			init();
		}

		public void foreground(int v) {
			_foreground = v;
		}

		public void background(int v) {
			_background = v;
		}

		public void background(Color v) {
			_background = v.ToArgb();
		}

		public void orientation(int o) {
			_rotation = o & 3;

			//Set write direction
			command(0x16);
			switch (_rotation) {
				case 0:
				default:
					//HC=1, VC=1, HV=0
					data(0x76);
					_width = _physical_width;
					_height = _physical_height;
					break;
				case 1:
					//HC=0, VC=1, HV=1
					data(0x73);
					_width = _physical_height;
					_height = _physical_width;
					break;
				case 2:
					//HC=0, VC=0, HV=0
					data(0x70);
					_width = _physical_width;
					_height = _physical_height;
					break;
				case 3:
					//HC=1, VC=0, HV=1
					data(0x75);
					_width = _physical_height;
					_height = _physical_width;
					break;
			}
			_columns = _width / 8;
			_rows = _height / 8;
		}

		public void init() {
			uint i = 0, j, k;
			uint[] init_commands = {
        0x06, // Display off
        0x00,
 
        //Osc control
        //Export1 internal clock and OSC operates with external resistor
        0x02,
        0x01,
 
        //Reduce current
        0x04,
        0x00,
 
        //Clock div ratio 1: freq setting 90Hz
        0x03,
        0x30,
 
        //Iref controlled by external resistor
        0x80,
        0x00,
 
        //Precharge time R
        0x08,
        0x01,
        //Precharge time G
        0x09,
        0x01,
        //Precharge time B
        0x0A,
        0x01,
 
        //Precharge current R
        0x0B,
        0x0A,
        //Precharge current G
        0x0C,
        0x0A,
        //Precharge current B
        0x0D,
        0x0A,
 
        //Driving current R = 82uA
        0x10,
        0x52,
        //Driving current G = 56uA
        0x11,
        0x38,
        //Driving current B = 58uA
        0x12,
        0x3A,
 
        //Display mode set
        //RGB,column=0-159, column data display=Normal display
        0x13,
        0x00,
 
        //External interface mode=MPU
        0x14,
        0x01,
 
        //Memory write mode
        //6 bits triple transfer, 262K support, Horizontal address counter is increased,
        //vertical address counter is increased. The data is continuously written
        //horizontally
        0x16,
        0x76,
 
        //Memory address setting range 0x17~0x19 to width x height
        0x17,  //Column start
        0x00,
        0x18,  //Column end
        _physical_width-1,
        0x19,  //row start
        0x00,
        0x1A,  //row end
        _physical_height-1,
 
        //Memory start address set to 0x20~0x21
        0x20,  //X
        0x00,
        0x21,  //Y
        0x00,
 
        //Duty
        0x29,
        0x00,
 
        //Display start line
        0x29,
        0x00,
 
        //DDRAM read address start point 0x2E~0x2F
        0x2E,  //X
        0x00,
        0x2F,  //Y
        0x00,
 
        //Display screen saver size 0x33~0x36
        0x33,  //Screen saver columns start
        0x00,
        0x34,  //Screen saver columns end
        _physical_width-1,
        0x35,  //screen saver row start
        0x00,
        0x36,  //Screen saver row end
        _physical_height-1,
 
        //End of commands
        0xFF,
        0xFF
    };

			bus.reset();
			//_spi.format(8);
			//_spi.frequency(10000000);

			//Send initialization commands
			for (i = 0; ; i += 2) {
				j = init_commands[i];
				k = init_commands[i + 1];
				if ((j == 0xFF) && (k == 0xFF)) break;

				command(j);
				data(k);
			}

			//command(0x3b);
			//data(IOUtils.ToByteArray(new bool[] { true, true, true, true, true, true, true, true })[0]);

			//command(0x3c);
			//data(1);

			//command(0x3d);
			//data(255);

			//command(0x3e);
			//data(5);

			//command(0x3f);
			//data(5);

			//command(0x40);
			//data(5);

			//command(0x42);
			//data(255);

			cls();
			screenOn();
		}

		public void screenOn() {
			command(0x06);
			data(0x01);
		}

		public void powerOn() {
			hw.displayPowerOn();
		}

		public void powerOf() {
			hw.displayPowerOff();
		}

		public void screenOff() {
			command(0x06);
			data(0x00);
		}

		public void command(int value) {
			command((uint)value);
		}

		public void data(int value) {
			data((uint)value);
		}

		public void command(uint value) {
			_writing_pixels = (value == 0x22);
			bus.command(value);
		}

		public void data(uint value) {
			bus.data(value);
		}

		public void _window(ref int x, ref int y, ref int width, ref int height) {
			int x1, x2, y1, y2, start_x, start_y;
			switch (_rotation) {
				default:
				case 0:
					x1 = x;
					y1 = y;
					x2 = x + width - 1;
					y2 = y + height - 1;
					break;
				case 1:
					x1 = _physical_width - y - height;
					y1 = x;
					x2 = _physical_width - y - 1;
					y2 = x + width - 1;
					break;
				case 2:
					x1 = _physical_width - x - width;
					y1 = _physical_height - y - height;
					x2 = _physical_width - x - 1;
					y2 = _physical_height - y - 1;
					break;
				case 3:
					x1 = y;
					y1 = _physical_height - x - width;
					x2 = y + height - 1;
					y2 = _physical_height - x - 1;

					//x1 = y;
					//x2 = x1 + height;

					//y2 = _physical_height - x;
					//y1 = y2 - width;

					break;
			}
			//Limit values
			//if (x1 < 0) x1 = 0;
			//if (x1 >= _physical_width) x1 = _physical_width - 1;
			//if (x2 < 0) x2 = 0;
			//if (x2 >= _physical_width) x2 = _physical_width - 1;
			//if (y1 < 0) y1 = 0;
			//if (y1 >= _physical_height) y1 = _physical_height - 1;
			//if (y2 < 0) y2 = 0;
			//if (y2 >= _physical_height) y2 = _physical_height - 1;


			/*    if ((width > 100) || (height > 100))
					{
							pc1.printf("x=%d\ty=%d\twidth=%d\theight=%d\tx1=%d\tx2=%d\ty1=%d\ty2=%d\n",x,y,width,height,x1,x2,y1,y2);
					}
			*/

			command(0x19);  // Y start
			data(y1);
			command(0x1A);  // Y end
			data(y2);
			command(0x17);  // X start
			data(x1);
			command(0x18);  // x end
			data(x2);

			switch (_rotation) {
				default:
				case 0:
					start_x = x1;
					start_y = y1;
					break;
				case 1:
					start_x = x1;
					start_y = y2;
					break;
				case 2:
					start_x = x2;
					start_y = y2;
					break;
				case 3:
					//start_x = x2;
					//start_y = y1;

					start_x = x1;
					start_y = y2;

					break;
			}

			command(0x20);  // memory accesspointer x
			data(start_x);
			command(0x21);  // memory accesspointer y
			data(start_y);

			//Console.WriteLine("x1:{0} x2:{1} y1:{2} y2:{3} w:{4} h:{5} sx:{6} sy:{7}", x1, x2, y1, y2, x2 - x1, y2 - y1, start_x, start_y);
		}

		//public int _x1, _x2, _y1, _y2, _start_x, _start_y;

		//public void debug() {
		//	while (true) {
		//		cls();
		//		fillDebug(2, 1, 20, 30, Color.Red.ToArgb());
		//		try {
		//			var line = Console.ReadLine();
		//			var ar = line.Split(new char[] { ' ' });
		//			var num = int.Parse(ar[1]);
		//			if (ar[0] == "x1") _x1 = num;
		//			if (ar[0] == "x2") _x2 = num;
		//			if (ar[0] == "y1") _y1 = num;
		//			if (ar[0] == "y2") _y2 = num;
		//			if (ar[0] == "sx") _start_x = num;
		//			if (ar[0] == "sy") _start_y = num;
		//		}
		//		catch { }
		//	}
		//}

		//public void fillDebug(int x, int y, int width, int height, int colour) {
		//	int r, g, b;

		//	_windowDebug(_x1, _x2, _y1, _y2, _start_x, _start_y);
		//	//Start "write data" command if not done already
		//	if (!_writing_pixels) {
		//		command(0x22);
		//	}
		//	r = (colour & 0xFF0000) >> 16;
		//	g = (colour & 0x00FF00) >> 8;
		//	b = colour & 0xFF;

		//	var n = width * height;
		//	while (--n >= 0) {
		//		bus.rgbdot(r, g, b);
		//	}
		//	//_window(0, 0, _width, _height);
		//}

		//public void _windowDebug(int x1, int x2, int y1, int y2, int start_x, int start_y) {

		//	command(0x19);  // Y start
		//	data(y1);
		//	command(0x1A);  // Y end
		//	data(y2);
		//	command(0x17);  // X start
		//	data(x1);
		//	command(0x18);  // x end
		//	data(x2);

		//	command(0x20);  // memory accesspointer x
		//	data(start_x);
		//	command(0x21);  // memory accesspointer y
		//	data(start_y);

		//	Console.WriteLine("x1:{0} x2:{1} y1:{2} y2:{3} w:{4} h:{5} sx:{6} sy:{7}", x1, x2, y1, y2, x2 - x1, y2 - y1, start_x, start_y);
		//}

		public void cls() {
			fill(0, 0, _width, _height, _background);
			_row = 0;
			_column = 0;
		}

		public void fill(int x, int y, int width, int height, int colour) {
			int r, g, b;

			_window(ref x, ref y, ref width, ref height);
			//Start "write data" command if not done already
			if (!_writing_pixels) {
				command(0x22);
			}
			r = (colour & 0xFF0000) >> 16;
			g = (colour & 0x00FF00) >> 8;
			b = colour & 0xFF;

			var n = width * height;
			while (--n >= 0) {
				bus.rgbdot(r, g, b);
			}
			var xx = 0; var yy = 0; var ww = _width; var hh = _height;
			_window(ref xx, ref yy, ref ww, ref hh);
		}

		public void drawImage(Bitmap bmp, Point target, Point source, Size size) {
			drawImage(bmp, target.X, target.Y, source.X, source.Y, size.Width, size.Height);
		}

		public void drawImage(Bitmap bmp, int targetX, int targetY, int sourceX, int sourceY, int sizeWidth, int sizeHeight) {
			_window(ref targetX, ref targetY, ref sizeWidth, ref sizeHeight);

			if (!_writing_pixels) {
				command(0x22);
			}

			for (var y = 0; y < sizeHeight; y++) {
				for (var x = 0; x < sizeWidth; x++) {
					var c = bmp.GetPixel(x + sourceX, y + sourceY);
					//c = Color.FromArgb((int)(255 / width * ix), (int)(255 / height * iy), 0);
					bus.rgbdot(c.R, c.G, c.B);
					//pixel(ix, iy, c.ToArgb());
				}
			}

		}

		public void drawImage(int[,] bmp, Point target, Point source, Size size) {
			drawImage(bmp, target.X, target.Y, source.X, source.Y, size.Width, size.Height);
		}

		public void drawImage(int[,] bmp, int targetX, int targetY, int sourceX, int sourceY, int sizeWidth, int sizeHeight) {
			_window(ref targetX, ref targetY, ref sizeWidth, ref sizeHeight);

			if (!_writing_pixels) {
				command(0x22);
			}

			for (var y = 0; y < sizeHeight; y++) {
				for (var x = 0; x < sizeWidth; x++) {
					var c = Color.FromArgb(bmp[x + sourceX, y + sourceY]);
					//c = Color.FromArgb((int)(255 / width * ix), (int)(255 / height * iy), 0);
					bus.rgbdot(c.R, c.G, c.B);
					//pixel(ix, iy, c.ToArgb());
				}
			}

		}

		public void pixel(int x, int y, int colour) {
			var sizeWidth = 1;
			var sizeHeight = 1;
			_window(ref x, ref y, ref sizeWidth, ref sizeHeight);
			_putp(colour);
		}

		private int static_colour_prev = 0xF000000, static_r = 0, static_g = 0, static_b = 0;
		private void _putp(int colour) {

			//Start "write data" command if not done already
			if (!_writing_pixels) {
				command(0x22);
			}

			//Only calculate rgb values if colour has changed
			if (static_colour_prev != colour) {
				static_r = (colour & 0xFF0000) >> 16;
				static_g = (colour & 0x00FF00) >> 8;
				static_b = colour & 0xFF;
				static_colour_prev = colour;
			}

			bus.rgbdot(static_r, static_g, static_b);
		}

		public int width {
			get {
				return _width;
			}
		}

		public int height {
			get {
				return _height;
			}
		}


	}

}