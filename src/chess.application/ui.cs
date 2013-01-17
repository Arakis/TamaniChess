﻿/******************************************************************************************************

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

		public int width {
			get {
				return lcd.width;
			}
		}

		public int height {
			get {
				return lcd.height;
			}
		}

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

			var bus = new TOLEDSPIFastDataBus(spi, RST, RS);
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

		public int width { get { return bmp.Width; } }
		public int height { get { return bmp.Height; } }

		public void createGraphics() {
			bmp = new Bitmap(app.ui.display.width, app.ui.display.height);
			gfx = Graphics.FromImage(bmp);
		}

		public void createGraphics(Size size) {
			bmp = new Bitmap(size.Width, size.Height);
			gfx = Graphics.FromImage(bmp);
		}

	}

	public class TUIListHandler : TUIDrawHandler
	{

		public TUIListHandler()
			: this(new Rectangle(0, 0, Program.app.ui.display.width, Program.app.ui.display.height)) {
		}

		public TUIListHandler(Rectangle rect) {
			createGraphics(rect.Size);
			this.rect = rect;
			screenRange = new TRange(0, rect.Height);
			scrollRange = new TRange(0, rect.Height);
		}

		public TUIListEntryCollection items = new TUIListEntryCollection();
		public Rectangle rect;
		public int scroll = 0;

		public TRange screenRange;
		public TRange scrollRange;

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);
			//gfx.FillRectangle(new SolidBrush(Color.Red), 0, 0, 30, 30);
			//gfx.DrawString(rand.Next().ToString(), new Font(FontFamily.GenericSansSerif, 18), new SolidBrush(Color.Blue), new Point(0, 0));

			gfx.Clear(Color.Black);

			calculateOffset();

			if (selected == null && items.Count != 0) selected = items[0];

			var offset = 0;
			for (var i = 0; i < items.Count; i++) {
				var itm = items[i];
				if (scrollRange.contains(itm.range)) {
					itm.updateGraphics();
					gfx.DrawImage(itm.bmp, 0, offset);
					gfx.DrawLine(Pens.DarkGray, 0.5f, (float)offset + (float)itm.height, (float)rect.Width, (float)offset + (float)itm.height);
					offset += itm.range.count;
				}
			}

		}

		public int border = 1;

		public void calculateOffset() {
			var offset = 0;
			foreach (var itm in items) {
				itm.range.start = offset;
				itm.range.count = itm.height + border;
				offset += itm.range.count;
			}
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.DrawImage(bmp, rect.X, rect.Y);
		}

		private TUIListEntry _selected;
		private TUIListEntry selected {
			get {
				return _selected;
			}
			set {
				if (_selected != null) _selected.selected = false;
				_selected = value;
				if (value != null) value.selected = true;
			}
		}

		public override void onButtonChanged(TButtonChangeEvent e) {
			base.onButtonChanged(e);
			if (!e.state) return;

			if (e.button == EButton.down) {
				var idx = items.IndexOf(selected) + 1;
				if (idx < items.Count) {
					selected = items[idx];
					if (!scrollRange.contains(selected.range)) {
						Console.WriteLine("scroll");
						scrollRange.start += selected.range.count;
					}
					Console.WriteLine(selected.range.start + " " + scrollRange.end);
				}
			}
			else if (e.button == EButton.up) {
				var idx = items.IndexOf(selected) - 1;
				if (idx >= 0) {
					selected = items[idx];
					if (!scrollRange.contains(selected.range)) scrollRange.start -= selected.range.count;
				}
			}

		}

	}

	public class TUIListEntryCollection : List<TUIListEntry>
	{
	}

	public class TUIListEntry
	{

		public Bitmap bmp;
		public Graphics gfx;

		public TUIListEntry(TUIListHandler list, string text) {
			this.text = text;
			bmp = new Bitmap(list.rect.Width, height);
			gfx = Graphics.FromImage(bmp);
		}

		public int height = 20;
		public string text;
		public bool selected = false;

		public TRange range = new TRange();

		public void updateGraphics() {
			var bgColor = Color.Transparent;
			var foreColor = Color.White;

			if (selected) {
				bgColor = Color.LightGray;
				foreColor = Color.Black;
			}

			gfx.Clear(bgColor);
			gfx.DrawString(text, new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(foreColor), new Point(0, 0));
		}
	}

	public class TRange
	{
		public int start;
		public int count;

		public TRange() { }

		public TRange(int start, int count) {
			this.start = start;
			this.count = count;
		}

		public int end {
			get {
				return start + count;
			}
			set {
				count = value - start;
			}
		}

		public double mid {
			get {
				return start + (count / 2);
			}
		}

		public bool isEmpty {
			get {
				return start == 0 && count == 0;
			}
		}

		public bool contains(TRange rect) {
			return start <= rect.start && start + count >= rect.start + rect.count;
		}

		public TRange inflate(int width) {
			return new TRange(this.start - width, this.count + 2 * width);
		}

		public static TRange union(TRange a, TRange b) {
			int x = Math.Min(a.start, b.start);
			int width = Math.Max(a.start + a.count, b.start + b.count);
			return new TRange(x, width - x);
		}

		public static TRange intersect(TRange a, TRange b) {
			int x = Math.Max(a.start, b.start);
			int width = Math.Min(a.start + a.count, b.start + b.count);
			if (width >= x) {
				return new TRange(x, width - x);
			}
			return new TRange();
		}

		public bool intersectsWith(TRange rect) {
			return rect.start <= this.start + this.count && this.start <= rect.start + rect.count;
		}

		public override string ToString() {
			return string.Format("start={0}, count={1}", start, count);
		}
		public TRange clone() {
			return new TRange(start, count);
		}
		public bool Equals(TRange rect) {
			return rect.start == start && rect.count == count;
		}
		public override bool Equals(object rect) {
			return Equals((TRange)rect);
		}

	}
}
