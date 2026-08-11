const BASE = "http://localhost:5291";

export async function createGame() {
  const res = await fetch(`${BASE}/api/games`, { method: "POST" });
  if (!res.ok) throw new Error(`Create failed (${res.status})`);
  return res.json();
}

export async function playMove(id, column) {
  const res = await fetch(`${BASE}/api/games/${id}/moves`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ column }),
  });
  if (!res.ok) throw new Error((await res.text()) || `Move failed (${res.status})`);
  return res.json();
}
