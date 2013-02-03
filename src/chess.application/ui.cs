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
using System.Drawing.Drawing2D;
using RaspberryPiDotNet;
using larne.io.ic;

namespace chess.application
{

	public class TUIController
	{

		public TUIBoard uiBoard;
		public TUIDisplay display;

		public void saveScreenShot() {
			var destFolder = Path.Combine(Config.applicationPath, "screenshots");
			if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
			var n = 0;
			foreach (var fileInfo in new DirectoryInfo(destFolder).GetFiles()) {
				var ar = fileInfo.Name.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
				var num = 0;
				if (ar.Length == 2 && int.TryParse(ar[1], out num)) {
					n = Math.Max(n, num);
				}
			}
			display.saveScreenshot(Path.Combine(destFolder, "image-" + (n + 1).ToString() + ".png"));
		}

		public void init() {
			display = new TUIDisplay();
			display.init();
			uiBoard = new TUIBoard();

			using (var bmp = Bitmap.FromFile(Path.Combine(Config.gfxPath, "init.png"))) {
				display.gfx.DrawImage(bmp, 0, 0);
			}
			display.gfx.DrawString("Lade Daten...", new Font(FontFamily.GenericSansSerif, 8), new SolidBrush(Color.White), new Point(0, 140));
			display.update();
		}

		public void drawAll() {
			display.gfx.Clear(Color.Black);
			Program.app.ioController.onUpdateGraphics(new TUpdateGraphicsEvent());
			Program.app.ioController.onDraw(new TDrawEvent());
			display.update();
		}

		public void powerOff() {
			display.gfx.Clear(Color.Black);
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

		public void saveScreenshot(string file) {
			bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
		}

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
			lcd.orientation(3);
			lcd.background(Color.Black);

			//var bg = (Bitmap)Image.FromFile(chess.shared.Config.applicationPath + "tmp/test.bmp");

			//lcd.cls();

			adapter = new TOLEDDisplayAdapter(lcd);

			bmp = new Bitmap(lcd.width, lcd.height);
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
			currentItems = items;
			createGraphics(rect.Size);
			this.rect = rect;
			screenRange = new TRange(0, rect.Height);
			scrollRange = new TRange(0, rect.Height);
		}

		public TUIListEntryCollection items = new TUIListEntryCollection();
		private TUIListEntryCollection currentItems = null;

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

			if (selected == null && currentItems.Count != 0) selected = currentItems[0];

			var offset = 0;
			for (var i = 0; i < currentItems.Count; i++) {
				var itm = currentItems[i];
				if (scrollRange.contains(itm.range)) {
					itm.updateGraphics();
					gfx.DrawImage(itm.bmp, 0, offset);
					gfx.DrawLine(new Pen(Color.FromArgb(70, 70, 70)), 0.5f, (float)offset + (float)itm.height, (float)rect.Width, (float)offset + (float)itm.height);
					offset += itm.range.count;
				}
			}

		}

		public event Action<TUIListEntry> onSelected;

		public int border = 1;

		public void calculateOffset() {
			var offset = 0;
			foreach (var itm in currentItems) {
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

		public void setCurrnetItems(TUIListEntryCollection items) {
			if (items == this.currentItems) return;
			currentItems = items;
			selected = (items.Count == 0 ? null : items[0]);
		}

		public override void onButtonChanged(TButtonChangeEvent e) {
			base.onButtonChanged(e);
			if (!e.state) return;

			if (e.button == EButton.down) {
				e.stop();
				var idx = currentItems.IndexOf(selected) + 1;
				if (idx < currentItems.Count) {
					selected = currentItems[idx];
					if (!scrollRange.contains(selected.range)) {
						Console.WriteLine("scroll");
						scrollRange.start += selected.range.count;
					}
					Console.WriteLine(selected.range.start + " " + scrollRange.end);
				}
			}
			else if (e.button == EButton.up) {
				e.stop();
				var idx = currentItems.IndexOf(selected) - 1;
				if (idx >= 0) {
					selected = currentItems[idx];
					if (!scrollRange.contains(selected.range)) scrollRange.start -= selected.range.count;
				}
			}
			else if (e.button == EButton.ok) {
				e.stop();
				if (selected != null) selected.selectedEvent();
			}

		}

		public void selectedEvent(TUIListEntry itm) {
			if (onSelected != null) onSelected(itm);
		}

	}

	public class TUIListEntryCollection : List<TUIListEntry>
	{
	}

	public class TUIListEntry
	{

		public Bitmap bmp;
		public Graphics gfx;
		public object tag;
		protected TUIListHandler list;
		public Action onSelected;

		public TUIListEntry(TUIListHandler list, string text, Action onSelected = null) {
			this.list = list;
			this.text = text;
			this.onSelected = onSelected;
			bmp = new Bitmap(list.rect.Width, height);
			gfx = Graphics.FromImage(bmp);
		}

		public int height = 18;
		public string text;
		public bool selected = false;

		public TRange range = new TRange();

		public virtual void updateGraphics() {
			var bgColor = Color.Transparent;
			var foreColor = Color.White;

			if (selected) {
				bgColor = Color.LightGray;
				foreColor = Color.Black;
			}

			gfx.Clear(bgColor);
			gfx.DrawString(text, new Font(FontFamily.GenericSansSerif, 10), new SolidBrush(foreColor), new Point(0, 0));
		}

		public virtual void selectedEvent() {
			if (onSelected != null) onSelected();
			list.selectedEvent(this);
		}

	}

	public class TUIListSubEntry : TUIListEntry
	{

		public TUIListSubEntry(TUIListHandler list, string text, Action onSelected = null)
			: base(list, text, onSelected) {
		}

		public TUIListEntryCollection items = new TUIListEntryCollection();

		public override void updateGraphics() {
			base.updateGraphics();

			//gfx.DrawRectangle(Pens.Red, 0, 0, 10, 10);
			//gfx.FillPath(Brushes.Red, new GraphicsPath(new Point()
			var x = list.rect.Width - 5 - 1;
			var y = 5;
			gfx.FillPolygon(Brushes.Red, new Point[] { new Point(0 + x, 0 + y), new Point(5 + x, 5 + y), new Point(0 + x, 10 + y) });
			//gfx.DrawString("\u25B6", new Font(FontFamily.GenericSansSerif, 11,), new SolidBrush(Color.Red), new Point(list.rect.Width - 10, 0));

		}

		public override void selectedEvent() {
			list.setCurrnetItems(items);
			base.selectedEvent();
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

	public class TUIChoosePawnConversion : TUIDrawHandler
	{

		private TUIListHandler list;

		public TUIChoosePawnConversion(Action<EPieceType> cb) {
			createGraphics();
			list = new TUIListHandler(new Rectangle(0, 20, Program.app.ui.display.width, Program.app.ui.display.height - 20));
			list.items.Add(new TUIListEntry(list, "Dame") { tag = EPieceType.queen });
			list.items.Add(new TUIListEntry(list, "Turm") { tag = EPieceType.rock });
			list.items.Add(new TUIListEntry(list, "Läufer") { tag = EPieceType.bishop });
			list.items.Add(new TUIListEntry(list, "Springer") { tag = EPieceType.knight });

			list.onSelected += (itm) => {
				cb((EPieceType)itm.tag);
			};
		}

		public override void install() {
			base.install();
			list.install();
		}

		public override void uninstall() {
			base.uninstall();
			list.uninstall();
		}

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);
			gfx.Clear(Color.Black);
			gfx.DrawString("Bitte wählen:", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), new SolidBrush(Color.White), new Point(0, 0));
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.DrawImage(bmp, 0, 0);
		}

	}

	public class TUIChooseFigure : TUIDrawHandler
	{

		private TUIListHandler list;
		public EPieceColor color = EPieceColor.white;

		private TUIListEntry colorItem;

		public TUIChooseFigure(Action<EPiece> cb) {
			createGraphics();
			list = new TUIListHandler(new Rectangle(0, 18, Program.app.ui.display.width, Program.app.ui.display.height - 18));

			list.items.Add(colorItem = new TUIListEntry(list, "") { tag = EPieceType.pawn });

			list.items.Add(new TUIListEntry(list, "Bauer") { tag = EPieceType.pawn });
			list.items.Add(new TUIListEntry(list, "Springer") { tag = EPieceType.knight });
			list.items.Add(new TUIListEntry(list, "Läufer") { tag = EPieceType.bishop });
			list.items.Add(new TUIListEntry(list, "Turm") { tag = EPieceType.rock });
			list.items.Add(new TUIListEntry(list, "Dame") { tag = EPieceType.queen });
			list.items.Add(new TUIListEntry(list, "König") { tag = EPieceType.king });

			setColor(color);

			list.onSelected += (itm) => {
				if (itm == colorItem) {
					setColor(color.getOtherColor());
				}
				else {
					var pt = (EPieceType)itm.tag;
					cb(pt.getPiece(color));
				}
			};
		}

		public void setColor(EPieceColor color) {
			this.color = color;
			colorItem.text = "Farbe: " + (color == EPieceColor.white ? "weiss" : "schwarz");
		}

		public override void install() {
			base.install();
			list.install();
		}

		public override void uninstall() {
			base.uninstall();
			list.uninstall();
		}

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);
			gfx.Clear(Color.Black);
			gfx.DrawString("Bitte wählen:", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), new SolidBrush(Color.White), new Point(0, 0));
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.DrawImage(bmp, 0, 0);
		}

	}

	public class TUIMainMenu : TUIDrawHandler
	{

		private TUIListHandler list;

		public TUIMainMenu() {
			createGraphics();
			list = new TUIListHandler(new Rectangle(0, 18, Program.app.ui.display.width, Program.app.ui.display.height - 18));
			list.items.Add(new TUIListEntry(list, "Neues Spiel", () => {
				app.board.newGame();
				app.engine.newGame();
				uninstall();
			}));
			list.items.Add(new TUIListEntry(list, "Rückgängig"));
			list.items.Add(new TUIListEntry(list, "Tipp"));

			var subEntry = new TUIListSubEntry(list, "Spiel bearbeiten", () => { title = "Spiel bearbeiten"; });
			list.items.Add(subEntry);

			subEntry.items.Add(new TUIListEntry(list, "Verschieben/Entf.", movePieces));
			subEntry.items.Add(new TUIListEntry(list, "Hinzufügen", setPieces));

		}

		public override void onButtonChanged(TButtonChangeEvent e) {
			base.onButtonChanged(e);
			if (e.state && e.button == EButton.back) uninstall();
		}

		private void setPieces() {
			uninstall();

			foreach (var oldHandler in ioController.handlers.findByType(typeof(TChangeBoardHandler)))
				oldHandler.uninstall();

			TUIChooseFigure chooseHandler = null;
			chooseHandler = new TUIChooseFigure((p) => {
				var setHandler = new TSetPieceHandler(p);
				setHandler.install();
				chooseHandler.uninstall();

				setHandler.buttonChanged += (e) => {
					if (e.state && (e.button == EButton.ok || e.button == EButton.back)) {
						e.stop();
						setHandler.uninstall();
						chooseHandler.install();
					}
				};
			});
			chooseHandler.buttonChanged += (e) => {
				if (e.state && e.button == EButton.back) {
					chooseHandler.uninstall();
					app.board.installMoveHandler();
				}
			};
			chooseHandler.install();

			app.board.uninstallMoveHandler();
		}

		private void movePieces() {
			uninstall();

			foreach (var oldHandler in ioController.handlers.findByType(typeof(TChangeBoardHandler)))
				oldHandler.uninstall();

			var setHandler = new TMovePieceHandler();
			setHandler.install();

			setHandler.buttonChanged += (e) => {
				if (e.state && (e.button == EButton.ok || e.button == EButton.back)) {
					e.stop();
					setHandler.uninstall();
					app.board.installMoveHandler();
				}
			};

			app.board.uninstallMoveHandler();
		}

		public override void install() {
			base.install();
			list.install();
		}

		public override void uninstall() {
			base.uninstall();
			list.uninstall();
		}

		private string title = "Hauptmenü";
		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);
			gfx.Clear(Color.Black);
			gfx.DrawString(title, new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), new SolidBrush(Color.White), new Point(0, 0));
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.DrawImage(bmp, 0, 0);
		}

	}

	public class TUIBoardStatusHandler : TUIDrawHandler
	{

		public TUIBoardStatusHandler() {
			//current = this;
			createGraphics(new Size(this.app.ui.display.width, 32));
		}

		//public static TUIBoardStatusHandler current;

		//public string text;

		public override void onUpdateGraphics(TUpdateGraphicsEvent e) {
			base.onUpdateGraphics(e);

			gfx.Clear(Color.Transparent);


			//foreach(var h in ioController.handlers)
			//	if(h is tui

			//gfx.DrawString(text, new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), new SolidBrush(Color.White), new Point(0, 0));
		}

		public override void onDraw(TDrawEvent e) {
			base.onDraw(e);
			e.gfx.DrawImage(bmp, 0, 128);
		}

	}

	public class TUIDefaultButtonActions : THandler
	{

		private TUIListHandler list;

		public override void onButtonChanged(TButtonChangeEvent e) {
			base.onButtonChanged(e);
			e.stop();

			if (e.button == EButton.ok && e.state) {
				var menu = new TUIMainMenu();
				menu.install();
			}

		}

	}

}
