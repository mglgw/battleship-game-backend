namespace BattleshipGame.Models;

public class Game
{
    public Guid Id { get; set; }
    public Player Host { get; private set; }
    public Player? Guest { get; set; }
    public int HostScore { get; set; } = 0;
    public int GuestScore { get; set; } = 0;
    public string CurrentTurnPlayerId { get; set; }
    public List<string> Winners { get; set; }
    public bool Ready => Guest!= null && Host.Board.IsReady && Guest.Board.IsReady;
    public int InvCode { get; set; }
    public bool GameOver { get; set; } = false;

    public Game(Player host)
    {
        Host = host;
        host.IsHost = true;
        Id = Guid.NewGuid();
        Winners = new List<string>();
    }
    
}