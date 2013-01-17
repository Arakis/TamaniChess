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

//using Gst;
//using Gst.GLib;

namespace chess.application
{

	public class TApplication
	{
		public TEventController ioController;
		public TEngine engine;

		public void clearBoard() {
			board.clearBoard();
			//engine.position(board.toFEN());
		}

		public EPieceColor currentColor {
			get {
				return board.currentColor;
			}
			set {
				board.currentColor = value;
			}
		}
		public EPieceColor myColor = EPieceColor.white;

		public Queue<string> consoleCommandQueue = new Queue<string>();

		public bool myTurn {
			get {
				return currentColor == myColor;
			}
		}

		public TAudioPlayer player;
		public TChessBoard board = new TChessBoard();
		public TCommandLineThread cmdThread;
		public TUIController ui;

		public void start() {
			try {
				Console.WriteLine("chess application initialization");

				ioController = new TEventController();

				var initHandler = new TInitHandler();
				initHandler.install();

				ui = new TUIController();
				ui.init();

				player = new TAudioPlayer();
				player.load("sound1", Config.soundPath + "sound1.wav");
				player.load("sound2", Config.soundPath + "sound2.wav");
				player.load("sound3", Config.soundPath + "sound3.wav");

				engine = new TEngine();
				engine.start();

				//board.newGame("k7/8/8/8/8/7p/8/K7 w - - 0 1");
				//board.newGame("k7/7P/8/8/8/8/8/K7 b - - 0 1");
				board.newGame();

				cmdThread = new TCommandLineThread();
				cmdThread.start();

				var cmdHandler = new TConsoleHandler();
				cmdHandler.install();

				var dbgHandler = new TDebugHandler();
				dbgHandler.install();

				var boardHandler = new TUIBoard();
				boardHandler.install();

				ui.drawAll();

				initHandler.uninstall();

				Console.WriteLine("ready");
				ioController.eventLoop();
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				Thread.Sleep(5000);
			}
		}

		//public void start() {
		//	Console.WriteLine("chess application initialization");

		//	boardController = new TIOBoardController();
		//	boardController.leds[0, 0].on();
		//	boardController.leds[8, 0].on();
		//	boardController.leds[0, 8].on();
		//	boardController.leds[8, 8].on();
		//	boardController.updateLeds();

		//	System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;

		//	player = new TAudioPlayer();
		//	player.load("sound1", "/home/pi/app/chess.application/sounds/sound1.wav");
		//	player.load("sound2", "/home/pi/app/chess.application/sounds/sound2.wav");
		//	player.load("sound3", "/home/pi/app/chess.application/sounds/sound3.wav");

		//	var board = new TChessBoard();
		//	//var boardTemp = new TChessBoard();

		//	engine = new TEngine();
		//	engine.start();

		//	//new game
		//	board.newGame();
		//	//board.copyTo(boardTemp);
		//	changes.Clear();

		//	TMove calculatedMove = null;

		//	while (true) {
		//		System.Threading.Thread.Sleep(50);

		//		if (changed) {
		//			Console.WriteLine();
		//			Console.WriteLine("--------");
		//			Console.WriteLine();

		//			//Print switches
		//			for (var y = 0; y < 8; y++) {
		//				for (var x = 0; x < 8; x++) {
		//					Console.Write(ioBoard.figureSwitchesNew[x, y] ? "1" : "0");
		//				}
		//				Console.WriteLine();
		//			}
		//			Console.WriteLine();

		//			//Print figures
		//			for (var y = 0; y < 8; y++) {
		//				for (var x = 0; x < 8; x++) {
		//					var fig = board.board[x, y];
		//					Console.Write(TChessBoard.figureToChar(fig));
		//				}
		//				Console.WriteLine();
		//			}
		//			Console.WriteLine();
		//		}

		//		//clean up changes
		//		//beim Schlagen wird ein Stein wieder abgesetzt
		//		while (changes.Count >= 1 && changes[0].state) changes.RemoveAt(0);

		//		if (changed && changes.Count >= 2) {
		//			//Console.WriteLine(changes.Count);
		//			var c = changes[0];
		//			//Console.WriteLine(c.x.ToString() + " " + c.y.ToString());
		//			//var fig = boardTemp.board[c.x, c.y];
		//			//boardTemp.board[c.x, c.y] = EColoredFigure.none;
		//			var c2 = changes[1];
		//			//Console.WriteLine(c2.x.ToString() + " " + c2.y.ToString());
		//			//boardTemp.board[c2.x, c2.y] = fig;

		//			string newFEN;
		//			if (engine.validateMove(board.FEN, TMove.ToString(c.x, c.y, c2.x, c2.y), out newFEN)) {
		//				player.play("sound3");
		//				board.setFEN(newFEN);
		//				//board.copyTo(boardTemp);

		//				engine.go((m) => {
		//					Console.WriteLine("MOVE: " + m);
		//					calculatedMove = new TMove(m);
		//					player.play("sound2");
		//				});
		//			}
		//			else {
		//				Console.WriteLine("ERROR");
		//			}

		//			changes.Clear();
		//		}

		//		if (calculatedMove != null) {
		//			ioBoard.setAllFieldLeds(calculatedMove.pos1);
		//			ioBoard.setAllFieldLeds(calculatedMove.pos2);
		//		}

		//		foreach (var c in changes) ioBoard.setAllFieldLeds(c.x, c.y);

		//		ioBoard.updateLeds();

		//	}
		//}

	}

	public class TCommandLineThread
	{

		private Thread th;

		public void start() {
			th = new Thread(() => {
				while (true) {
					var line = Console.ReadLine();
					lock (Program.app.consoleCommandQueue)
						Program.app.consoleCommandQueue.Enqueue(line);
				}
			});
			th.Start();
		}

	}

}
