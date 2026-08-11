import { useState } from "react";
import { createGame, playMove } from "./api";
import "./App.css";

export default function App() {
  const [game, setGame] = useState(null);   // the latest GameStateDto
  const [busy, setBusy] = useState(false);   // true while a request is in flight
  const [error, setError] = useState(null);

  async function newGame() {
    setError(null);
    setBusy(true);
    try {
      setGame(await createGame());
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }

  async function drop(col) {
    if (!game || game.isOver || busy) return;
    if (!game.validMoves.includes(col)) return;
    setError(null);
    setBusy(true);
    try {
      setGame(await playMove(game.id, col)); // server plays your move + the AI's
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="app">
      <h1>Connect Four</h1>

      <div className="controls">
        <button onClick={newGame} disabled={busy}>
          {game ? "Restart" : "New Game"}
        </button>
        <span className="status">{status(game, busy)}</span>
      </div>

      {error && <p className="error">{error}</p>}

      {game ? (
        <Board game={game} busy={busy} onDrop={drop} />
      ) : (
        <p className="hint">Press “New Game” to start. You’re Red; the computer is Yellow.</p>
      )}
    </div>
  );
}

function status(game, busy) {
  if (!game) return "";
  if (busy) return "Thinking…";
  if (game.isDraw) return "Draw!";
  if (game.winner) return `${game.winner} wins!`;
  return `${game.currentPlayer}’s turn (${game.currentColour})`;
}

function Board({ game, busy, onDrop }) {
  const columns = game.board[0].length; // 7
  return (
    <div className="board">
      {Array.from({ length: columns }, (_, c) => {
        const playable = !game.isOver && !busy && game.validMoves.includes(c);
        return (
          <div
            key={c}
            className={`column ${playable ? "playable" : ""}`}
            onClick={() => playable && onDrop(c)}
          >
            {game.board.map((row, r) => (
              <div key={r} className={`cell ${row[c].toLowerCase()}`}>
                <span className="disc" />
              </div>
            ))}
          </div>
        );
      })}
    </div>
  );
}