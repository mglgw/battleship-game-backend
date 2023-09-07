﻿using BattleshipGame.Models;
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
    Task SendInfo(string message);
}

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
    public async void SetBoard(SetBoardRequest setBoardRequest)
    {
        _boardService.SetBoard(setBoardRequest.BoardId, setBoardRequest.ShipSize, setBoardRequest.X, setBoardRequest.Y,
            setBoardRequest.ShipId);
        await RefreshBoards(setBoardRequest.BoardId);
        await CheckForErrors(setBoardRequest.BoardId);
    }
    public Task GetBoard(int boardId)
    {
        SendRivalBoard(_boardService.GetBoard(boardId).RivalBoardId);
        return Clients.Client(Context.ConnectionId).GetBoard(_boardService.GetBoard(boardId));
    }
    public async Task<Task> JoinGame(int boardId)
    {
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
        return Clients.Clients(Context.ConnectionId).StartDuel(_boardService.StartDuel(boardId));
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
            if (errorMessage.Value.BoardId == boardId && errorMessage.Value.Send == false)
            {
                var errorMsg = _memoryService.GetMessage(errorMessage.Key);
                errorMessage.Value.Send = true;
                return Clients.Client(Context.ConnectionId).SendInfo(errorMsg.Message);
            }
        }
        return Clients.Client(Context.ConnectionId).SendInfo("");
    }
}