using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class MemoryService
{
    public Dictionary<int, Board> Boards { get; set; } = new();
    public Dictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();
    public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();
    public void AddBoard(Board board, int id)
    {
        Boards.TryAdd(id, board);
    }
    
}