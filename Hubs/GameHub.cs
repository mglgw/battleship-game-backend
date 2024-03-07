using BattleshipGame.Models;
using BattleshipGame.Models.Requests;
using BattleshipGame.Models.ViewModels;
using BattleshipGame.Services;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipGame.Hubs;

public class GameHub : Hub<IGameHub>
{
    private readonly MemoryService _memoryService;
    private readonly BoardService _boardService;

    public GameHub(MemoryService memoryService, BoardService boardService)
    {
        _memoryService = memoryService;
        _boardService = boardService;

    }

    public async Task CreateGame()
    {
        var rnd = new Random();
        var player = new Player();
        player.Board.FillWithEmptyCells();
        player.ConnectionId = Context.ConnectionId;
        var game = new Game(player);
        game.InvCode = rnd.Next();
        await Groups.AddToGroupAsync(Context.ConnectionId, game.Id.ToString());
        player.Game = game;
        game.CurrentTurnPlayerId = player.Id;
        _memoryService.Games.Add(game.Id, game);
        _memoryService.Players.Add(Context.ConnectionId, player);
        await Clients.Caller.UpdatePlayerBoard(player.Board);
        await Clients.Caller.SendGameInfo((GameViewModel)game);
        await Clients.Caller.SendPlayerId(player.Id);
    }

    public async Task<bool> JoinGame(int inviteCode)
    {
        foreach (var gameSession in _memoryService.Games.Where(gameSession =>
                     gameSession.Value.InvCode == inviteCode))
        {
            if (_memoryService.Games.TryGetValue((gameSession.Value.Id), out var game))
            {
                game.Guest = new Player();
                game.Guest.ConnectionId = Context.ConnectionId;
                game.Guest.Board.FillWithEmptyCells();
                game.Guest.Game = game;
                _memoryService.Players.Add(Context.ConnectionId, game.Guest);
                await Groups.AddToGroupAsync(Context.ConnectionId, gameSession.Value.Id.ToString());
                await Clients.Caller.UpdatePlayerBoard(game.Guest.Board);
                await Clients.Caller.SendPlayerId(game.Guest.Id);
                return true;
            }
        }

        return false;
    }
    public async Task SendBoardToPlayer(Player player)
    {
        foreach (var gameSession in _memoryService.Games.Where(gameSession 
                     => gameSession.Value.Host.Id == player.Id))
        {
            await Clients.Client(gameSession.Value.Host.ConnectionId).UpdatePlayerBoardAfterMove(gameSession.Value.Host.Board);
            return;
        }
        foreach (var gameSession in _memoryService.Games.Where(gameSession => gameSession.Value.Guest.Id == player.Id))
        {
            await Clients.Client(gameSession.Value.Guest.ConnectionId).UpdatePlayerBoardAfterMove(gameSession.Value.Guest.Board);
            return;
        }
    }
    public async void SetShipOnBoard(SetShipOnBoardRequest setShipOnBoardRequest)
    {
        _boardService.SetShipOnBoard(Context.ConnectionId, setShipOnBoardRequest.ShipSize, setShipOnBoardRequest.X, setShipOnBoardRequest.Y,
            setShipOnBoardRequest.ShipId);
    }
    public async Task HitBoard(int x, int y)
    {
       _boardService.HitBoard(Context.ConnectionId, x, y); 
    }
}