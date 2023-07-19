using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class BoardService
{
    private readonly MemoryService _memoryService;

    public BoardService(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }
    public void CreateBoard()
    {
        var newBoard = new Board();
        var rnd = new Random();
        for (int i = 0; i <= 11; i++)
        {
            newBoard.rowX[i]++;
            newBoard.rowY[i]++;
            newBoard.setCoordsX[i] = 0;
            newBoard.setCoordsY[i] = 0;
            newBoard.hitCoordsX[i] = 0;
            newBoard.setCoordsY[i] = 0;
        }
        newBoard.id = rnd.Next();
        _memoryService.AddBoard(newBoard, newBoard.id);
    }

    public void SetBoard(int id, int setX, int setY)
    {
        var board = _memoryService.GetBoard(id);
        board.setCoordsX[setX] = setX;
        board.setCoordsY[setY] = setY;

    }
    public void CheckBoard(int id, int shootX, int shootY)
    {
        
        var board = _memoryService.GetBoard(id);
        for (int i = 0; i <= 11; i++)
        {
            if (board.setCoordsX[shootX] == shootX)
            {
                board.hitCoordsX[shootX] = board.setCoordsX[i];
            }
            if (board.setCoordsY[shootY] == shootY)
            {
                board.hitCoordsY[shootY] = board.setCoordsY[i];
            }
        }
    }
}
