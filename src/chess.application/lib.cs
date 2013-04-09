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
				foreach (var h in handlers.ToArray().Reverse())
					if (h.active) {
						h.onButtonChanged(e);
						if (e.stopped) {
							Console.WriteLine(h.GetType().ToString());
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

	public class TIOHardware : IOLEDDisplayPowerUpDown
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
			readFieldSwitchBits(0, 0, figureSwitchesNew);
			readFieldSwitchBits(0, 2, figureSwitchesNew);
			readFieldSwitchBits(0, 4, figureSwitchesNew, outMappingBugFix);
			readFieldSwitchBits(0, 6, figureSwitchesNew);
			readFieldSwitchBits(4, 6, figureSwitchesNew);
			readFieldSwitchBits(4, 4, figureSwitchesNew);
			readFieldSwitchBits(4, 2, figureSwitchesNew);
			readFieldSwitchBits(4, 0, figureSwitchesNew);
		}

		public bool loadingLED = true;
		public bool ScreenICOn;
		public bool ScreenLightOn;

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

		public EBoardRow row {
			get {
				return (EBoardRow)7 - y;
			}
		}

		public EBoardColumn column {
			get {
				return (EBoardColumn)x;
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
		public TChessBoard board = new TChessBoard();

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
				var fen = (string)js["fen"];
				var colorStr = (string)js["myColor"];
				var color = (colorStr == "b" ? EPieceColor.black : EPieceColor.white);
				return newGame(fen, color);
			}
			catch {
				return false;
			}
		}

		public bool newGame(string FEN, EPieceColor myColor) {
			history.Clear();
			if (!setBoard(FEN)) return false;

			installMoveHandler();
			return true;
		}

		public void newGame() {
			newGame(TChessBoard.startFEN, myColor);
		}

		public bool setBoard(string fen) {
			return set(fen);
		}

		public bool makeMove(TMove move) {
			return set(board.FEN, move);
		}

		private bool set(string fen, TMove move = null) {
			//return board.setFEN(fen, move);

			var b = new TChessBoard();
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
				var calcHandler = new TCaluclateMoveHandler();
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

		public EPieceColor myColor = EPieceColor.white;

		public bool myTurn {
			get {
				return currentColor == myColor;
			}
		}

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

		public TMove lastMove;
		public bool check = false;
		public bool checkMate = false;

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
			bool tmpCheck;

			var tmpBoard = new TChessBoard();
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
			this.check = tmpCheck;

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

	public class THistoryEntry
	{

		public string fen;
		public string lastMove = "";
		public EPieceColor myColor;

		public TChessBoard toChessBoard() {
			var b = new TChessBoard();
			b.setFEN(fen, lastMove == "" ? null : new TMove(lastMove));
			b.myColor = myColor;
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
