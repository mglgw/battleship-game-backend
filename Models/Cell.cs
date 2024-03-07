namespace BattleshipGame.Models;

public class Cell
{
    public CellState State { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Ship? Battleship { get; set; }
}