namespace BattleshipGame.Models;

public class Player
{
    public Board Board { get; set; } = new Board();
    public string ConnectionId { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsHost { get; set; }
    public Game Game { get; set; }
}