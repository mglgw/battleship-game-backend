using BattleshipGame.Models.Requests;
using BattleshipGame.Services;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.Controllers;

[ApiController]
public class BoardController : ControllerBase
{
    private readonly BoardService _boardService;

    public BoardController(BoardService boardService)
    {
        _boardService = boardService;
    }

    /*[ActionName("JoinGame")]
    [HttpPost("api/board")]
    public IActionResult JoinGame([FromBody] JoinGameRequest joinGameRequest)
    {
        var joinGame = _boardService.JoinToGame(joinGameRequest.GameId);
        return Ok (joinGame);
    }*/
    [ActionName("Test")]
    [HttpPost("api/test")]
    public IActionResult Test()
    {
        return Ok();
    }
    [ActionName("Create Board")]
    [HttpPost("api/boards")]
    public IActionResult CreateBoard()
    {
        var cb = _boardService.CreateBoard();
        return Ok(cb);
    }
    [ActionName("Set Board")]
    [HttpPut("api/board")]
    public IActionResult SetBoard([FromBody] SetBoardRequest setBoardRequest)
    {
        _boardService.SetBoard(setBoardRequest.BoardId, setBoardRequest.ShipSize, setBoardRequest.X, setBoardRequest.Y,
            setBoardRequest.ShipId);
        return Ok();
    }
    [ActionName("Get Board")]
    [HttpGet("api/board")]
    public IActionResult GetBoard(int id)
    {
        var gb = _boardService.GetBoard(id);
        return Ok(gb);
    }
    [ActionName("Hit Board")]
    [HttpPut("api/boards")]
    public IActionResult HitBoard([FromBody] int id, int hitX, int hitY)
    {
        _boardService.HitBoard(id, hitX, hitY);
        return Ok();
    }
    /*[ActionName("Check Board")]
    [HttpPost("api/checkboard")]
    public IActionResult CheckBoard([FromBody] int id, int shootX, int shootY)
    {
        _boardService.CheckBoard(id, shootX, shootY);
        return Ok();
    }*/
}