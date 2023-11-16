using BattleshipGame.Hubs;
using BattleshipGame.Models;
using BattleshipGame.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipGame.Services;

public class BoardService
{
    private readonly MemoryService _memoryService;
    private readonly IHubContext<GameHub, IGameHub> _gameHubContext;
    
    public BoardService(MemoryService memoryService, IHubContext<GameHub, IGameHub> gameHubContext)
    {
        _memoryService = memoryService;
        _gameHubContext = gameHubContext;
    }
    
    public Board CreateBoard()
    {
        var rnd = new Random();
        var newBoard = new Board()
        {
            Id = rnd.Next(),
        };
        var newBoard2 = new Board()
        {
            Id = rnd.Next() + 1,
        };
        newBoard.FillWithEmptyCells();
        newBoard2.FillWithEmptyCells();
        newBoard.GroupId = newBoard.Id.ToString();
        newBoard2.GroupId = newBoard.GroupId;
        _memoryService.AddBoard(newBoard, newBoard.Id);
        _memoryService.AddBoard(newBoard2, newBoard2.Id);
        return newBoard;
    }
    
    public void SetShipOnBoard(string connectionId, int shipSize, int x, int y, int shipId)
    {
        if (_memoryService.Players.TryGetValue(connectionId, out var player))
        {
            if (!AreFieldsLegal(x, y, shipSize))
            {
                _gameHubContext.Clients.Client(connectionId).SendError("Invalid placement - outside of grid range");
                return;
            }
            if (!AreFieldsValid(player.Board, x, y, shipSize))
            { 
                _gameHubContext.Clients.Client(connectionId).SendError("Invalid placement - other ship is placed on this coords or you're trying to place ship too close to other one");
                return;
            }
            var ship = PlaceShipOnBoard(shipSize, player.Board, x, y, shipId, player.ConnectionId);
            player.Board.Ships.Add(ship);
            _gameHubContext.Clients.Client(connectionId).UpdatePlayerBoardAfterMove(player.Board);
            NotifyOnChange(player.Game);
        }
    }
    
    public void HitBoard(string connectionId, int x, int y)
    {
        if (_memoryService.Players.TryGetValue(connectionId, out var player))
        {
            if (player.Id == player.Game.Host.Id)
            {
                CheckHit(player.Game.Guest,x,y);
            }
            else
            {
                CheckHit(player.Game.Host, x,y );
            }
        }
    }
    
    private void CheckHit(Player player, int x, int y)
    {
        var hitInfo = new HitInfoViewModel()
        {
            PlayerId = player.Id,
            CellState = CellState.Empty,
            y = y,
            x = x
        };
        if (player.Board.Cells[x][y].State == CellState.Ship)
        {
            player.Board.Cells[x][y].State = CellState.Hit;
            hitInfo.CellState = CellState.Hit;
            if (player.Id == player.Game.Host.Id)
            {
                player.Game.GuestScore++;
                _gameHubContext.Clients.Client(player.Game.Guest.ConnectionId).SendScore(player.Game.GuestScore);
                CheckScore(player.Game);
            }
            else
            {
                player.Game.HostScore++;
                _gameHubContext.Clients.Client(player.Game.Host.ConnectionId).SendScore(player.Game.HostScore);
                CheckScore(player.Game);
            }
            
        }
        else
        {
            player.Board.Cells[x][y].State = CellState.Missed;
            hitInfo.CellState = CellState.Missed;
            
            if (player.Game.CurrentTurnPlayerId == player.Game.Host.Id)
            {
                player.Game.CurrentTurnPlayerId = player.Game.Guest.Id;
            }
            else
            {
                player.Game.CurrentTurnPlayerId = player.Game.Host.Id;
            }
        }
        _gameHubContext.Clients.Group(player.Game.Id.ToString()).SendHitInfo(hitInfo);
        NotifyOnChange(player.Game);
    }

    private void CheckScore(Game game)
    {
        if (game.HostScore == 20)
        {
            game.Winners.Add(game.Host.Id);
            game.GameOver = true;
            NotifyOnChange(game);
        }
        if (game.GuestScore == 20)
        {
            game.GameOver = true;
            game.Winners.Add(game.Guest.Id);
            NotifyOnChange(game);
            
        }
    }
    
    private Ship PlaceShipOnBoard(int shipSize, Board board, int x, int y, int shipId, string connectionId)
    {
        var ship = new Ship();
        ship.Length = shipSize;
        ship.BoardId = board.Id;
        ship.X = x;
        ship.Y = y;
        if (board.IsReady)
        {
            _gameHubContext.Clients.Client(connectionId).SendError("Board is locked, you placed all ships");
            
            return null;
        }
        if (!CheckIfShipExist(board, shipSize))
        {
            _gameHubContext.Clients.Client(connectionId).SendError("Too many ships of same type");
            return null;
        }
        if (x + shipSize - 1 > 9)
        {
            _gameHubContext.Clients.Client(connectionId).SendError("Ship is outside of board borders");
            return null;
        }
        for (int i = 0; i <= shipSize - 1; i++)
        {
            board.Cells[x + i][y].State = CellState.Ship;
            board.Cells[x + i][y].Battleship = ship;
            if (y == 0 && x == 0)
            {
                board.Cells[x + i][y + 1].State = CellState.Taken;
                board.Cells[x + i + 1][y + 1].State = CellState.Taken;
                board.Cells[x + i + 1][y].State = CellState.Taken;
            }
            if (y == 0 && x != 0)
            {
                if (x < 9 && x + i < 8)
                {
                    board.Cells[x + i][y + 1].State = CellState.Taken;
                    board.Cells[x + i - 1][y + 1].State = CellState.Taken;
                    board.Cells[x + i + 1][y + 1].State = CellState.Taken;
                    board.Cells[x - 1][y].State = CellState.Taken;
                    board.Cells[x + i + 1][y].State = CellState.Taken;
                }
                if (x == 9 || x + i == 9)
                {
                    board.Cells[x - 1][y].State = CellState.Taken;
                    board.Cells[x - 1][y + 1].State = CellState.Taken;
                    board.Cells[x][y + 1].State = CellState.Taken;
                    board.Cells[x + i][y + 1].State = CellState.Taken;
                }
            }
            if (y == 9)
            {
                board.Cells[x + i][y - 1].State = CellState.Taken;
            }
            if (x == 0 && y != 0)
            {
                if (y == 9)
                {
                    board.Cells[x + i][y - 1].State = CellState.Taken;
                    board.Cells[x + i + 1][y - 1].State = CellState.Taken;
                    board.Cells[x + i + 1][y].State = CellState.Taken;
                }
                if (y < 9)
                {
                    board.Cells[x + i + 1][y].State = CellState.Taken;
                    board.Cells[x + i + 1][y + 1].State = CellState.Taken;
                    board.Cells[x + i + 1][y - 1].State = CellState.Taken;
                }
            }
            if (x == 9 && y != 0)
            {
                if (y < 9)
                {
                    board.Cells[x - 1][y].State = CellState.Taken;
                    board.Cells[x - 1][y + 1].State = CellState.Taken;
                    board.Cells[x - 1][y - 1].State = CellState.Taken;
                }
                if (y == 9)
                {
                    board.Cells[x - 1][y].State = CellState.Taken;
                    board.Cells[x - 1][y - 1].State = CellState.Taken;
                }
            }
            if (y != 0 && y < 9)
            {
                board.Cells[x + i][y + 1].State = CellState.Taken;
                board.Cells[x + i][y - 1].State = CellState.Taken;
            }
            if (x != 0 && x + i != 9 && y < 9 && y != 0)
            {
                board.Cells[x - 1][y + 1].State = CellState.Taken;
                board.Cells[x - 1][y - 1].State = CellState.Taken;
                board.Cells[x - 1][y].State = CellState.Taken;
                board.Cells[x + i + 1][y].State = CellState.Taken;
                board.Cells[x + i + 1][y + 1].State = CellState.Taken;
                board.Cells[x + i + 1][y - 1].State = CellState.Taken;
            }
            if (y == 9 && x != 0 && x + i < 9)
            {
                board.Cells[x - 1][y - 1].State = CellState.Taken;
                board.Cells[x + i + 1][y].State = CellState.Taken;
                board.Cells[x + i + 1][y - 1].State = CellState.Taken;
                board.Cells[x - 1][y].State = CellState.Taken;
            }
        }
        ship.Id = shipId;
        ship.IsSet = true;
        board.ShipPool[shipSize] -= 1;
        return ship;
    }
    
    private bool AreFieldsLegal(int x, int y, int shipSize)
    {
        if (y < 0 || y > 9)
        {
            return false;
        }
        if (x < 0 || x + shipSize - 1 > 9)
        {
            return false;
        }
        return true;
    }
    
    private bool AreFieldsValid(Board board, int x, int y, int shipSize)
    {
        foreach (var ship in board.Ships)
        {
            for (int i = 0; i <= shipSize - 1; i++)
            {
                if (shipSize == 1)
                {
                    if (board.Cells[x][y].State == CellState.Empty)
                    {
                        return true;
                    }
                }
                if (board.Cells[x + i][y].State == CellState.Ship ||
                    board.Cells[x + i][y].State == CellState.Taken) // checking if placement is not on another ship
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    private bool CheckIfShipExist(Board board, int shipSize)
    {
        if (shipSize is 0 or > 4)
        {
            return false;
        }
        if (board.ShipPool[shipSize] <= 0)
        {
            return false;
        }
        return true;
    }
    
    public void NotifyOnChange(Game game)
    {
        if ( game.Ready)
        {
            _gameHubContext.Clients.Group(game.Id.ToString()).SendGameInfo((GameViewModel)game);
            
        }
    }
    
}