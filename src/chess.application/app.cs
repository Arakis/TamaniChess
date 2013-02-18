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
using Newtonsoft.Json.Linq;

//using Gst;
//using Gst.GLib;

namespace chess.application
{

	public class TApplication
	{
		public TEventController ioController;
		public TEngine engine;

		public void clearBoard() {
			game.board.clearBoard();
			//engine.position(board.toFEN());
		}

		public EPieceColor currentColor {
			get {
				return game.currentColor;
			}
		}

		public EPieceColor myColor {
			get {
				return game.myColor;
			}
		}

		public bool myTurn {
			get {
				return game.myTurn;
			}
		}

		public TAudioPlayer player;
		public TGame game = new TGame();

		public TChessBoard board {
			get {
				return game.board;
			}
		}

		public TUIController ui;

		public void start() {
			try {
				Console.WriteLine("chess application initialization");

				Console.CancelKeyPress += (sender, e) => {
					e.Cancel = true;
					//quit();
				};

				ioController = new TEventController();

				var initHandler = new TInitHandler();
				initHandler.install();

				ui = new TUIController();
				ui.init();

				player = new TAudioPlayer();
				player.load("sound1", Config.soundPath + "sound1.wav");
				player.load("sound2", Config.soundPath + "sound2.wav");
				player.load("sound3", Config.soundPath + "sound3.wav");
				player.load("silence", Config.soundPath + "silence.wav");
				player.load("loaded", Config.soundPath + "loaded.wav");
				player.load("silence500ms", Config.soundPath + "silence500ms.wav");
				player.play("silence500ms");

				engine = new TEngine();
				engine.start();
				//return;
				//board.newGame("k7/8/8/8/8/7p/8/K7 w - - 0 1");
				//board.newGame("k7/7P/8/8/8/8/8/K7 w - - 0 1");
				//board.newGame();
				if (!loadAutoSave()) {
					game.newGame();
				}

				var cmdHandler = new TConsoleHandler();
				cmdHandler.install();

				var dbgHandler = new TDebugHandler();
				dbgHandler.install();

				var boardHandler = new TUIBoard();
				boardHandler.install();

				var statusHandler = new TUIBoardStatusHandler();
				statusHandler.install();

				var acthandler = new TUIDefaultButtonActions();
				acthandler.install();

				var screenSaver = new TScreenSaverHandler();
				screenSaver.install();

				ui.drawAll();

				initHandler.uninstall();

				Console.WriteLine("ready");
				player.play("loaded");
				ioController.eventLoop();
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				Thread.Sleep(5000);
			}
		}

		public void quit() {
			Console.WriteLine("quit");
			ioController.handlers.Clear();
			saveAutoSave();
			ui.powerOff();
			ioController.loadingLED = true;
			ioController.updateLeds();
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}

		public static string saveDirectory = Path.Combine(Config.applicationPath, "save");
		public static string autosaveFile = Path.Combine(saveDirectory, "autosave.json");

		public bool loadAutoSave() {
			return game.load(autosaveFile);
		}

		public void saveAutoSave() {
			game.save(autosaveFile);
		}

	}

	public static class CommandLineThread
	{

		private static Thread th;
		public static Queue<string> consoleCommandQueue = new Queue<string>();

		public static void processEvents(DConsoleLine cb) {
			if (th == null) start();

			List<string> commands;
			lock (consoleCommandQueue) {
				commands = new List<string>(consoleCommandQueue);
				consoleCommandQueue.Clear();
			}

			foreach (var line in commands) {
				if (line != null) {
					var parts = line.Split(' ');
					var args = new List<string>(parts);
					args.RemoveAt(0);
					cb(new TConsoleLineEvent() { line = line, command = parts[0], args = args.ToArray() });
				}
			}
		}

		public static void start() {
			th = new Thread(() => {
				while (true) {
					var line = Console.ReadLine();
					lock (consoleCommandQueue)
						consoleCommandQueue.Enqueue(line);
				}
			});
			th.Start();
		}

	}

}
