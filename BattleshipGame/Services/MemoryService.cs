using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class MemoryService
{
    public Dictionary<int, Board> Boards { get; set; } = new Dictionary<int, Board>();

    public void AddBoard(Board board, int id)
    {
        Boards.TryAdd(id, board);
    }

    public Board GetBoard(int id)
    {
        Boards.TryGetValue(id, out Board board);
        return board;
    }
}