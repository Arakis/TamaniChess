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

	public abstract class THandler : IDisposable
	{

		protected TEventController ioController {
			get {
				return Program.app.ioController;
			}
		}

		private int suspendCount = 0;
		public void suspend() {
			suspendCount++;
		}

		public void resume() {
			if (suspendCount > 0)
				suspendCount--;
			else if (suspendCount < 0)
				throw new Exception("Too much resumes");
		}

		protected TEngine engine {
			get {
				return Program.app.engine;
			}
		}

		protected TApplication app {
			get {
				return Program.app;
			}
		}

		public THandler() {
		}

		public virtual void uninstall() {
			if (!installed) throw new Exception("action is already uninstalled");
			ioController.handlers.Remove(this);
			installed = false;
		}

		public bool installed = false;

		public bool active {
			get {
				return installed && suspendCount == 0;
			}
		}

		public TBoardLeds boardLeds = new TBoardLeds();

		public virtual void install() {
			install(ioController);
		}

		public virtual void install(TEventController ioController) {
			if (installed) throw new Exception("action is already installed");
			ioController.handlers.Add(this);
			installed = true;
		}

		public event DPieceChanged pieceChanged;
		public event DPiecesChanged piecesChanged;
		public event DPieceChanged pieceChangedDelay;
		public event DPiecesChanged piecesChangedDelay;

		public event DButtonChanged buttonChanged;
		public event DButtonsChanged buttonsChanged;
		public event DButtonChanged buttonChangedDelay;
		public event DButtonsChanged buttonsChangedDelay;

		public event Action processEvents;
		public event Action tick;

		public event Action<TUpdateGraphicsEvent> updateGraphics;
		public event Action<TDrawBoardEvent> drawBoard;
		public event Action<TDrawBoardStatusEvent> drawBoardStatus;
		public event Action<TDrawEvent> draw;
		public event Action<TConsoleLineEvent> consoleLine;

		public void onProcessEvents() {
			if (processEvents != null) processEvents();
		}

		public virtual void onPieceChangedDelay(TSwitchChangeEvent e) {
			if (pieceChangedDelay != null) pieceChangedDelay(e);
		}
		public virtual void onPiecesChangedDelay(TSwitchesChangesEvent e) {
			if (piecesChangedDelay != null) piecesChangedDelay(e);
		}
		public virtual void onPieceChanged(TSwitchChangeEvent e) {
			if (pieceChanged != null) pieceChanged(e);
		}
		public virtual void onPiecesChanged(TSwitchesChangesEvent e) {
			if (piecesChanged != null) piecesChanged(e);
		}

		public virtual void onButtonChangedDelay(TButtonChangeEvent e) {
			if (buttonChangedDelay != null) buttonChangedDelay(e);
		}
		public virtual void onButtonsChangedDelay(TButtonsChangesEvent e) {
			if (buttonsChangedDelay != null) buttonsChangedDelay(e);
		}
		public virtual void onButtonChanged(TButtonChangeEvent e) {
			if (buttonChanged != null) buttonChanged(e);
		}
		public virtual void onButtonsChanged(TButtonsChangesEvent e) {
			if (buttonsChanged != null) buttonsChanged(e);
		}

		public virtual void onConsoleLine(TConsoleLineEvent e) {
			if (consoleLine != null) consoleLine(e);
		}
		public virtual void onTick() {
			if (tick != null) tick();
		}

		public virtual void onUpdateGraphics(TUpdateGraphicsEvent e) {
			if (updateGraphics != null) updateGraphics(e);
		}
		public virtual void onDrawBoard(TDrawBoardEvent e) {
			if (drawBoard != null) drawBoard(e);
		}
		public virtual void onDrawBoardStatus(TDrawBoardStatusEvent e) {
			if (drawBoardStatus != null) drawBoardStatus(e);
		}
		public virtual void onDraw(TDrawEvent e) {
			if (draw != null) draw(e);
		}

		public void Dispose() {
			if (installed) uninstall();
		}

	}

	public class TUpdateGraphicsEvent
	{
	}

	public class TDrawEvent
	{

		public TDrawEvent() {
			gfx = display.gfx;
		}

		public TUIDisplay display {
			get {
				return Program.app.ui.display;
			}
		}

		public Graphics gfx;

		public void DrawString(string text) {
			gfx.DrawString(text, new Font(FontFamily.GenericSansSerif, 10), new SolidBrush(Color.White), new Point(0, 0));
		}

	}

	public class TDrawBoardEvent : TDrawEvent
	{
		public TUIBoard board;
		public EDrawBoardEventType type;
	}

	public class TDrawBoardStatusEvent : TDrawEvent
	{
	}

	public enum EDrawBoardEventType
	{
		backgroundDrawed,
		PiecesDrawed
	}

	public class TButtonChangeEvent
	{
		public bool state;
		public EButton button;
		internal bool stopped = false;

		public void stop() {
			stopped = true;
		}
	}

	public class TButtonsChangesEvent
	{
		public bool[] oldSwitches;
		public bool[] newSwitches;
	}

	public class TSwitchChangeEvent
	{
		public TPosition pos;
		public bool state;
	}

	public class TSwitchesChangesEvent
	{
		public bool[,] oldSwitches;
		public bool[,] newSwitches;
	}

	public class TConsoleLineEvent
	{
		public string line;
		public string command;
		public string[] args;
	}

	public delegate void DPieceChanged(TSwitchChangeEvent e);
	public delegate void DPiecesChanged(TSwitchesChangesEvent e);

	public delegate void DButtonChanged(TButtonChangeEvent e);
	public delegate void DButtonsChanged(TButtonsChangesEvent e);
	public delegate void DTick();

	public delegate void DConsoleLine(TConsoleLineEvent e);

	public class THandlerList : List<THandler>
	{

		public THandler findFirstByType(Type t) {
			foreach (var h in this) {
				var thisType = h.GetType();
				if (thisType == t || h.GetType().IsSubclassOf(t)) return h;
			}
			return null;
		}

		public IEnumerable<THandler> findByType(Type t) {
			foreach (var h in ToArray()) {
				var thisType = h.GetType();
				if (thisType == t || h.GetType().IsSubclassOf(t)) yield return h;
			}
		}

	}

	public class TEventController
	{

		public event DPieceChanged onPieceChangedDelay;
		public event DPiecesChanged onPiecesChangedDelay;
		public event DPieceChanged onPieceChanged;
		public event DPiecesChanged onPiecesChanged;

		public event DButtonChanged onButtonChangedDelay;
		public event DButtonsChanged onButtonsChangedDelay;
		public event DButtonChanged onButtonChanged;
		public event DButtonsChanged onButtonsChanged;
		public event DTick onTick;

		public event DConsoleLine onConsoleLine;
		public event Action onProcessEvents;

		public THandlerList handlers = new THandlerList();

		public TEventController() {
			ioHardware = new TIOHardware();
			ioHardware.init();

			onPieceChangedDelay += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onPieceChangedDelay(e);
			};

			onPiecesChangedDelay += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onPiecesChangedDelay(e);
			};

			onPieceChanged += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onPieceChanged(e);
			};

			onPiecesChanged += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onPiecesChanged(e);
			};

			//--
			onButtonChangedDelay += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onButtonChangedDelay(e);
			};

			onButtonsChangedDelay += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onButtonsChangedDelay(e);
			};

			onButtonChanged += (e) => {
				Console.WriteLine(e.button.ToString());
				foreach (var h in handlers.ToArray().Reverse())
					if (h.active) {
						h.onButtonChanged(e);
						if (e.stopped) {
							//Console.WriteLine(h.GetType().ToString());
							return;
						}
					}
			};

			onButtonsChanged += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onButtonsChanged(e);
			};

			//--
			onProcessEvents += () => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onProcessEvents();
			};

			onConsoleLine += (e) => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onConsoleLine(e);
			};

			onTick += () => {
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onTick();
			};

			ioHardware.updateSwitches();
			ioHardware.updateDelaySwitches();
		}

		public void onDrawBoard(TDrawBoardEvent e) {
			foreach (var h in handlers.ToArray())
				if (h.active)
					h.onDrawBoard(e);
		}

		public void onDrawBoardStatus(TDrawBoardStatusEvent e) {
			foreach (var h in handlers.ToArray())
				if (h.active)
					h.onDrawBoardStatus(e);
		}

		public void onDraw(TDrawEvent e) {
			foreach (var h in handlers.ToArray())
				if (h.active)
					h.onDraw(e);
		}

		public void onUpdateGraphics(TUpdateGraphicsEvent e) {
			foreach (var h in handlers.ToArray())
				if (h.active)
					h.onUpdateGraphics(e);
		}

		private TIOHardware ioHardware;

		public TOLEDDisplay getDisplay() {
			return ioHardware.lcd;
		}

		public bool loadingLED {
			get {
				return ioHardware.loadingLED;
			}
			set {
				ioHardware.loadingLED = value;
			}
		}

		public void updateLeds() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					ioHardware.ledBitArray[x, y] = false;
					for (var i = (int)EPriority.none; i < (int)EPriority.high; i++) {
						var currPrio = (EPriority)i;
						foreach (var h in handlers.ToArray()) {
							if (h.active && h.boardLeds[x, y].prio >= currPrio) {
								ioHardware.ledBitArray[x, y] = h.boardLeds[x, y].state;
							}
						}
					}
				}
			}
			ioHardware.updateLeds();
		}

		public void eventLoop() {
			while (true) {
				Thread.Sleep(50);
				processEvents();
			}
		}

		private EButton getButtonBySwitchIndex(int index) {
			switch (index) {
				case 0: return EButton.up;
				case 1: return EButton.forward;
				case 3: return EButton.down;
				case 4: return EButton.back;
				case 5: return EButton.ok;
				default: return EButton.none;
			}
		}

		private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		private void processEvents() {
			ioHardware.updateSwitches();

			if (onTick != null) onTick();

			bool changed = false;
			bool buttonsChanged = false;
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					if (ioHardware.figureSwitchesNew[x, y] != ioHardware.figureSwitchesOld[x, y]) {
						changed = true;
						if (onPieceChanged != null) onPieceChanged(new TSwitchChangeEvent() { pos = new TPosition(x, y), state = ioHardware.figureSwitchesNew[x, y] });
					}
				}
				//if (changed) break;
			}
			for (var i = 0; i < ioHardware.sideSwitchCount; i++) {
				if (ioHardware.sideSwitchesNew[i] != ioHardware.sideSwitchesOld[i]) {
					buttonsChanged = true;
					//Console.Write("#" + i.ToString() + "#");
					if (onButtonChanged != null) onButtonChanged(new TButtonChangeEvent() { state = ioHardware.sideSwitchesNew[i], button = getButtonBySwitchIndex(i) });
				}
			}

			if (changed) {
				watch.Reset();
				watch.Start();

				if (onPiecesChanged != null) {
					onPiecesChanged(new TSwitchesChangesEvent() { oldSwitches = ioHardware.figureSwitchesOld, newSwitches = ioHardware.figureSwitchesNew });
				}
			}

			const int switchDelay = 300;
			//const int switchDelay = 3000;

			if (watch.IsRunning && watch.ElapsedMilliseconds >= switchDelay) {
				watch.Stop();
				ioHardware.updateDelaySwitches();
				for (var y = 0; y < 8; y++) {
					for (var x = 0; x < 8; x++) {
						if (ioHardware.figureSwitchesNewDelay[x, y] != ioHardware.figureSwitchesOldDelay[x, y]) {
							if (onPieceChangedDelay != null) onPieceChangedDelay(new TSwitchChangeEvent() { pos = new TPosition(x, y), state = ioHardware.figureSwitchesNewDelay[x, y] });
						}
					}
				}

				if (onPiecesChangedDelay != null) {
					onPiecesChangedDelay(new TSwitchesChangesEvent() { oldSwitches = ioHardware.figureSwitchesOldDelay, newSwitches = ioHardware.figureSwitchesNewDelay });
				}
			}

			if (onConsoleLine != null)
				CommandLineThread.processEvents((e) => { onConsoleLine(e); });

			updateLeds();

			if (Program.app != null)
				Program.app.ui.drawAll();
		}

	}

	public class TBoardLeds
	{

		private TLed[,] array;

		public TBoardLeds() {
			array = new TLed[9, 9];
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					array[x, y] = new TLed();
				}
			}
		}

		public TLed this[int x, int y] {
			get {
				return array[x, y];
			}
		}

		public IEnumerable<TLed> getAllFieldLeds(TPosition pos) {
			return getAllFieldLeds(pos.x, pos.y);
		}

		public IEnumerable<TLed> getAllFieldLeds(int x, int y) {
			yield return this[x, y];
			yield return this[x + 1, y];
			yield return this[x, y + 1];
			yield return this[x + 1, y + 1];
		}

		public IEnumerable<TLed> getAllLeds() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					yield return this[x, y];
				}
			}
		}

		public void clear() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					array[x, y].off();
				}
			}
		}

		public TLed this[TPosition pos] {
			get {
				return array[pos.x, pos.y];
			}
		}

	}

	public enum EPriority
	{
		none = 0,
		low = 1,
		normal = 2,
		high = 3,
	}

	public class TLed
	{

		private int onTime;
		private int offTime;

		public EPriority prio = EPriority.none;

		public void on() {
			on(1, 0, EPriority.normal);
		}

		public void on(EPriority prio) {
			on(1, 0, prio);
		}

		public void off() {
			this.onTime = 0;
			this.offTime = 0;
			this.prio = EPriority.none;
		}

		public void on(int onTime, int offTime, EPriority prio) {
			this.onTime = onTime;
			this.offTime = offTime;
			this.prio = prio;
		}

		public bool state {
			get {
				if (blinking) {
					//calculate...
					return true;
				}
				else {
					return onTime != 0;
				}
			}
			set {
				if (value)
					on();
				else {
					off();
				}
			}
		}

		public bool blinking {
			get {
				return offTime != 0;
			}
		}

	}

	[Flags]
	public enum EButton
	{
		none = 0,
		ok = 1,
		up = 2,
		down = 4,
		back = 8,
		forward = 16,
	}

	public class TMove
	{
		public TPosition start;
		public TPosition target;
		public EPieceType pawnConversion = EPieceType.none;

		public TMove() { }

		public TMove(TPosition pos1, TPosition pos2) {
			this.start = pos1;
			this.target = pos2;
		}

		public TMove(int x1, int y1, int x2, int y2) {
			start = new TPosition(x1, y1);
			target = new TPosition(x2, y2);
		}

		public TMove(string move) {
			if (move == "(none)") return;
			start = new TPosition(move.Substring(0, 2));
			target = new TPosition(move.Substring(2, 2));
			if (move.Length > 4) pawnConversion = TChessBoard.getPieceFromChar(move.Substring(4, 1)[0]).getPieceType();
		}

		public bool isNone {
			get {
				return start == null || target == null;
			}
		}

		#region comparison

		public static bool operator ==(TMove a, TMove b) {
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b)) {
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}

			return a.start == b.start && a.target == b.target && a.pawnConversion == b.pawnConversion;
		}

		public static bool operator !=(TMove a, TMove b) {
			return !(a == b);
		}

		public override bool Equals(object obj) {
			return this == (TMove)obj;
		}

		public bool Equals(TMove obj) {
			return this == obj;
		}

		#endregion

		public static string ToString(int x1, int y1, int x2, int y2, EPieceType pawnConversion) {
			return TPosition.ToString(x1, y1) + TPosition.ToString(x2, y2) + (pawnConversion == EPieceType.none ? "" : TChessBoard.pieceTypeToChar(pawnConversion).ToString());
		}

		public static string ToString(TPosition pos1, TPosition pos2, EPieceType pawnConversion) {
			return (pos1 == null ? "--" : pos1.ToString()) + (pos2 == null ? "--" : pos2.ToString()) + (pawnConversion == EPieceType.none ? "" : TChessBoard.pieceTypeToChar(pawnConversion).ToString());
		}

		public override string ToString() {
			return ToString(start, target, pawnConversion);
		}

	}

	public class TPosition
	{

		//Current position

		public int x {
			get {
				return Program.app.board.getFlipOffset(baseX);
			}
			set {
				baseX = Program.app.board.getFlipOffset(value);
			}
		}

		public int y {
			get {
				return Program.app.board.getFlipOffset(baseY);
			}
			set {
				baseY = Program.app.board.getFlipOffset(value);
			}
		}

		public int baseX;
		public int baseY;

		#region comparison

		public static bool operator ==(TPosition a, TPosition b) {
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b)) {
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}

			return a != null && b != null && a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(TPosition a, TPosition b) {
			return !(a == b);
		}

		public override bool Equals(object obj) {
			return this == (TPosition)obj;
		}

		public bool Equals(TPosition obj) {
			return this == obj;
		}

		#endregion

		public TPosition(int x, int y) {
			this.x = x;
			this.y = y;
		}

		private TPosition() { }

		public static TPosition fromBasePosition(int baseX, int baseY) {
			return new TPosition() { baseX = baseX, baseY = baseY };
		}

		public TPosition(string position) {
			baseX = charsX.IndexOf(position[0].ToString());
			baseY = charsY.IndexOf(position[1].ToString());
		}

		const string charsX = "abcdefgh";
		const string charsY = "87654321";

		//base position
		public static string ToString(int x, int y) {
			return charsX[Program.app.board.getFlipOffset(x)].ToString() + charsY[Program.app.board.getFlipOffset(y)].ToString();
		}

		public override string ToString() {
			return ToString(x, y);
		}

		public EBoardRow row {
			get {
				return (EBoardRow)7 - baseY;
			}
		}

		public EBoardColumn column {
			get {
				return (EBoardColumn)baseX;
			}
		}

	}

	public class TPiece
	{

		public TPiece(TPosition pos, EPiece piece) {
			this._pos = pos;
			this._piece = piece;
		}

		public void clear() {
			_piece = EPiece.none;
		}

		private TPosition _pos;

		public TPosition pos {
			get {
				return _pos;
			}
		}

		public EPieceColor color {
			get {
				return _piece.getColor();
			}
		}

		public bool isOwn {
			get {
				return color == Program.app.currentColor;
			}
		}

		public bool isOpponent {
			get {
				return color != Program.app.currentColor && !isEmtpy;
			}
		}

		public bool isEmtpy {
			get {
				return piece == EPiece.none;
			}
		}

		//public bool canMove {
		//	get {
		//		return Program.app.currentColor == color;
		//	}
		//}

		public EPieceType type {
			get {
				return _piece.getPieceType();
			}
		}

		private EPiece _piece;
		public EPiece piece {
			get {
				return _piece;
			}
			set {
				_piece = value;
			}
		}

	}

	public static class Extensions
	{

		public static EPieceColor getColor(this EPiece piece) {
			if (piece == EPiece.none) return EPieceColor.none;
			if (piece >= EPiece.wPawn && piece <= EPiece.wKing) return EPieceColor.white;
			return EPieceColor.black;
		}

		public static EPieceType getPieceType(this EPiece piece) {
			if (piece == EPiece.none) return EPieceType.none;
			if (piece >= EPiece.wPawn && piece <= EPiece.wKing) return (EPieceType)piece;
			return (EPieceType)piece - 6;
		}

		public static EPiece getPiece(this EPieceType pieceType, EPieceColor color) {
			if (pieceType == EPieceType.none) return EPiece.none;
			if (color == EPieceColor.white) return (EPiece)pieceType;
			return (EPiece)pieceType + 6;
		}

		public static EPieceColor getOtherColor(this EPieceColor color) {
			if (color == EPieceColor.none) return EPieceColor.none;
			return color == EPieceColor.white ? EPieceColor.black : EPieceColor.white;
		}

		public static string toChar(this EPieceColor color) {
			if (color == EPieceColor.none) return "";
			return color == EPieceColor.white ? "w" : "b";
		}

	}

	public class TGame
	{
		public TChessBoard board = new TChessBoard(EPieceColor.white);

		public THistoryList history = new THistoryList();

		public void save(string file) {
			var dir = Path.GetDirectoryName(file);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

			var jsFile = new JObject();

			var jsHistory = new JArray();
			foreach (var b in history) {
				var js = new JObject();
				js["fen"] = b.fen;
				if (b.lastMove != null) js["lastMove"] = b.lastMove.ToString();
				js["myColor"] = b.myColor.toChar();
				jsHistory.Add(js);
			}

			jsFile["history"] = jsHistory;

			var str = jsFile.ToString();
			shared.Tools.StringSaveToFileSecure(file, str);
		}

		public bool load(string file) {
			if (!File.Exists(file)) return false;
			try {
				var str = shared.Tools.StringLoadFromFile(file);
				var js = JObject.Parse(str);
				var jsHistList = (JArray)js["history"];
				var histList = new THistoryList();
				foreach (JObject jsHist in jsHistList) {
					var fen = (string)jsHist["fen"];
					var colorStr = (string)jsHist["myColor"];
					var lastMove = (string)jsHist["lastMove"];
					var color = (colorStr == "b" ? EPieceColor.black : EPieceColor.white);

					var hist = new THistoryEntry() { fen = fen, lastMove = lastMove, myColor = color };
					histList.Add(hist);
				}
				return newGame(histList);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return false;
			}
		}

		public bool newGame(string FEN, EPieceColor myColor) {
			history.Clear();
			if (!setBoard(FEN)) return false;

			installMoveHandler();
			return true;
		}

		public bool newGame(THistoryList loadFrom) {
			var lastHist = loadFrom[loadFrom.Count - 1];
			var result = newGame(lastHist.fen, lastHist.myColor);
			if (!result) return false;
			history.Clear();
			history.AddRange(loadFrom);
			return true;
		}

		public void newGame() {
			newGame(TChessBoard.startFEN, myColor);
		}

		public void undo() {
			if (history.Count == 1) return;
			var histList = new THistoryList();
			histList.AddRange(history);
			histList.RemoveAt(histList.Count - 1);
			newGame(histList);
		}

		public bool setBoard(string fen) {
			return set(fen);
		}

		public bool makeMove(TMove move) {
			return set(board.FEN, move);
		}

		private bool set(string fen, TMove move = null) {
			//return board.setFEN(fen, move);

			var b = new TChessBoard(board.myColor);
			if (b.setFEN(fen, move)) {
				board = b;
				history.Add(THistoryEntry.fromChessBoard(b));
				return true;
			}
			return false;
		}

		public EPieceColor currentColor {
			get {
				return board.currentColor;
			}
		}

		public bool myTurn {
			get {
				return board.myTurn;
			}
		}

		public EPieceColor myColor {
			get {
				return board.myColor;
			}
		}

		public void installMoveHandler() {
			uninstallMoveHandler();

			Console.WriteLine("{0} {1} {2}", Program.app.myTurn, this.currentColor.ToString(), Program.app.myColor.ToString());

			if (Program.app.myTurn) {
				var ownHandler = new TOwnMoveHandler();
				ownHandler.install();
			}
			else {
				var calcHandler = new TCaluclateComputerMoveHandler();
				calcHandler.install();
			}
		}

		public void uninstallMoveHandler() {
			foreach (var handler in Program.app.ioController.handlers.ToArray())
				if (handler is TMoveHandler)
					handler.uninstall();
		}

	}

	public class TChessBoard
	{

		public TChessBoard(EPieceColor myColor) {
			this.myColor = myColor;

			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					board[x, y] = new TPiece(TPosition.fromBasePosition(x, y), EPiece.none);
				}
			}
		}

		public bool canSendToEngine() {
			var kingCount = 0;
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					if (this[x, y].type == EPieceType.king) kingCount++;
					if (kingCount == 2) return true;
				}
			}
			return false;
		}

		public EPieceColor currentColor;

		public EPieceColor myColor = EPieceColor.white;

		public bool myTurn {
			get {
				return currentColor == myColor;
			}
		}

		//base view
		private TPiece[,] board = new TPiece[8, 8];

		//public class TBaseView {

		//	public TBaseView(TChessBoard parent) {
		//		this.parent = parent;
		//	}

		//	private TChessBoard parent;

		//	public TPiece this[int x, int y] {
		//		get {
		//			return parent.board[x, y];
		//		}
		//		set {
		//			parent.board[x, y] = value;
		//		}
		//	}

		//}

		//current view
		public TPiece this[int x, int y] {
			get {
				return board[getFlipOffset(x), getFlipOffset(y)];
			}
			set {
				board[getFlipOffset(x), getFlipOffset(y)] = value;
			}
		}

		//current view
		public TPiece this[TPosition pos] {
			get {
				return board[getFlipOffset(pos.x), getFlipOffset(pos.y)];
			}
			set {
				board[getFlipOffset(pos.x), getFlipOffset(pos.y)] = value;
			}
		}

		public int getFlipOffset(int xy) {
			if (myColor == EPieceColor.white) return xy;
			return 7 - xy;
		}

		public string FEN;

		public void clearBoard() {
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					board[x, y].clear();
				}
			}
			castingAvailability.Clear();
			fullMoveNumber = 0;
			halfmoveClock = 0;
			enPassant = "-";
		}

		// FEN string of the initial position, normal chess
		public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		public TMove lastMove;
		public ECheckState checkState = ECheckState.none;

		public void copyTo(TChessBoard board) {
			throw new NotImplementedException();
			//TODO: Copy piece instance!
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					board.board[x, y] = this.board[x, y];
				}
			}
			board.currentColor = currentColor;
			board.FEN = FEN;
		}

		public void setFENNoValidate(string fenStr) {
			/*
				 A FEN string defines a particular position using only the ASCII character set.

				 A FEN string contains six fields separated by a space. The fields are:

				 1) Piece placement (from white's perspective). Each rank is described, starting
						with rank 8 and ending with rank 1; within each rank, the contents of each
						square are described from file A through file H. Following the Standard
						Algebraic Notation (SAN), each piece is identified by a single letter taken
						from the standard English names. White pieces are designated using upper-case
						letters ("PNBRQK") while Black take lowercase ("pnbrqk"). Blank squares are
						noted using digits 1 through 8 (the number of blank squares), and "/"
						separates ranks.

				 2) Active color. "w" means white moves next, "b" means black.

				 3) Castling availability. If neither side can castle, this is "-". Otherwise,
						this has one or more letters: "K" (White can castle kingside), "Q" (White
						can castle queenside), "k" (Black can castle kingside), and/or "q" (Black
						can castle queenside).

				 4) En passant target square (in algebraic notation). If there's no en passant
						target square, this is "-". If a pawn has just made a 2-square move, this
						is the position "behind" the pawn. This is recorded regardless of whether
						there is a pawn in position to make an en passant capture.

				 5) Halfmove clock. This is the number of halfmoves since the last pawn advance
						or capture. This is used to determine if a draw can be claimed under the
						fifty-move rule.

				 6) Fullmove number. The number of the full move. It starts at 1, and is
						incremented after Black's move.
			*/

			clearBoard();
			this.FEN = fenStr;
			var parts = fenStr.Split(' ');
			var lines = parts[0].Split('/');
			for (var lineIdx = 0; lineIdx < 8; lineIdx++) {
				var line = lines[lineIdx];
				var col = 0;
				for (var charIdx = 0; charIdx < line.Length; charIdx++) {
					var c = line[charIdx];
					if (char.IsDigit(c)) {
						col += int.Parse(c.ToString());
					}
					else {
						board[col, lineIdx].piece = getPieceFromChar(c);
						col++;
					}
				}
			}

			currentColor = parts[1] == "w" ? EPieceColor.white : EPieceColor.black;

			if (parts[2] == "-") {
				castingAvailability.Clear();
			}
			else {
				for (var i = 0; i < parts[2].Length; i++) {
					castingAvailability.Add(getPieceFromChar(parts[2][i]));
				}
			}
			enPassant = parts[3];
			halfmoveClock = int.Parse(parts[4]);
			fullMoveNumber = int.Parse(parts[5]);
		}

		public bool setFEN(string fenStr, TMove move = null) {
			string tmpFEN;
			ECheckState tmpCheck;

			var tmpBoard = new TChessBoard(myColor);
			tmpBoard.setFENNoValidate(fenStr);
			if (!tmpBoard.canSendToEngine()) {
				return false;
			}

			var moveStr = "";
			if (move != null) moveStr = move.ToString();
			var valid = Program.app.engine.validate(fenStr, out tmpFEN, out tmpCheck, moveStr);
			if (!valid) {
				return false;
			}

			setFENNoValidate(tmpFEN);
			lastMove = move;
			this.checkState = tmpCheck;

			return true;
		}

		public List<EPiece> castingAvailability = new List<EPiece>();
		public string enPassant;
		public int halfmoveClock;
		public int fullMoveNumber;

		public string toFEN() {
			var sb = new StringBuilder();
			for (var y = 0; y < 8; y++) {
				if (y != 0) sb.Append("/");
				var emptyFields = 0;
				for (var x = 0; x < 8; x++) {
					if (board[x, y].piece == EPiece.none) {
						emptyFields++;
					}
					else {
						if (emptyFields != 0) {
							sb.Append(emptyFields);
							emptyFields = 0;
						}
						sb.Append(pieceToChar(board[x, y].piece));
					}
				}
				if (emptyFields != 0) sb.Append(emptyFields);
			}

			sb.Append(" ");
			sb.Append(currentColor == EPieceColor.white ? "w" : "b");

			sb.Append(" ");
			if (castingAvailability.Count == 0) sb.Append("-");
			else
				foreach (var piece in castingAvailability)
					sb.Append(pieceToChar(piece));

			sb.Append(" ");
			sb.Append(enPassant);

			sb.Append(" ");
			sb.Append(halfmoveClock);

			sb.Append(" ");
			sb.Append(fullMoveNumber);

			return sb.ToString();
		}

		public static char pieceToChar(EPiece piece) {
			switch (piece) {
				case EPiece.bRock: return 'r';
				case EPiece.bKnight: return 'n';
				case EPiece.bBishop: return 'b';
				case EPiece.bQueen: return 'q';
				case EPiece.bKing: return 'k';
				case EPiece.bPawn: return 'p';

				case EPiece.wRock: return 'R';
				case EPiece.wKnight: return 'N';
				case EPiece.wBishop: return 'B';
				case EPiece.wQueen: return 'Q';
				case EPiece.wKing: return 'K';
				case EPiece.wPawn: return 'P';

				default: return '1';
			}
		}

		public static char pieceTypeToChar(EPieceType pieceType) {
			switch (pieceType) {
				case EPieceType.rock: return 'r';
				case EPieceType.knight: return 'n';
				case EPieceType.bishop: return 'b';
				case EPieceType.queen: return 'q';
				case EPieceType.king: return 'k';
				case EPieceType.pawn: return 'p';
			}
			throw new ArgumentException();
		}

		public static EPiece getPieceFromChar(char c) {
			switch (c) {
				case 'r': return EPiece.bRock;
				case 'n': return EPiece.bKnight;
				case 'b': return EPiece.bBishop;
				case 'q': return EPiece.bQueen;
				case 'k': return EPiece.bKing;
				case 'p': return EPiece.bPawn;

				case 'R': return EPiece.wRock;
				case 'N': return EPiece.wKnight;
				case 'B': return EPiece.wBishop;
				case 'Q': return EPiece.wQueen;
				case 'K': return EPiece.wKing;
				case 'P': return EPiece.wPawn;

				default: throw new ArgumentException("unknown piece char: " + c.ToString());
			}
		}

	}

	public class THistoryEntry
	{

		public string fen;
		public string lastMove = "";
		public EPieceColor myColor;

		public TChessBoard toChessBoard() {
			var b = new TChessBoard(myColor);
			b.setFEN(fen, lastMove == "" ? null : new TMove(lastMove));
			return b;
		}

		public static THistoryEntry fromChessBoard(TChessBoard board) {
			var entry = new THistoryEntry();
			entry.fen = board.toFEN();
			if (board.lastMove != null) entry.lastMove = board.lastMove.ToString();
			entry.myColor = board.myColor;
			return entry;
		}

	}

	public class THistoryList : List<THistoryEntry>
	{

	}

	public enum EPieceType
	{
		none = 0,

		pawn = 1, //Bauer
		knight = 2, //Springer
		bishop = 3, //Läufer
		rock = 4, //Turm
		queen = 5, //Königin
		king = 6, //König
	}

	public enum EPiece
	{
		none = 0,

		wPawn = 1, //Bauer
		wKnight = 2, //Springer
		wBishop = 3, //Läufer
		wRock = 4, //Turm
		wQueen = 5, //Königin
		wKing = 6, //König

		bPawn = 7, //Bauer
		bKnight = 8, //Springer
		bBishop = 9, //Läufer
		bRock = 10, //Turm
		bQueen = 11, //Königin
		bKing = 12, //König
	}

	public enum EBoardPosition
	{
		a1 = 0, b1 = 1, c1 = 2, d1 = 3, e1 = 4, f1 = 5, g1 = 6, h1 = 7,
		a2 = 8, b2 = 9, c2 = 10, d2 = 11, e2 = 12, f2 = 13, g2 = 14, h2 = 15,
		a3 = 16, b3 = 17, c3 = 18, d3 = 19, e3 = 20, f3 = 21, g3 = 22, h3 = 23,
		a4 = 24, b4 = 25, c4 = 26, d4 = 27, e4 = 28, f4 = 29, g4 = 30, h4 = 31,
		a5 = 32, b5 = 33, c5 = 34, d5 = 35, e5 = 36, f5 = 37, g5 = 38, h5 = 39,
		a6 = 40, b6 = 41, c6 = 42, d6 = 43, e6 = 44, f6 = 45, g6 = 46, h6 = 47,
		a7 = 48, b7 = 49, c7 = 50, d7 = 51, e7 = 52, f7 = 53, g7 = 54, h7 = 55,
		a8 = 56, b8 = 57, c8 = 58, d8 = 59, e8 = 60, f8 = 61, g8 = 62, h8 = 63,
	}

	public enum EBoardColumn
	{
		a = 0,
		b = 1,
		c = 2,
		d = 3,
		e = 4,
		f = 5,
		g = 6,
		h = 7
	}


	public enum EBoardRow
	{
		r1 = 0,
		r2 = 1,
		r3 = 2,
		r4 = 3,
		r5 = 4,
		r6 = 5,
		r7 = 6,
		r8 = 7
	}

	public enum EPieceColor
	{
		none,
		white,
		black
	}

	public enum ECheckState
	{
		none,
		check,
		mate
	}

}
