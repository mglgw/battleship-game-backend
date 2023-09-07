using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class BoardService
{
    private readonly MemoryService _memoryService;
    public BoardService(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }
    public Board CreateBoard()
    {
        var newBoard = new Board();
        var newBoard2Nd = new Board();
        var rnd = new Random();
        for (int i = 0; i < 10; i++)
        {
            var row = new List<Cell>();
            for (int j = 0; j < 10; j++)
            {
                var cell = new Cell();
                cell.State = CellState.Empty;
                cell.X = i;
                cell.Y = j;
                row.Add(cell);
            }
            newBoard.Cells.Add(row);
        }
        newBoard.IsLocked = false;
        newBoard.BoardId = rnd.Next();
        newBoard.Score = 0;
        for (int i = 0; i < 10; i++)
        {
            var row = new List<Cell>();
            for (int j = 0; j < 10; j++)
            {
                var cell = new Cell();
                cell.State = CellState.Empty;
                cell.X = i;
                cell.Y = j;
                row.Add(cell);
            }
            newBoard2Nd.Cells.Add(row);
        }
        newBoard2Nd.IsLocked = false;
        newBoard2Nd.BoardId = rnd.Next() + 1;
        newBoard2Nd.Score = 0;
        newBoard.RivalBoardId = newBoard2Nd.BoardId;
        newBoard2Nd.RivalBoardId = newBoard.BoardId;
        newBoard.IsHost = true;
        newBoard.GroupId = newBoard.BoardId.ToString();
        newBoard2Nd.GroupId = newBoard.GroupId;
        _memoryService.AddBoard(newBoard, newBoard.BoardId);
        _memoryService.AddBoard(newBoard2Nd, newBoard2Nd.BoardId);
        return newBoard;
    }
    public bool JoinToGame(int boardId, string connectionId)
    {
        Board board;
        if (!_memoryService.CountBoards())
        {
            AddMessageToPlayer("Unable to join game right now, make sure host started a game", connectionId, boardId);
            return false;
        }
        if (_memoryService.GetBoard(boardId) == null)
        {
            AddMessageToPlayer("Invalid game ID, try again!", connectionId, boardId);
            return false;
        }
        board = _memoryService.GetBoard(boardId);
        board = _memoryService.GetBoard(board.RivalBoardId);
        if (board != null)
        {
            board.ConnectionId = connectionId;
            return true;
        }
        return false;
    }
    public void SetBoard(int boardId, int shipSize, int x, int y, int shipId)
    {
        var ship = new Ship();
        var board = _memoryService.GetBoard(boardId);
        if (board == null)
        {
            return;
        }
        ship.BoardId = board.BoardId;
        if (!AreFieldsLegal(x, y, shipSize))
        {
            AddMessageToPlayer("Invalid placement - outside of grid range", board.ConnectionId, board.BoardId);
            return;
        }
        if (!AreFieldsValid(boardId, x, y, shipSize))
        {
            AddMessageToPlayer(
                "Invalid placement - other ship is placed on this coords or you're trying to place ship too close to other one",
                board.ConnectionId, board.BoardId);
            return;
        }
        ship = PlaceShipOnBoard(shipSize, boardId, x, y, shipId);
        board.Ships.Add(ship);
        if (board.Ships.Count == 10)
        {
            board.IsLocked = true;
        }
    }
    public void HitBoard(int boardId, int x, int y)
    {
        var board = _memoryService.GetBoard(boardId);
        var rivalBoard = _memoryService.GetBoard(board.RivalBoardId);
        if (board.IsReady == false || rivalBoard.IsReady == false)
        {
            AddMessageToPlayer("Players not ready", board.ConnectionId, board.BoardId);
        }
        if (board.IsYourTurn == false)
        {
            AddMessageToPlayer("Not your turn!", board.ConnectionId, board.BoardId);
        }
        if (board == null)
        {
            return;
        }
        if (board.Cells[x][y].State == CellState.Ship)
        {
            board.Cells[x][y].State = CellState.Hit;
            rivalBoard.Score++;
        }
        else
        {
            board.Cells[x][y].State = CellState.Missed;
            board.IsYourTurn = false;
            rivalBoard.IsYourTurn = true;
        }
        CheckScore(boardId);
    }
    public Board GetBoard(int boardId)
    {
        var board = _memoryService.GetBoard(boardId);
        return board;
    }
    public Board StartDuel(int boardId)
    {
        CheckIfBoardIsReady(boardId);
        var board = _memoryService.GetBoard(boardId);
        var rivalBoard = _memoryService.GetBoard(board.RivalBoardId);
        if (board.IsHost && board.IsReady)
        {
            rivalBoard.IsYourTurn = true;
        }
        return board;
    }
    private void CheckScore(int boardId)
    {
        var board = _memoryService.GetBoard(boardId);
        var rivalBoard = _memoryService.GetBoard(board.RivalBoardId);
        if (board.Score == 20)
        {
            board.IsWinner = true;
            board.IsGameOver = true;
            rivalBoard.IsGameOver = true;
        }
        if (rivalBoard.Score == 20)
        {
            rivalBoard.IsWinner = true;
            rivalBoard.IsGameOver = true;
            board.IsGameOver = true;
        }
    }
    private void AddMessageToPlayer(string message, string connectionId, int boardId)
    {
        var errMess = new ErrorMessage();
        var rnd = new Random();
        errMess.Id = rnd.Next();
        errMess.Message = message;
        errMess.ConnectionId = connectionId;
        errMess.BoardId = boardId;
        _memoryService.AddErrMess(errMess, errMess.Id);
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
    private bool AreFieldsValid(int boardId, int x, int y, int shipSize)
    {
        var board = _memoryService.GetBoard(boardId);
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
    private Ship PlaceShipOnBoard(int shipSize, int boardId, int x, int y, int shipId)
    {
        var board = _memoryService.GetBoard(boardId);
        var ship = new Ship();
        ship.Length = shipSize;
        ship.BoardId = boardId;
        if (board.IsLocked)
        {
            AddMessageToPlayer("Board is locked, you placed all ships", board.ConnectionId, board.BoardId);
            return null;
        }
        if (!CheckIfShipExist(boardId, shipSize))
        {
            AddMessageToPlayer("Too many ships of same type", board.ConnectionId, board.BoardId);
            return null;
        }
        if (x + shipSize - 1 > 9)
        {
            AddMessageToPlayer("Ship is outside of board borders", board.ConnectionId, board.BoardId);
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
        board.PlacedShips[shipSize] -= 1;
        board.NumberOfPlacedShips++;
        return ship;
    }
    private bool CheckIfShipExist(int boardId, int shipSize)
    {
        var board = _memoryService.GetBoard(boardId);
        if (shipSize is 0 or > 4)
        {
            return false;
        }
        if (board.PlacedShips[shipSize] <= 0)
        {
            return false;
        }
        return true;
    }
    private void CheckIfBoardIsReady(int boardId)
    {
        var board = _memoryService.GetBoard(boardId);
        if (board.NumberOfPlacedShips < 10)
        {
            AddMessageToPlayer("Place more ships and try again!", board.ConnectionId, board.BoardId);
        }
        board.IsReady = true;
    }
}