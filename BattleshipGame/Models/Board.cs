namespace BattleshipGame.Models;

public class Board
{

    public readonly Dictionary<int, int> PlacedShips = new()
    {
        { 1, 4 }, // ShipSize, NumberOfShipsToPlace
        { 2, 3 },
        { 3, 2 },
        { 4, 1 }
    };
    
    public List<List<Cell>> Cells { get; set; } = new();
    public List<Ship> Ships { get; set; } = new();
    public int NumberOfPlacedShips { get; set; }
    public bool IsHost { get; set; } 
    public bool IsReady { get; set; } 
    public bool IsYourTurn { get; set; } 
    public bool IsLocked { get; set; }
    public bool IsWinner { get; set; }
    public bool IsGameOver { get; set; }
    public int Score { get; set; }
    public int BoardId { get; set; }
    public int RivalBoardId { get; set; }
    public string GroupId { get; set; } = "empty";
    public string ConnectionId { get; set; } = "empty";
}