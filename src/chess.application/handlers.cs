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

	public class TInitHandler : THandler
	{

		public override void install() {
			base.install();
			boardLeds[0, 0].on(EPriority.high);
			boardLeds[8, 0].on(EPriority.high);
			boardLeds[0, 8].on(EPriority.high);
			boardLeds[8, 8].on(EPriority.high);
			ioController.updateLeds();
		}

	}

	public class TConsoleHandler : THandler
	{

		public override void onConsoleLine(TConsoleLineEvent e) {
			base.onConsoleLine(e);
			switch (e.command) {
				case "newgame":
					app.board.newGame();
					app.engine.newGame();
					break;
				case "d":
					app.engine.debug();
					break;
				case "k":
					System.Diagnostics.Process.GetCurrentProcess().Kill();
					break;
				case "setpiece":
					foreach (var oldHandler in ioController.handlers.findByType(typeof(TChangeBoardHandler)))
						oldHandler.uninstall();

					var h = new TSetPieceHandler(TChessBoard.getPieceFromChar((char)e.args[0][0]));
					h.install();

					foreach (var handler in app.ioController.handlers)
						if (handler is TMoveHandler)
							handler.suspend();

					break;
				case "screenshot":
					Program.app.ui.saveScreenShot();
					break;
				case "clearboard":
					app.clearBoard();
					//engine.debug();
					break;
			}
		}

	}

	public class TChangeBoardHandler : THandler { }

	public class TRemovePieceHandler : TChangeBoardHandler { }

	public class TMovePieceHandler : TChangeBoardHandler { }

	public class TSetPieceHandler : TChangeBoardHandler
	{

		public EPiece piece;

		public TSetPieceHandler(EPiece piece) {
			this.piece = piece;
		}

		public override void onPieceChangedDelay(TSwitchChangeEvent e) {
			base.onPieceChangedDelay(e);
			app.board[e.pos].piece = piece;

			if (app.board.canSendToEngine()) {
				app.engine.position(app.board.toFEN());
				engine.debug();
			}
		}

		public override void onConsoleLine(TConsoleLineEvent e) {
			base.onConsoleLine(e);
			if (e.line == "") {
				Console.WriteLine("set pieces done");
				uninstall();

				foreach (var handler in app.ioController.handlers)
					if (handler is TMoveHandler)
						handler.resume();

			}
		}


	}

	public abstract class TMoveHandler : THandler
	{

		protected void highlightPosition(TDrawBoardEvent e, TPosition pos) {
			if (pos != null)
				e.gfx.FillRectangle(new SolidBrush(Color.Red), pos.x * 16, pos.y * 16, 16, 16);
		}

		public TMove tmpMove;
		public TMove showMove;

		public TPosition start { get { return tmpMove.start; } set { tmpMove.start = value; } }
		public TPosition target { get { return tmpMove.target; } set { tmpMove.target = value; } }

		public override void onDrawBoard(TDrawBoardEvent e) {
			base.onDrawBoard(e);

			if (e.type == EDrawBoardEventType.backgroundDrawed) {
				highlightPosition(e, showMove.start);
				highlightPosition(e, showMove.target);
			}

		}

		protected virtual void moveDone() { }

	}

	//public abstract class TMoveHandler2 : THandler
	//{



	//}

	public class TOwnMoveHandler : TMoveHandler
	{

		public TOwnMoveHandler() {
			tmpMove = new TMove();
			showMove = tmpMove;
		}

		public override void install() {
			base.install();
		}

		public override void onPieceChanged(TSwitchChangeEvent e) {
			Console.WriteLine(e.pos.ToString() + e.state.ToString());
			base.onPieceChanged(e);
			boardLeds.clear();
			if ((!e.state && !app.board[e.pos].isEmtpy) || (e.state && app.board[e.pos].isEmtpy))
				foreach (var led in boardLeds.getAllFieldLeds(e.pos)) led.on();

			//Weil der Schalter beim schlagen u.U. zu kurz offen ist
			if (start != null && app.board[e.pos].isOpponent) {
				target = e.pos;

				//moveDone();
			}
		}

		public override void onPieceChangedDelay(TSwitchChangeEvent e) {
			base.onPieceChangedDelay(e);

			if (app.board[e.pos].isOpponent && !e.state) target = e.pos;
			if (app.board[e.pos].isEmtpy && e.state) target = e.pos;

			//Gegnerischer Stein wurde wieder abgesetzt
			if (app.board[e.pos].isOpponent && e.state && start == null) target = null;

			//Eigenen Stein wurde wieder abgesetzt
			if (app.board[e.pos].isOwn && e.state && target == null) start = null;

			if (app.board[e.pos].isOwn && !e.state) start = e.pos;

			//boardLeds.clear();
			//if (start != null) foreach (var led in boardLeds.getAllFieldLeds(start)) led.on();
			//if (target != null) foreach (var led in boardLeds.getAllFieldLeds(target)) led.on();
		}

		public override void onPiecesChangedDelay(TSwitchesChangesEvent e) {
			base.onPiecesChangedDelay(e);

			if (start != null && target != null && !e.newSwitches[start.x, start.y] && e.newSwitches[target.x, target.y]) {
				moveDone();
			}

		}

		private TUIChoosePawnConversion pawnConvert;

		protected override void moveDone() {
			base.moveDone();

			if (pawnConvert != null) {
				pawnConvert.uninstall();
				pawnConvert = null;
			}

			string newFEN;
			bool isCheck;

			var startPiece = app.board[tmpMove.start];
			if (startPiece.type == EPieceType.pawn && tmpMove.pawnConversion == EPieceType.none) {
				Console.WriteLine("Pawn conversion needed! ***");
				if ((startPiece.color == EPieceColor.white && startPiece.pos.y == 1) || (startPiece.color == EPieceColor.black && startPiece.pos.y == 6)) {
					Console.WriteLine("Pawn conversion needed!");
					pawnConvert = new TUIChoosePawnConversion((type) => {
						tmpMove.pawnConversion = type;
						moveDone();
					});
					pawnConvert.install();
					return;
				}
			}

			if (engine.validateMove(app.board.FEN, tmpMove.ToString(), out newFEN, out isCheck)) {
				app.player.play("sound3");
				app.board.setFEN(newFEN);
				//board.copyTo(boardTemp);

				boardLeds.clear();

				engine.go((m) => {
					Console.WriteLine("MOVE: " + m);
					uninstall();

					var handler = new TComputerMoveHandler(new TMove(m));
					handler.install();
					app.player.play("sound2");
				});
			}
			else {
				Console.WriteLine("INVALID MOVE");
				tmpMove.pawnConversion = EPieceType.none;
			}

		}

	}

	public class TComputerMoveHandler : TMoveHandler
	{

		// up fast up fast down
		// up slow up fast down
		// up fast up
		// up slow up

		public TMove move;

		public TComputerMoveHandler(TMove move) {
			this.tmpMove = new TMove();
			showMove = move;
			this.move = move;
		}

		public override void install() {
			base.install();
			foreach (var led in boardLeds.getAllFieldLeds(move.start)) led.on();
		}

		public override void onPieceChanged(TSwitchChangeEvent e) {
			Console.WriteLine(e.pos.ToString() + e.state.ToString());
			if (e.pos == move.start && !e.state) tmpMove.start = e.pos;
			if (e.pos == move.start && e.state) tmpMove.start = null;
			if (e.pos == move.target && e.state) tmpMove.target = e.pos;
			if (e.pos == move.target && !e.state) tmpMove.target = null;

			if (tmpMove.start == null) {
				foreach (var led in boardLeds.getAllFieldLeds(move.target)) led.off();
				foreach (var led in boardLeds.getAllFieldLeds(move.start)) led.on();
			}
			else {
				foreach (var led in boardLeds.getAllFieldLeds(move.start)) led.off();
				foreach (var led in boardLeds.getAllFieldLeds(move.target)) led.on();
			}


			if (tmpMove.start != null && tmpMove.target != null) {

				string newFEN;
				bool isCheck;

				if (engine.validateMove(app.board.FEN, move.ToString(), out newFEN, out isCheck)) {
					if (isCheck) Console.WriteLine("CHECK!");
					app.player.play("sound3");
					app.board.setFEN(newFEN);

					Console.WriteLine("computer move done");
					uninstall();

					var handler = new TOwnMoveHandler();
					handler.install();
				}
				else {
					Console.WriteLine("INVALID MOVE");
				}

			}
		}

		public override void onPieceChangedDelay(TSwitchChangeEvent e) {
			base.onPieceChangedDelay(e);

		}

	}

	public class TDebugHandler : THandler
	{

		public override void onButtonChanged(TButtonChangeEvent e) {
			base.onButtonChanged(e);
			Console.WriteLine(e.button);
		}

		public override void onConsoleLine(TConsoleLineEvent e) {
			base.onConsoleLine(e);
			if (e.command == "f1") {
				foreach (var h in ioController.handlers)
					Console.WriteLine(h.GetType().Name);
			}
		}



	}

}
