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
			if (installed) throw new Exception("action is already installed");
			ioController.handlers.Add(this);
			installed = true;
		}

		public void onProcessEvents() { }
		public virtual void onPieceChangedDelay(TSwitchChangeEvent e) { }
		public virtual void onPiecesChangedDelay(TSwitchesChangesEvent e) { }
		public virtual void onPieceChanged(TSwitchChangeEvent e) { }
		public virtual void onPiecesChanged(TSwitchesChangesEvent e) { }

		public virtual void onButtonChangedDelay(TButtonChangeEvent e) { }
		public virtual void onButtonsChangedDelay(TButtonsChangesEvent e) { }
		public virtual void onButtonChanged(TButtonChangeEvent e) { }
		public virtual void onButtonsChanged(TButtonsChangesEvent e) { }

		public virtual void onConsoleLine(TConsoleLineEvent e) { }

		public virtual void onUpdateGraphics(TUpdateGraphicsEvent e) { }
		public virtual void onDrawBoard(TDrawBoardEvent e) { }
		public virtual void onDraw(TDrawEvent e) { }

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

	}

	public class TDrawBoardEvent : TDrawEvent
	{
		public TUIBoard board;
		public EDrawBoardEventType type;
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
				foreach (var h in handlers.ToArray())
					if (h.active)
						h.onButtonChanged(e);
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

			ioHardware.updateSwitches();
			ioHardware.updateDelaySwitches();
		}

		public void onDrawBoard(TDrawBoardEvent e) {
			foreach (var h in handlers.ToArray())
				if (h.active)
					h.onDrawBoard(e);
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

		public void updateLeds() {
			for (var y = 0; y < 9; y++) {
				for (var x = 0; x < 9; x++) {
					ioHardware.ledBitArray[x, y] = false;
					for (var i = (int)EPriority.none; i < (int)EPriority.high; i++) {
						var currPrio = (EPriority)i;
						foreach (var h in handlers) {
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
				case 0: return EButton.down;
				case 1: return EButton.ok;
				case 2: return EButton.up;
				default: return EButton.none;
			}
		}

		private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		private void processEvents() {
			ioHardware.updateSwitches();

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

			lock (Program.app.consoleCommandQueue)
				while (Program.app.consoleCommandQueue.Count > 0) {
					string line = Program.app.consoleCommandQueue.Dequeue();
					if (line != null) {
						var parts = line.Split(' ');
						var args = new List<string>(parts);
						args.RemoveAt(0);
						onConsoleLine(new TConsoleLineEvent() { line = line, command = parts[0], args = args.ToArray() });
					}
				}

			updateLeds();

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
	}

	public class TIOHardware
	{

		private TPISO piso;
		private TSIPO sipo;
		private TBitMapping ledMapping;
		private TBitMapping ledMappingBottom;
		private TBitMapping ledMappingRight;
		private TBitMapping ledMappingSpecial;
		private TBitMapping outMapping;
		private TBitMapping outMappingBugFix;
		private TBitMapping sideMapping;

		public bool[,] figureSwitchesOld = new bool[8, 8];
		public bool[,] figureSwitchesNew = new bool[8, 8];

		public bool[,] figureSwitchesOldDelay = new bool[8, 8];
		public bool[,] figureSwitchesNewDelay = new bool[8, 8];

		public int sideSwitchCount = 16;

		public bool[] sideSwitchesOld;
		public bool[] sideSwitchesNew;

		public bool[] sideSwitchesOldDelay;
		public bool[] sideSwitchesNewDelay;

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

			sipo = new TSIPO(namedPins["SER"], namedPins["OE"], namedPins["RCLK"], namedPins["SRCLK"], namedPins["SRCLR"]);
			sipo.setBits(new int[200]); //clear all
			piso = new TPISO(namedPins["O7"], namedPins["PL"], namedPins["CP"]);

			ledMapping = new TBitMapping(8);
			ledMapping.setMapping(0, 3, 7, 6, 1, 2, 4, 5);
			ledMappingBottom = new TBitMapping(8);
			ledMappingBottom.setMapping(0, 3, 7, 6, 1, 2, 4, 5);
			ledMappingRight = new TBitMapping(8);
			ledMappingRight.setMapping(0, 3, 7, 6, 1, 2, 4, 5);
			ledMappingSpecial = new TBitMapping(8);
			ledMappingSpecial.setMapping(0, 3, 7, 6, 1, 2, 4, 5);

			outMapping = new TBitMapping(8);
			outMapping.setMapping(4, 0, 1, 2, 5, 6, 7, 3);
			outMappingBugFix = new TBitMapping(8);
			outMappingBugFix.setMapping(4, 0, 1, 2, 5, 7, 6, 3);

			sideMapping = new TBitMapping(8);
			sideMapping.setMapping(7, 0, 1, 2, 3, 4, 5, 6);

			sideSwitchesOld = new bool[sideSwitchCount];
			sideSwitchesNew = new bool[sideSwitchCount];
			sideSwitchesOldDelay = new bool[sideSwitchCount];
			sideSwitchesNewDelay = new bool[sideSwitchCount];
		}

		public void updateSwitches() {
			figureSwitchesOld = figureSwitchesNew;
			figureSwitchesNew = new bool[8, 8];

			piso.load();
			readFieldSwitchBits(0, 0, figureSwitchesNew);
			readFieldSwitchBits(0, 2, figureSwitchesNew);
			readFieldSwitchBits(0, 4, figureSwitchesNew, outMappingBugFix);
			readFieldSwitchBits(0, 6, figureSwitchesNew);
			readFieldSwitchBits(4, 6, figureSwitchesNew);
			readFieldSwitchBits(4, 4, figureSwitchesNew);
			readFieldSwitchBits(4, 2, figureSwitchesNew);
			readFieldSwitchBits(4, 0, figureSwitchesNew);

			sideSwitchesOld = sideSwitchesNew;
			sideSwitchesNew = new bool[sideSwitchCount];
			readSideSwitchBits(0, 8, sideSwitchesNew);
		}

		private List<bool> oldLedBits = new List<bool>();
		public void updateLeds() {
			var tmpBits = new bool[8];

			//set led-pins
			var bitList = new List<bool>();
			if (ledBitArray[8, 8]) tmpBits[0] = true;
			bitList.InsertRange(0, ledMappingBottom.convert(tmpBits));

			tmpBits = new bool[8];
			for (var i = 0; i < 8; i++) {
				tmpBits[i] = ledBitArray[8, i];
			}
			bitList.InsertRange(0, ledMappingBottom.convert(tmpBits));

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
			start = new TPosition(move.Substring(0, 2));
			target = new TPosition(move.Substring(2, 2));
			if (move.Length > 4) pawnConversion = TChessBoard.getPieceFromChar(move.Substring(4, 1)[0]).getPieceType();
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
		public int x;
		public int y;

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

		public TPosition(string position) {
			x = charsX.IndexOf(position[0].ToString());
			y = charsY.IndexOf(position[1].ToString());
		}

		const string charsX = "abcdefgh";
		const string charsY = "87654321";

		public static string ToString(int x, int y) {
			return charsX[x].ToString() + charsY[y].ToString();
		}

		public override string ToString() {
			return ToString(x, y);
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

	}

	public class TChessBoard
	{

		public TChessBoard() {
			for (var y = 0; y < 8; y++) {
				for (var x = 0; x < 8; x++) {
					board[x, y] = new TPiece(new TPosition(x, y), EPiece.none);
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

		private TPiece[,] board = new TPiece[8, 8];

		public TPiece this[int x, int y] {
			get {
				return board[x, y];
			}
			set {
				board[x, y] = value;
			}
		}

		public TPiece this[TPosition pos] {
			get {
				return board[pos.x, pos.y];
			}
			set {
				board[pos.x, pos.y] = value;
			}
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

		public void newGame() {
			newGame(startFEN);
		}

		public void newGame(string FEN) {
			setFEN(FEN);

			foreach (var h in Program.app.ioController.handlers.ToArray()) {
				if (h is TMoveHandler) h.uninstall();
			}

			var ownHandler = new TOwnMoveHandler();
			ownHandler.install();
		}

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

		public void setFEN(string fenStr) {
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
					if (this[x, y].piece == EPiece.none) {
						emptyFields++;
					}
					else {
						if (emptyFields != 0) {
							sb.Append(emptyFields);
							emptyFields = 0;
						}
						sb.Append(pieceToChar(this[x, y].piece));
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

	public enum EPieceColor
	{
		none,
		white,
		black
	}

	public class TOLEDDisplayAdapter
	{

		private TOLEDDisplay dsp;
		public TOLEDDisplayAdapter(TOLEDDisplay dsp) {
			this.dsp = dsp;
			deviceImage = new int[dsp.width, dsp.height];
		}

		private int[,] deviceImage;

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

			*set0 = RSMask;
			
			*set0 = CSMask;
			if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			*set1 = CSMask;

			*set1 = RSMask;
		}

		public override void data(uint value) {
			var SDIMask = this.SDIMask;
			var SCKMask = this.SCKMask;

			*set0 = CSMask;
			if ((value & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((value & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			*set1 = CSMask;
		}

		private bool[] tmp8Bits = new bool[8];
		private bool[] tmp6Bits = new bool[6];

		public override void rgbdot(int r, int g, int b) {
			var set0 = GPIOMem.ClrAddr;
			var set1 = GPIOMem.SetAddr;

			var SDIMask = this.SDIMask;
			var SCKMask = this.SCKMask;

			*set0 = CSMask;
			if ((r & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((r & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((r & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((r & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((r & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((r & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((r & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((r & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			*set1 = CSMask;

			*set0 = CSMask;
			if ((g & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((g & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((g & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((g & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((g & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((g & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((g & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((g & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			*set1 = CSMask;

			*set0 = CSMask;
			if ((b & (1 << 7)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((b & (1 << 6)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((b & (1 << 5)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((b & (1 << 4)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((b & (1 << 3)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			if ((b & (1 << 2)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((b & (1 << 1)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			//if ((b & (1 << 0)) != 0) *set1 = SDIMask; else *set0 = SDIMask; *set1 = SCKMask; *set0 = SCKMask;
			*set1 = CSMask;
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

	public class TOLEDDisplay
	{


		private const int _physical_width = 160;
		private const int _physical_height = 128;

		private int _row, _column, _rows, _columns, _tablength, _foreground, _background, _width, _height, _rotation;
		private bool _writing_pixels;

		private TOLEDDataBus bus;

		public TOLEDDisplay(TOLEDDataBus bus) {
			this.bus = bus;

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
			reset();
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

		public void reset() {
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
 
        //Display ON
        0x06,
        0x01,
 
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

		public void _window(int x, int y, int width, int height) {
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
					break;
			}
			//Limit values
			if (x1 < 0) x1 = 0;
			if (x1 >= _physical_width) x1 = _physical_width - 1;
			if (x2 < 0) x2 = 0;
			if (x2 >= _physical_width) x2 = _physical_width - 1;
			if (y1 < 0) y1 = 0;
			if (y1 >= _physical_height) y1 = _physical_height - 1;
			if (y2 < 0) y2 = 0;
			if (y2 >= _physical_height) y2 = _physical_height - 1;


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
					start_x = x2;
					start_y = y1;
					break;
			}

			command(0x20);  // memory accesspointer x
			data(start_x);
			command(0x21);  // memory accesspointer y
			data(start_y);
		}

		public void cls() {
			fill(0, 0, _width, _height, _background);
			_row = 0;
			_column = 0;
		}

		public void fill(int x, int y, int width, int height, int colour) {
			int r, g, b;

			_window(x, y, width, height);
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
			_window(0, 0, _width, _height);
		}

		public void drawImage(Bitmap bmp, Point target, Point source, Size size) {
			_window(target.X, target.Y, size.Width, size.Height);

			if (!_writing_pixels) {
				command(0x22);
			}

			for (var y = 0; y < size.Height; y++) {
				for (var x = 0; x < size.Width; x++) {
					var c = bmp.GetPixel(x + source.X, y + source.Y);
					//c = Color.FromArgb((int)(255 / width * ix), (int)(255 / height * iy), 0);
					bus.rgbdot(c.R, c.G, c.B);
					//pixel(ix, iy, c.ToArgb());
				}
			}

		}

		public void drawImage(int[,] bmp, Point target, Point source, Size size) {
			_window(target.X, target.Y, size.Width, size.Height);

			if (!_writing_pixels) {
				command(0x22);
			}

			for (var y = 0; y < size.Height; y++) {
				for (var x = 0; x < size.Width; x++) {
					var c = Color.FromArgb(bmp[x + source.X, y + source.Y]);
					//c = Color.FromArgb((int)(255 / width * ix), (int)(255 / height * iy), 0);
					bus.rgbdot(c.R, c.G, c.B);
					//pixel(ix, iy, c.ToArgb());
				}
			}

		}

		public void pixel(int x, int y, int colour) {
			_window(x, y, 1, 1);
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
