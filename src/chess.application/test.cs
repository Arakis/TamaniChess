using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaspberryPiDotNet;
using larne.io.ic;
using System.IO;
using System.Threading;
using System.Drawing;

namespace chess.application
{

	public class TIOTest
	{

		public void start() {

			var bits = larne.io.ic.IOUtils.getBits(1);
			foreach (var bit in bits) Console.Write(bit ? 1 : 0);
			Console.WriteLine();
			var bytes = larne.io.ic.IOUtils.ToByteArray(bits);
			foreach (var b in bytes) Console.WriteLine(b);
			Console.WriteLine();

			//SPITest.main();
			//while (true) { 
			//}
			//Console.WriteLine(IONative.getpid());
			var device = new RPI();

			//var SDI = device.createPin(GPIO.GPIOPins.GPIO10, GPIO.DirectionEnum.OUT, false);
			//var CLK = device.createPin(GPIO.GPIOPins.GPIO11, GPIO.DirectionEnum.OUT, false);
			//var CS = device.createPin(GPIO.GPIOPins.GPIO27, GPIO.DirectionEnum.OUT, false);

			//var RST = device.createPin(GPIO.GPIOPins.GPIO04, GPIO.DirectionEnum.OUT, false);
			//var RS = device.createPin(GPIO.GPIOPins.GPIO17, GPIO.DirectionEnum.OUT, false);

			var SDI = device.createPin(GPIO.GPIOPins.GPIO25, GPIO.DirectionEnum.OUT, false);
			var CLK = device.createPin(GPIO.GPIOPins.GPIO08, GPIO.DirectionEnum.OUT, false);
			var CS = device.createPin(GPIO.GPIOPins.GPIO07, GPIO.DirectionEnum.OUT, false);

			var RST = device.createPin(GPIO.GPIOPins.GPIO23, GPIO.DirectionEnum.OUT, false);
			var RS = device.createPin(GPIO.GPIOPins.GPIO24, GPIO.DirectionEnum.OUT, false);


			var spi = new TSPIEmulator(SDI, null, CLK, CS);
			//var spi = new TSPIFile(0, 1, CS);
			//var spi = new TSPI_BCM(CS);
			var rnd = new Random();
			var watch = new System.Diagnostics.Stopwatch();

			var lcd = new TOLEDDisplay(spi, RST, RS);
			lcd.background(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)));

			var bg = (Bitmap)Image.FromFile(chess.shared.Config.applicationPath + "tmp/test.bmp");

			//lcd.drawImage(bmp, 0, 0, bmp.Width, bmp.Height);
			lcd.cls();

			var adapter = new TOLEDDisplayAdapter(lcd);
			adapter.update(bg, 0, 0, lcd.width, lcd.height);

			var bmp = new Bitmap(160, 128);
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
				gfx.DrawImage(bg, 0, 0);

				st.Add(st[0]);
				st.RemoveAt(0);
				//gfx.DrawString(string.Join("\n", st.ToArray()), new Font(FontFamily.GenericSansSerif, 11), new SolidBrush(Color.DarkBlue), new PointF(0, 0));

				gfx.DrawString("Schach", new Font(FontFamily.GenericSansSerif, 18), new SolidBrush(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))), new PointF(rnd.Next(120) - 10, rnd.Next(130) - 10));

				watch.Restart();
				adapter.update(bmp, 0, 0, lcd.width, lcd.height);
				Console.WriteLine(watch.ElapsedMilliseconds);
				System.Threading.Thread.Sleep(2000);
			}
			//while (true) {
			//lcd.background(Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255)));
			//lcd.cls();


			////Test colour and fill
			//lcd.fill(0, 50, lcd.width, 10, 0x00FF00);
			//lcd.fill(50, 0, 10, lcd.height, 0xFF0000);
			////Test pixel writing
			//for (int i = 0; i != lcd.width; i++) {
			//	lcd.pixel(i, 80 + (int)(Math.Sin(i / 5.0) * 10), 0x000000);
			//}

			//}
			Console.ReadLine();
		}

	}

}
