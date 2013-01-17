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
			Program.app.ioController.onUpdateGraphics(new TUpdateGraphicsEvent());
			Program.app.ioController.onDraw(new TDrawEvent());
			display.update();
		}

	}

	public class TUIBoard : TUIDrawHandler
	{
		private Bitmap background;
		private Bitmap pieces;

		public TUIBoard() {
			bmp = new Bitmap(128, 128);
			gfx = Graphics.FromImage(bmp);

			background = new Bitmap(Config.gfxPath + "board.bmp");
			pieces = new Bitmap(Config.gfxPath + "pieces16.png");
		}

		public Rectangle getPieceSourceRect(EPiece p) {
			int idx = (int)p - 1;
			return new Rectangle(idx * 16, 0, 16, 16);
		}

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);

			gfx.DrawImage(background, 0, 0);

			Program.app.ioController.onDrawBoard(new TDrawBoardEvent() { board = this, gfx = gfx, type = EDrawBoardEventType.backgroundDrawed });

			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					var piece = Program.app.board[x, y];

					if (!piece.isEmtpy) {
						gfx.DrawImage(pieces, x * 16, y * 16, getPieceSourceRect(piece.piece), GraphicsUnit.Pixel);
					}
				}
			}

			Program.app.ioController.onDrawBoard(new TDrawBoardEvent() { board = this, gfx = gfx, type = EDrawBoardEventType.PiecesDrawed });
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);

			e.gfx.DrawImage(bmp, 0, 0);
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

			var SDI = device.createPin(GPIOPins.V2_GPIO_25, GPIODirection.Out, false);
			var CLK = device.createPin(GPIOPins.V2_GPIO_08, GPIODirection.Out, false);
			var CS = device.createPin(GPIOPins.V2_GPIO_07, GPIODirection.Out, false);

			var RST = device.createPin(GPIOPins.V2_GPIO_23, GPIODirection.Out, false);
			var RS = device.createPin(GPIOPins.V2_GPIO_24, GPIODirection.Out, false);

			var spi = new TSPIEmulator(SDI, null, CLK, CS);
			var watch = new System.Diagnostics.Stopwatch();

			var bus = new TOLEDSPIDataBus(spi, RST, RS);
			lcd = new TOLEDDisplay(bus);
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

	public class TUIDrawHandler : THandler
	{
		protected Graphics gfx;
		protected Bitmap bmp;
	}

	public class TUIListHandler : TUIDrawHandler
	{

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);
			//e.board.gfx.FillRectangle(new SolidBrush(Color.Red), 0, 0, 30, 30);
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.FillRectangle(new SolidBrush(Color.Red), 0, 0, 30, 30);
		}

	}

}
