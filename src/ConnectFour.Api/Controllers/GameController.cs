using ConnectFour.Api.Dtos;
using ConnectFour.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace ConnectFour.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GameStore store;
        public GamesController(GameStore store) => this.store = store;

        // POST /api/games
        [HttpPost]
        public ActionResult<GameStateDto> Create()
        {
            var session = store.Create();
            return GameMapper.ToDto(session);
        }

        // GET /api/games/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<GameStateDto> Get(Guid id)
        {
            var session = store.Get(id);
            if (session is null) return NotFound();
            return GameMapper.ToDto(session);
        }

        // POST /api/games/{id}/moves
        [HttpPost("{id:guid}/moves")]
        public ActionResult<GameStateDto> Move(Guid id, MoveRequest request)
        {
            var session = store.Get(id);
            if (session is null) return NotFound();

            var game = session.GetGame();
            if (game.IsOver) return BadRequest("Game is already over.");

            if (!game.PlayMove(request.Column))
                return BadRequest("Invalid move.");

            // Let any non-human player respond (guarded by IsHuman so HumanPlayer.GetMove never throws)
            while (!game.IsOver && !game.CurrentPlayer.IsHuman)
            {
                int aiColumn = game.CurrentPlayer.GetMove(game.Board);
                game.PlayMove(aiColumn);
            }

            return GameMapper.ToDto(session);
        }
    }
}