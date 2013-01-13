using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using chess.shared;
using System.Threading;
using System.Drawing;
using RaspberryPiDotNet;
using larne.io.ic;

namespace chess.application
{

	public class TUIController
	{

		public TUIBoard uiBoard;
		public TUIDisplay display;

		public void init() {
			display = new TUIDisplay();
			display.init();
			uiBoard = new TUIBoard();

			display.gfx.Clear(Color.Black);
			display.gfx.DrawString("Bitte warten...", new Font(FontFamily.GenericSansSerif, 11), new SolidBrush(Color.White), new Point(0, 0));
			display.update();
		}

		public void drawAll() {
			display.gfx.Clear(Color.Black);
			uiBoard.update();
			display.gfx.DrawImage(uiBoard.bmp, 0, 0);
			display.update();
		}

	}

	public class TUIBoard
	{
		public Bitmap bmp;
		public Graphics gfx;
		private Bitmap background;
		private Bitmap pieces;

		public TUIBoard() {
			bmp = new Bitmap(128, 128);
			gfx = Graphics.FromImage(bmp);

			background = new Bitmap(Config.gfxPath + "board.bmp");
			pieces = new Bitmap(Config.gfxPath + "pieces16.png");
		}

		public void update() {
			gfx.DrawImage(background, 0, 0);

			Program.app.ioController.onDrawBoard(new TDrawBoardEvent() { board = this, type = EDrawBoardEventType.backgroundDrawed });

			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					var piece = Program.app.board[x, y];

					if (!piece.isEmtpy) {
						gfx.DrawImage(pieces, x * 16, y * 16, getPieceSourceRect(piece.piece), GraphicsUnit.Pixel);
					}
				}
			}

			Program.app.ioController.onDrawBoard(new TDrawBoardEvent() { board = this, type = EDrawBoardEventType.PiecesDrawed });
		}

		public Rectangle getPieceSourceRect(EPiece p) {
			int idx = (int)p - 1;
			return new Rectangle(idx * 16, 0, 16, 16);
		}

	}

	public class TUIDisplay
	{

		public Graphics gfx;
		private TOLEDDisplayAdapter adapter;
		private Bitmap bmp;
		private TOLEDDisplay lcd;

		public void init() {
			var device = new RPI();

			var SDI = device.createPin(GPIO.GPIOPins.GPIO25, GPIO.DirectionEnum.OUT, false);
			var CLK = device.createPin(GPIO.GPIOPins.GPIO08, GPIO.DirectionEnum.OUT, false);
			var CS = device.createPin(GPIO.GPIOPins.GPIO07, GPIO.DirectionEnum.OUT, false);

			var RST = device.createPin(GPIO.GPIOPins.GPIO23, GPIO.DirectionEnum.OUT, false);
			var RS = device.createPin(GPIO.GPIOPins.GPIO24, GPIO.DirectionEnum.OUT, false);

			var spi = new TSPIEmulator(SDI, null, CLK, CS);
			var watch = new System.Diagnostics.Stopwatch();

			lcd = new TOLEDDisplay(spi, RST, RS);
			lcd.background(Color.Black);

			//var bg = (Bitmap)Image.FromFile(chess.shared.Config.applicationPath + "tmp/test.bmp");

			//lcd.cls();

			adapter = new TOLEDDisplayAdapter(lcd);

			bmp = new Bitmap(160, 128);
			gfx = Graphics.FromImage(bmp);
			gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
		}

		public void update() {
			adapter.update(bmp, 0, 0, lcd.width, lcd.height);
		}

	}

}
