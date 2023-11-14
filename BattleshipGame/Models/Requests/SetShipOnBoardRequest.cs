namespace BattleshipGame.Models.Requests;

public class SetShipOnBoardRequest
{
    public int BoardId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int ShipSize { get; set; }
    public int ShipId { get; set; }
}