using BattleshipGame.Hubs;
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
        return (newBoard);
    }
    public Board JoinToGame(int boardId)
    {
        var board = new Board();
        foreach (var x in _memoryService.Boards)
        {
            if (x.Value.BoardId == boardId)
            {
               board = _memoryService.GetBoard(x.Value.RivalBoardId);
            }
        }
        return board;
    }
    public void SetBoard(int boardId, int shipSize, int x, int y)
    {
        var ship = new Ship();
        var board = _memoryService.GetBoard(boardId);
        if (board == null)
        {
            throw new Exception("Invalid board number or board does not exist");
        }
        ship.BoardId = board.BoardId;
        if (!AreFieldsLegal(x, y))
        {
            throw new Exception("Invalid placement - outside of grid range");
        }
        if (!AreFieldsValid(boardId, x, y, shipSize ))
        {
            throw new Exception(
                "Invalid placement - other ship is placed on this coords or you're trying to place ship too close to other one");
        }
        board.Ships.Add(PlaceShipOnBoard(shipSize, boardId, x, y));
        if (board.Ships.Count == 10)
        {
            board.IsLocked = true;
        }
    }
    public Board HitBoard(int boardId, int x, int y)
    {
        var board = _memoryService.GetBoard(boardId);
        var rivalBoard = _memoryService.GetBoard(board.RivalBoardId);
        if (board.IsYourTurn == false)
        {
            throw new Exception("Not your turn!");
        }
        if (board == null)
        {
            throw new Exception("Invalid board number or board does not exist");
        }
        if (board.Cells[x][y].State == CellState.Ship)
        {
            board.Cells[x][y].State = CellState.Hit;
            rivalBoard.Score++;
        }
        else
        {
            board.Cells[x][y].State = CellState.Missed;
        }
        CheckScore(boardId);
        board.IsYourTurn = false;
        rivalBoard.IsYourTurn = true;
        return board;
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
        if (board.IsHost && board.IsReady && rivalBoard.IsReady)
        {
            rivalBoard.IsYourTurn = true;
        }
        return board;

    }
    private void CheckScore(int boardId)
    {
        var board = _memoryService.GetBoard(boardId);
        var rivalBoard = _memoryService.GetBoard(board.RivalBoardId);
        if (board.Score == 10)
        {
            board.IsGameOver = true;
        }
        if (rivalBoard.Score== 10)
        {
            rivalBoard.IsGameOver = true;
        }
        
    }
    /*public void HitBoard(int id, int hitX, int hitY)
    {
        if (id != _memoryService.GetBoard(id).Id)
        {
            throw new Exception("Invalid board number or board does not exist");
        }
        else
        {
            var board = _memoryService.GetBoard(id);
            board.HitCoordsX[hitX] = hitX;
            board.HitCoordsY[hitY] = hitY;
        }
    }*/
    private bool AreFieldsLegal(int x, int y)
    {
        if (y < 0 || y > 9) return false;
        if (x < 0 || x > 9) return false;
        return true;
    }
    private bool AreFieldsValid(int boardId, int x, int y, int shipSize)
    {
        var board = _memoryService.GetBoard(boardId);
        foreach (var ship in board.Ships)
        {
            for (int i = 0; i <= shipSize-1; i++)
            {
                if (shipSize==1)
                {
                    if (board.Cells[x][y].State == CellState.Empty)
                    {
                        return true;
                    }
                }
                if (board.Cells[x+i][y].State == CellState.Ship || board.Cells[x+i][y].State== CellState.Taken) // checking if placement is not on another ship
                {
                    return false;
                }
            }
        }
        return true;
    }
    private Ship PlaceShipOnBoard(int shipSize, int boardId, int x, int y)
    {
        var board = _memoryService.GetBoard(boardId);
        var ship = new Ship();
        ship.length = shipSize;
        if (board.IsLocked)
        {
            throw new Exception("Board is locked, you placed all ships");
        }
        if (!CheckIfShipExist(boardId,shipSize))
        {
            throw new Exception("Too many ships of same type");
        }
        if (x+shipSize-1 >9)
        {
            throw new Exception("Ship is outside of board borders");
        }
        for (int i = 0; i <= shipSize-1; i++)
        {
            board.Cells[x+i][y].State = CellState.Ship;
            board.Cells[x+i][y].Battleship = ship;
            if (y == 0 && x ==0)
            {
                board.Cells[x+i][y+1].State = CellState.Taken;
                board.Cells[x+i+1][y+1].State = CellState.Taken;
                board.Cells[x+i+1][y].State = CellState.Taken;
            }
            if (y== 0 && x!=0 )
            {
                if (x<9 && x+i <8)
                {
                    board.Cells[x+i][y+1].State = CellState.Taken;
                    board.Cells[x+i-1][y+1].State = CellState.Taken;
                    board.Cells[x+i+1][y+1].State = CellState.Taken;
                    board.Cells[x-1][y].State = CellState.Taken;
                    board.Cells[x+i+1][y].State = CellState.Taken;
                }
                if (x==9||x+i==9)
                {
                    board.Cells[x-1][y].State = CellState.Taken;
                    board.Cells[x-1][y+1].State = CellState.Taken;
                    board.Cells[x][y+1].State = CellState.Taken;
                    board.Cells[x+i][y+1].State = CellState.Taken;
                }
            }
            if (y==9)
            {
                board.Cells[x+i][y-1].State = CellState.Taken;
            }
            if (x==0 && y!=0)
            {
                if (y==9)
                {
                    board.Cells[x+i][y-1].State = CellState.Taken;
                    board.Cells[x+i+1][y-1].State = CellState.Taken;
                    board.Cells[x+i+1][y].State = CellState.Taken;
                }
                if (y<9)
                {
                    board.Cells[x+i+1][y].State = CellState.Taken;
                    board.Cells[x+i+1][y+1].State = CellState.Taken;
                    board.Cells[x+i+1][y-1].State = CellState.Taken;
                }
            }
            if (x==9&& y!=0)
            {
                if (y<9)
                {
                    board.Cells[x-1][y].State = CellState.Taken;
                    board.Cells[x-1][y+1].State = CellState.Taken;
                    board.Cells[x-1][y-1].State = CellState.Taken;
                }
                if (y==9)
                {
                    board.Cells[x-1][y].State = CellState.Taken;
                    board.Cells[x-1][y-1].State = CellState.Taken;
                }
            }
            if (y!=0 && y<9)
            {
                board.Cells[x+i][y+1].State = CellState.Taken;
                board.Cells[x+i][y-1].State = CellState.Taken;
            }
            if (x!=0 && x+i!=9 && y<9 && y!=0)
            {
                board.Cells[x-1][y+1].State = CellState.Taken;
                board.Cells[x-1][y-1].State = CellState.Taken;
                board.Cells[x-1][y].State = CellState.Taken;
                board.Cells[x+i+1][y].State = CellState.Taken;
                board.Cells[x+i+1][y+1].State = CellState.Taken;
                board.Cells[x+i+1][y-1].State = CellState.Taken;
            }
            if (y==9 && x!=0&& x+i<9)
            {
                board.Cells[x-1][y-1].State = CellState.Taken;
                board.Cells[x+i+1][y].State = CellState.Taken;
                board.Cells[x+i+1][y-1].State = CellState.Taken;
                board.Cells[x-1][y].State = CellState.Taken;
            }
        }
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
        if (board.NumberOfPlacedShips < 10 )
        {
            throw new Exception("Place more ships and try again!");
        }
        board.IsReady = true;
    }
}