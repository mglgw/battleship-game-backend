namespace BattleshipGame.Models;

public class Board
{
    public List<Cell> CellList { get; set; } = new List<Cell>();
    public List<List<Cell>> Cells { get; set; } = new List<List<Cell>>();
    public List<Ship> Ships { get; set; } = new List<Ship>();
    public int NumberOfPlacedShips { get; set; }
    public bool IsHost { get; set; } = false;
    public bool IsReady { get; set; } = false;
    public bool IsYourTurn { get; set; } = false;
    public bool IsGameOver { get; set; }
    public int Score { get; set; }
    public int BoardId { get; set; }
    public string GroupId { get; set; } = "empty";
    public int RivalBoardId { get; set; }
    public bool IsLocked { get; set; }

    public Dictionary<int, int> PlacedShips = new Dictionary<int, int>()
    {
        {1,4}, // ShipSize, NumberOfShipsToPlace
        {2,3},
        {3,2},
        {4,1},
    };



}