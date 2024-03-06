namespace BattleshipGame.Models.ViewModels;

public class HitInfoViewModel
{
    public int x { get; set; }
    public int y { get; set; }
    public CellState CellState { get; set; }
    public string PlayerId { get; set; }
}