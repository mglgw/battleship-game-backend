using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class MemoryService
{
    public Dictionary<int, Board> Boards { get; set; } = new();
    public Dictionary<int, Ship> Ships { get; set; } = new();
    public Dictionary<int, ErrorMessage> ErrorMessages { get; set; } = new();
    public void AddBoard(Board board, int id)
    {
        Boards.TryAdd(id, board);
    }

    public Board GetBoard(int id)
    {
        Boards.TryGetValue(id, out var board);
        return board;
    }
    public void AddErrMess(ErrorMessage message, int id)
    {
        ErrorMessages.TryAdd(id, message);
    }
    public ErrorMessage GetMessage(int id)
    {
        ErrorMessages.TryGetValue(id, out var errorMessage);
        return errorMessage;
    }
    public bool CountBoards()
    {
        if (Boards.Count == 0)
        {
            return false;
        }
        return true;
    }
}