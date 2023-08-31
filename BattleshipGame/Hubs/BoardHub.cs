using BattleshipGame.Models;
using BattleshipGame.Models.Requests;
using BattleshipGame.Services;
using Microsoft.AspNetCore.SignalR;
namespace BattleshipGame.Hubs;

public interface IBoardClient
{
    Task CreateBoard(Board board);
    Task GetBoard(Board board);
    Task JoinGame(Board board);
    Task HitBoard(Board board);
    Task GetRivalBoard(Board board);
    Task StartDuel(Board board);
    Task RefreshBoards(string test);
}
public class BoardHub : Hub<IBoardClient>
{
    private readonly BoardService _boardService;
    public BoardHub(BoardService boardService)
    {
        _boardService = boardService;
    }
    public Task CreateBoard()
    {
        var board = _boardService.CreateBoard();
        Groups.AddToGroupAsync(Context.ConnectionId, board.GroupId);
        return Clients.Client(Context.ConnectionId).CreateBoard(board);
    }
    public void SetBoard(SetBoardRequest setBoardRequest)
    {
        _boardService.SetBoard(setBoardRequest.BoardId, setBoardRequest.ShipSize, setBoardRequest.X, setBoardRequest.Y);
        GetBoard(setBoardRequest.BoardId);
    }
    public Task GetBoard(int boardId)
    {
        SendRivalBoard(_boardService.GetBoard(boardId).RivalBoardId);
        return Clients.Client(Context.ConnectionId).GetBoard(_boardService.GetBoard(boardId));
    }
    public Task JoinGame(int boardId)
    {
        var board = _boardService.JoinToGame(boardId);
        Groups.AddToGroupAsync(Context.ConnectionId, board.GroupId);
        return Clients.Client(Context.ConnectionId).JoinGame(board);
    }
    public async Task<Task> HitBoard(int boardId, int x, int y)
    {
        await RefreshBoards(boardId);
        return Clients.Clients(Context.ConnectionId).HitBoard(_boardService.HitBoard(boardId, x, y));
    }
    public Task SendRivalBoard(int boardId)
    {
        return Clients.Client(Context.ConnectionId).GetRivalBoard(_boardService.GetBoard(boardId));
    }
    public Task StartDuel(int boardId)
    {
        return Clients.Clients(Context.ConnectionId).StartDuel(_boardService.StartDuel(boardId));
    }
    public async Task RefreshBoards(int boardId)
    {
        var board = _boardService.GetBoard(boardId);
        var rivalBoard = _boardService.GetBoard(board.RivalBoardId);
        await Clients.Groups(board.GroupId).RefreshBoards("test");
    }
}