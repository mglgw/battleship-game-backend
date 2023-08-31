using BattleshipGame.Models;

namespace BattleshipGame.Services;

public class MemoryService
{
    public Dictionary<int, Board> Boards { get; set; } = new Dictionary<int, Board>();
    public Dictionary<int, Ship> Ships { get; set; } = new Dictionary<int, Ship>();

    public void AddBoard(Board board, int id)
    {
        Boards.TryAdd(id, board);
    }

    public Board GetBoard(int id)
    {
        Boards.TryGetValue(id, out Board board);
        return board;
    }
    public void AddShip(Ship ship, int id)
    {
        Ships.TryAdd(id, ship);
    }
    public Ship GetShip(int id)
    {
        Ships.TryGetValue(id, out Ship ship);
        return ship;
    }
  
}