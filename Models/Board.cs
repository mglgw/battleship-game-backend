namespace BattleshipGame.Models;

public class Board
{
    public readonly Dictionary<int, int> ShipPool = new()
    {
        { 1, 4 }, // ShipSize, NumberOfShipsToPlace
        { 2, 3 },
        { 3, 2 },
        { 4, 1 }
    };
    
    public int Id { get; set; }
    public List<List<Cell>> Cells { get; set; } = new();
    public List<Ship> Ships { get; set; } = new();
    public bool IsReady => Ships.Count >= 10;
    public string GroupId { get; set; } = "empty";
    public void FillWithEmptyCells()
    {
        for (int i = 0; i < 10; i++)
        {
            var row = new List<Cell>();
            for (int j = 0; j < 10; j++)
            {
                var cell = new Cell();
                cell.State = CellState.Empty;
                cell.X = i;
                cell.Y = j;
                row.Add(cell);
            }
            Cells.Add(row);
        }
    }
    public void ChangeCellState(CellState cellState, int x, int y)
    {
        switch (cellState)
        {
            case CellState.Taken:
            {
                Cells[x][y].State = CellState.Taken;
                break;
            }
        }
    }
}