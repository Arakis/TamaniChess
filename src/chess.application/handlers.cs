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
				e.board.gfx.FillRectangle(new SolidBrush(Color.Red), pos.x * 16, pos.y * 16, 16, 16);
		}

		public TMove tmpMove;
		public TMove showMove;

		public TPosition start { get { return tmpMove.pos1; } set { tmpMove.pos1 = value; } }
		public TPosition target { get { return tmpMove.pos2; } set { tmpMove.pos2 = value; } }

		public override void onDrawBoard(TDrawBoardEvent e) {
			base.onDrawBoard(e);

			if (e.type == EDrawBoardEventType.backgroundDrawed) {
				highlightPosition(e, showMove.pos1);
				highlightPosition(e, showMove.pos2);
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
			Console.Write("-");
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

		protected override void moveDone() {
			base.moveDone();

			string newFEN;
			bool isCheck;

			if (engine.validateMove(app.board.FEN, TMove.ToString(start, target), out newFEN, out isCheck)) {
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
			}

		}

	}

	public class TComputerMoveHandler : TMoveHandler
	{

		public List<TSwitchChangeEvent> changes = new List<TSwitchChangeEvent>();

		public TMove move;

		public TComputerMoveHandler(TMove move) {
			this.tmpMove = new TMove();
			showMove = move;
			this.move = move;
		}

		public override void install() {
			base.install();
			foreach (var led in boardLeds.getAllFieldLeds(move.pos1)) led.on();
		}

		public override void onPieceChanged(TSwitchChangeEvent e) {
			Console.Write("-");
		}

		public override void onPieceChangedDelay(TSwitchChangeEvent e) {
			base.onPieceChangedDelay(e);

			if (!e.pos.Equals(move.pos1) && changes.Count == 0) {
				Console.WriteLine("Wrong computer's pice");
				return;
			}

			if (!e.pos.Equals(move.pos2) && changes.Count == 1) {
				Console.WriteLine("Wrong computer's destination position");
				return;
			}

			if (changes.Count == 0) {
				foreach (var led in boardLeds.getAllFieldLeds(move.pos1)) led.off();
				foreach (var led in boardLeds.getAllFieldLeds(move.pos2)) led.on();

				changes.Add(e);
			}
			else if (changes.Count == 1) {

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

	}

	public class TDebugHandler : THandler
	{

		public override void onConsoleLine(TConsoleLineEvent e) {
			base.onConsoleLine(e);
			if (e.command == "f1") {
				foreach (var h in ioController.handlers)
					Console.WriteLine(h.GetType().Name);
			}
		}

	}

}
