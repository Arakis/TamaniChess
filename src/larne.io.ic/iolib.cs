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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace larne.io.ic
{

	public static class IOUtils
	{
		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_delayMicroseconds")]
		public static extern bool microSleep(uint micros);

		public static bool[] getBits(byte b) {
			//var bits = new bool[8];
			//for (int i = 0; i < 8; i++) {
			//	bits[i] = (b & 0x80) != 0;
			//	b *= 2;
			//}
			//return bits;

			//var bits = new bool[8];
			//for (int i = 0; i < 8; i++) {
			//	bits[7-i] = (b & 1) == 1;
			//	b >>= 1;
			//}
			//return bits;

			var bits = new bool[8];
			bits[0] = (b & (1 << 0)) != 0;
			bits[1] = (b & (1 << 1)) != 0;
			bits[2] = (b & (1 << 2)) != 0;
			bits[3] = (b & (1 << 3)) != 0;
			bits[4] = (b & (1 << 4)) != 0;
			bits[5] = (b & (1 << 5)) != 0;
			bits[6] = (b & (1 << 6)) != 0;
			bits[7] = (b & (1 << 7)) != 0;
			return bits;
		}

		public static void getBits(byte b, bool[] bits) {
			bits[0] = (b & (1 << 0)) != 0;
			bits[1] = (b & (1 << 1)) != 0;
			bits[2] = (b & (1 << 2)) != 0;
			bits[3] = (b & (1 << 3)) != 0;
			bits[4] = (b & (1 << 4)) != 0;
			bits[5] = (b & (1 << 5)) != 0;
			bits[6] = (b & (1 << 6)) != 0;
			bits[7] = (b & (1 << 7)) != 0;
		}

		public static bool[] get6Bits(byte b) {
			var bits = new bool[8];
			bits[0] = (b & (1 << 0)) != 0;
			bits[1] = (b & (1 << 1)) != 0;
			bits[2] = (b & (1 << 2)) != 0;
			bits[3] = (b & (1 << 3)) != 0;
			bits[4] = (b & (1 << 4)) != 0;
			bits[5] = (b & (1 << 5)) != 0;
			return bits;
		}

		//public static bool[] getBits(byte b, int count) {
		//	var bits = new bool[count];
		//	for (int i = 0; i < count; i++) {
		//		bits[i] = (b & 1) == 1;
		//		b >>= 1;
		//	}
		//	return bits;
		//}

		//public static byte[] ToByteArray(IEnumerable<bool> bits) {
		//	var bitArray = bits.ToArray();
		//	int numBytes = bitArray.Length / 8;
		//	if (bitArray.Length % 8 != 0) numBytes++;

		//	byte[] bytes = new byte[numBytes];
		//	int byteIndex = 0, bitIndex = 0;

		//	for (int i = 0; i < bitArray.Length; i++) {
		//		if (bitArray[i])
		//			bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

		//		bitIndex++;
		//		if (bitIndex == 8) {
		//			bitIndex = 0;
		//			byteIndex++;
		//		}
		//	}

		//	return bytes;
		//}

	}

	public class TSIPO
	{

		private GPIO SER;
		private GPIO OE;
		private GPIO RCLK;
		private GPIO SRCLK;
		private GPIO SRCLR;

		public TSIPO(GPIO SER, GPIO OE, GPIO RCLK, GPIO SRCLK, GPIO SRCLR) {
			this.SER = SER;
			this.OE = OE;
			this.RCLK = RCLK;
			this.SRCLK = SRCLK;
			this.SRCLR = SRCLR;
		}

		private bool lastBit;
		private bool lastBitUndefined = true;
		public void setBits(IEnumerable<bool> bits, bool flush = true) {
			//foreach (var bit in bits) Console.Write(bit ? "1" : "0"); Console.WriteLine();

			if (SRCLR != null) {
				SRCLR.Write(true);
				wait();
			}
			if (OE != null) {
				OE.Write(false);
				wait();
			}

			foreach (var bit in bits.Reverse()) {
				//foreach (var bit in bits) {
				if (lastBit != bit || lastBitUndefined) {
					SER.Write(bit);
					lastBit = bit;
					lastBitUndefined = false;
				}
				wait();
				SRCLK.Write(true);
				wait();
				SRCLK.Write(false);
				wait();
			}

			if (flush) this.flush();
		}

		public void flush() {
			RCLK.Write(true);
			wait();
			RCLK.Write(false);
			wait();
		}

		public void setBits(IEnumerable<int> bits, bool flush = true) {
			var list = new List<bool>();
			foreach (var bit in bits) {
				list.Add(bit == 0 ? false : true);
			}
			setBits(list.ToArray(), flush);
		}

		private void wait() {
			IOUtils.microSleep(100);
			//System.Threading.Thread.Sleep(10);
		}

	}

	public class TPISO
	{

		private GPIO O7;
		private GPIO PL;
		private GPIO CP;

		public TPISO(GPIO O7, GPIO PL, GPIO CP) {
			this.O7 = O7;
			this.PL = PL;
			this.CP = CP;
		}

		public void load() {
			PL.Write(true);
			IOUtils.microSleep(100);
			PL.Write(false);
			IOUtils.microSleep(100);
		}

		public IEnumerable<bool> readBits(int count, bool load = true) {
			var list = new List<bool>();

			if (load) this.load();

			for (var i = 0; i < count; i++) {
				CP.Write(false);
				IOUtils.microSleep(100);
				list.Add(O7.Read());
				IOUtils.microSleep(100);
				CP.Write(true);
				IOUtils.microSleep(100);
			}

			return list;
		}

	}

	public abstract class Device
	{

		public abstract GPIO createPin(GPIOPins pin);
		public abstract GPIO createPin(GPIOPins pin, GPIODirection direction);
		public abstract GPIO createPin(GPIOPins pin, GPIODirection direction, bool initialValue);

	}

	public class RPI : Device
	{

		public override GPIO createPin(GPIOPins pin) {
			return new GPIOMem(pin);
		}

		public override GPIO createPin(GPIOPins pin, GPIODirection direction) {
			return new GPIOMem(pin, direction);
		}

		public override GPIO createPin(GPIOPins pin, GPIODirection direction, bool initialValue) {
			return new GPIOMem(pin, direction, initialValue);
		}

	}

	public class TNamedPins : Dictionary<string, GPIO>
	{

	}

	//public class TLogicalPins
	//{
	//  public GPIO this[int idx] {
	//    get {
	//      return null;
	//    }
	//    set { }
	//  }
	//}

	public class TSIPOArray
	{
		private TSIPO sipo;
		public int length;
		public bool[] bits;

		public TSIPOArray(TSIPO sipo, int length) {
			this.sipo = sipo;
			this.length = length;
			this.bits = new bool[length];
		}

		public void setBits(IEnumerable<bool> bits) {
			var i = 0;
			foreach (var bit in bits) {
				this.bits[i] = bit;
				i++;
			}
		}

		public void setBits(IEnumerable<bool> bits, int offset) {
			var i = offset;
			foreach (var bit in bits) {
				this.bits[i] = bit;
				i++;
			}
		}

		public void flush() {
			sipo.setBits(bits);
		}

	}

	public class TBitMapping
	{

		public int[] mapping;
		private int length;

		public TBitMapping(int length) {
			this.length = length;
			mapping = new int[length];
		}

		public void setMapping(int source, int dest) {
			mapping[source] = dest;
		}

		public void setMapping(params int[] mapping) {
			setMapping((IEnumerable<int>)mapping);
		}

		public void setMapping(IEnumerable<int> mapping) {
			var i = 0;
			foreach (var dest in mapping) {
				this.mapping[i] = dest;
				i++;
			}
		}

		public IEnumerable<bool> convert(IEnumerable<bool> bits) {
			var resultBits = new bool[length];
			var i = 0;
			foreach (var bit in bits) {
				var idx = mapping[i];
				if (idx >= 0)
					resultBits[idx] = bit;
				i++;
			}
			return resultBits;
		}

	}

	public abstract class TSPIDevice
	{
		public void writeByte(byte value) {
			writeBits(IOUtils.getBits(value));
		}

		public void writeByte(int value) {
			writeBits(IOUtils.getBits((byte)value));
		}

		public void writeByte(uint value) {
			writeBits(IOUtils.getBits((byte)value));
		}

		public byte readByte() {
			throw new NotImplementedException();
		}

		public abstract void writeBits(bool[] bits);
		public abstract IEnumerable<bool> readBits();

	}

	public class TSPIFile : TSPIDevice
	{

		private GPIO CS;

		public TSPIFile(int bus, int chipSelect, GPIO CS = null)
			: this(string.Format("/dev/spidev{0}.{1}", bus, chipSelect), CS) {
		}

		public TSPIFile(string device, GPIO CS = null) {
			this.device = device;
			this.CS = CS;
			if (CS != null) CS.Write(true);
		}

		private string device;

		public override void writeBits(bool[] bits) {
			throw new NotImplementedException();
			//var bytes = IOUtils.ToByteArray(bits);
			//if (CS != null) CS.Write(false);
			//using (var s = System.IO.File.OpenWrite(device)) {
			//	foreach (var b in bytes) {
			//		s.WriteByte(b);
			//	}
			//}
			//if (CS != null) CS.Write(true);
		}

		public override IEnumerable<bool> readBits() {
			throw new NotImplementedException();
		}

	}

	public class TSPIEmulator : TSPIDevice
	{

		public GPIO SDI;
		public GPIO SDO;
		public GPIO SCK;
		public GPIO CS;

		public TSPIEmulator(GPIO SDI, GPIO SDO, GPIO SCK, GPIO CS) {
			this.SDI = SDI;
			this.SDO = SDO;
			this.SCK = SCK;
			this.CS = CS;
			CS.Write(true); //ensure it's true
			SCK.Write(false);
		}

		public override void writeBits(bool[] bits) {
			//testFunc(bits);
			//testFunc2(bits);

			CS.Write(false);

			var n = bits.Length;
			while (--n >= 0) {
				SDI.Write(bits[n]);

				SCK.Write(true);
				SCK.Write(false);
			}

			CS.Write(true);
		}

		public override IEnumerable<bool> readBits() {
			throw new NotImplementedException();
		}

	}

	public class TSPI_BCM : TSPIDevice
	{

		private GPIO CS;

		public TSPI_BCM(GPIO CS = null) {
			//this.CS = CS;
			//GPIOMem.Initialize();

			//BCM2835Native.bcm2835_spi_begin();
			//BCM2835Native.bcm2835_spi_setBitOrder(BCM2835Native.BCM2835_SPI_BIT_ORDER_MSBFIRST);      // The default
			//BCM2835Native.bcm2835_spi_setDataMode(BCM2835Native.BCM2835_SPI_MODE0);                   // 0, 3
			//BCM2835Native.bcm2835_spi_setClockDivider(BCM2835Native.BCM2835_SPI_CLOCK_DIVIDER_16); // The default
			//BCM2835Native.bcm2835_spi_chipSelect(BCM2835Native.BCM2835_SPI_CS_NONE);                      // The default
			////BCM2835Native.bcm2835_spi_setChipSelectPolarity(BCM2835Native.BCM2835_SPI_CS0, BCM2835Native.LOW); // the default
			//CS.Write(true);
		}

		//private int cnt;
		public override void writeBits(bool[] bits) {
			////System.Threading.Thread.Sleep(1);
			//CS.Write(false);
			//var bytes = IOUtils.ToByteArray(bits);
			//foreach (var b in bytes) {
			//	//if (cnt++ < 20) Console.WriteLine(b);
			//	BCM2835Native.bcm2835_spi_transfer(b);
			//}
			////System.Threading.Thread.Sleep(1);
			//CS.Write(true);
		}

		public override IEnumerable<bool> readBits() {
			throw new NotImplementedException();
		}

	}

	public static class SPITest
	{
		public static void main() {
			//GPIOMem.Initialize();

			//BCM2835Native.bcm2835_spi_begin();
			//BCM2835Native.bcm2835_spi_setBitOrder(BCM2835Native.BCM2835_SPI_BIT_ORDER_MSBFIRST);      // The default
			//BCM2835Native.bcm2835_spi_setDataMode(BCM2835Native.BCM2835_SPI_MODE0);                   // The default
			//BCM2835Native.bcm2835_spi_setClockDivider(BCM2835Native.BCM2835_SPI_CLOCK_DIVIDER_256); // The default
			//BCM2835Native.bcm2835_spi_chipSelect(BCM2835Native.BCM2835_SPI_CS0);                      // The default
			//BCM2835Native.bcm2835_spi_setChipSelectPolarity(BCM2835Native.BCM2835_SPI_CS0, BCM2835Native.LOW);      // the default

			//// Send a byte to the slave and simultaneously read a byte back from the slave
			//// If you tie MISO to MOSI, you should read back what was sent
			//int data = BCM2835Native.bcm2835_spi_transfer(0x23);
			//Console.WriteLine("Read from SPI: {0}", data);

			//BCM2835Native.bcm2835_spi_end();
			//BCM2835Native.bcm2835_close();
		}
	}

	public static class BCM2835Native
	{

		//Low, high
		public const uint LOW = 0x0;
		public const uint HIGH = 0x1;

		//bcm2835SPIBitOrder
		public const uint BCM2835_SPI_BIT_ORDER_MSBFIRST = 1;
		public const uint BCM2835_SPI_BIT_ORDER_LSBFIRST = 0;

		//bcm2835SPIMode
		public const uint BCM2835_SPI_MODE0 = 0;
		public const uint BCM2835_SPI_MODE1 = 1;
		public const uint BCM2835_SPI_MODE2 = 2;
		public const uint BCM2835_SPI_MODE3 = 3;

		//bcm2835SPIChipSelect
		public const uint BCM2835_SPI_CS0 = 0;
		public const uint BCM2835_SPI_CS1 = 1;
		public const uint BCM2835_SPI_CS2 = 2;
		public const uint BCM2835_SPI_CS_NONE = 3;

		//bcm2835SPIClockDivider
		public const uint BCM2835_SPI_CLOCK_DIVIDER_65536 = 0;       //< 65536 = 262.144us = 3.814697260kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_32768 = 32768;   //< 32768 = 131.072us = 7.629394531kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_16384 = 16384;   //< 16384 = 65.536us = 15.25878906kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_8192 = 8192;    //< 8192 = 32.768us = 30/51757813kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_4096 = 4096;    //< 4096 = 16.384us = 61.03515625kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_2048 = 2048;    //< 2048 = 8.192us = 122.0703125kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_1024 = 1024;    //< 1024 = 4.096us = 244.140625kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_512 = 512;     //< 512 = 2.048us = 488.28125kHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_256 = 256;     //< 256 = 1.024us = 976.5625MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_128 = 128;     //< 128 = 512ns = = 1.953125MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_64 = 64;      //< 64 = 256ns = 3.90625MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_32 = 32;      //< 32 = 128ns = 7.8125MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_16 = 16;      //< 16 = 64ns = 15.625MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_8 = 8;       //< 8 = 32ns = 31.25MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_4 = 4;       //< 4 = 16ns = 62.5MHz
		public const uint BCM2835_SPI_CLOCK_DIVIDER_2 = 2;       //< 2 = 8ns = 125MHz, fastest you can get
		public const uint BCM2835_SPI_CLOCK_DIVIDER_1 = 1;       //< 0 = 262.144us = 3.814697260kHz, same as 0/65536

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_begin")]
		public static extern void bcm2835_spi_begin();

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_setBitOrder")]
		public static extern void bcm2835_spi_setBitOrder(uint order);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_setDataMode")]
		public static extern void bcm2835_spi_setDataMode(uint mode);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_setClockDivider")]
		public static extern void bcm2835_spi_setClockDivider(uint divider);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_chipSelect")]
		public static extern void bcm2835_spi_chipSelect(uint cs);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_setChipSelectPolarity")]
		public static extern void bcm2835_spi_setChipSelectPolarity(uint cs, uint active);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_transfer")]
		public static extern int bcm2835_spi_transfer(uint data);

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_spi_end")]
		public static extern void bcm2835_spi_end();

		[DllImport("libbcm2835.so", EntryPoint = "bcm2835_close")]
		public static extern void bcm2835_close();

	}

	public static class IONative
	{

		[DllImport("libc.so.6")]
		public static extern int getpid();

	}

}
