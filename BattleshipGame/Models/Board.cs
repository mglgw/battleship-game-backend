namespace BattleshipGame.Models;

public class Board
{
    public int[] rowX { get; set; }
    public int[] rowY { get; set; }
    public int id { get; set; }
    public int[] setCoordsX { get; set; }
    public int[] setCoordsY { get; set; }
   
    public int[] hitCoordsX { get; set; }
   
    public int[] hitCoordsY { get; set; }
}