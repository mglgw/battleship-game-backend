namespace BattleshipGame.Models;

public class ErrorMessage
{
    public int Id { get; set; }
    public string Message { get; set; } = "";
    public int BoardId { get; set; }
    public string ConnectionId { get; set; } = "";
    public bool Send { get; set; } = false;
}