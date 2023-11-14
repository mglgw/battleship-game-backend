using BattleshipGame.Models.Requests;
using BattleshipGame.Services;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipGame.Hubs;

public class BoardHub : Hub<IBoardClient>
{
    private readonly BoardService _boardService;
    private readonly MemoryService _memoryService;
    
    public BoardHub(BoardService boardService, MemoryService memoryService)
    {
        _boardService = boardService;
        _memoryService = memoryService;
    }
    
    public Task CreateBoard()
    {
        var board = _boardService.CreateBoard();
        board.ConnectionId = Context.ConnectionId;
        return Clients.Client(Context.ConnectionId).CreateBoard(board);
    }
    
    public async void SetShipOnBoard(SetShipOnBoardRequest setShipOnBoardRequest)
    { 
        //TODO:add verification between boardID and connectionID to prevent setting ships on wrong boards f.e. by bypassing boardId in requests from frontend
        _boardService.SetShipOnBoard(setShipOnBoardRequest.BoardId, setShipOnBoardRequest.ShipSize, setShipOnBoardRequest.X, setShipOnBoardRequest.Y,
            setShipOnBoardRequest.ShipId);
        await RefreshBoards(setShipOnBoardRequest.BoardId);
        //TODO:Change method of checking for errors
        await CheckForErrors(setShipOnBoardRequest.BoardId);
    }
    
    public Task GetBoard(int boardId)
    {
        //TODO: set some kind of encryption while sending rivalBoard
        SendRivalBoard(_boardService.GetBoard(boardId).RivalBoardId);
        return Clients.Client(Context.ConnectionId).GetBoard(_boardService.GetBoard(boardId));
    }
    
    public async Task<Task> JoinGame(int boardId)
    {
        //TODO: resolve possible null while getting board
        var board = _boardService.GetBoard(boardId);
        if (_boardService.JoinToGame(boardId, Context.ConnectionId))
        {
            board = _boardService.GetBoard(board.RivalBoardId);
        }
        await CheckForErrors(boardId);
        return Clients.Client(Context.ConnectionId).JoinGame(board);
    }
    
    public async Task<Task> HitBoard(int rivalBoardId, int x, int y)
    {
        //TODO:add verification between boardID and connectionID to prevent hitting ships on wrong boards f.e. by bypassing boardId in requests from frontend
        var rivalBoard = _boardService.GetBoard(rivalBoardId);
        var board = _boardService.GetBoard(rivalBoard.RivalBoardId);
        _boardService.HitBoard(rivalBoardId, x, y);
        await RefreshBoards(board.BoardId);
        await CheckForErrors(rivalBoardId);
        return Clients.Clients(Context.ConnectionId).HitBoard(rivalBoard);
    }
    
    public Task SendRivalBoard(int boardId)
    {
        return Clients.Client(Context.ConnectionId).GetRivalBoard(_boardService.GetBoard(boardId));
    }
    
    public Task StartDuel(int boardId)
    {
        CheckForErrors(boardId);
        return Clients.Clients(Context.ConnectionId).StartDuel(_boardService.StartGame(boardId));
    }
    
    public async Task RefreshBoards(int boardId)
    {
        var board = _boardService.GetBoard(boardId);
        var rivalBoard = _boardService.GetBoard(board.RivalBoardId);
        if (board.NumberOfPlacedShips == 10 && board.IsReady == false)
        {
            await StartDuel(board.BoardId);
        }
        await Clients.Client(board.ConnectionId).GetBoard(board);
        await Clients.Client(rivalBoard.ConnectionId).GetBoard(rivalBoard);
        await Clients.Clients(rivalBoard.ConnectionId).GetRivalBoard(board);
    }
    
    public Task CheckForErrors(int boardId)
    {
        foreach (var errorMessage in _memoryService.ErrorMessages)
        {
            if (errorMessage.Value.BoardId != boardId || errorMessage.Value.Send != false)
            {
                continue;
            }
            var errorMsg = _memoryService.GetMessage(errorMessage.Key);
            errorMessage.Value.Send = true;
            return Clients.Client(Context.ConnectionId).SendInfo(errorMsg.Message);
        }
        return Clients.Client(Context.ConnectionId).SendInfo("");
    }
}