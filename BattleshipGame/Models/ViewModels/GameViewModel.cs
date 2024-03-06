namespace BattleshipGame.Models.ViewModels;

public class GameViewModel
{
    public Guid Id { get; set; }
    public List<string> Winners { get; set; }
    public bool Ready { get; set; }
    public int InvCode { get; set; }
    public string CurrentTurnPlayerId { get; set; }
    public bool GameOver { get; set; } = false;
    public static explicit operator GameViewModel(Game game) => new GameViewModel()
    {
        Id = game.Id,
        Ready = game.Ready,
        Winners = game.Winners,
        InvCode = game.InvCode,
        CurrentTurnPlayerId = game.CurrentTurnPlayerId,
        GameOver = game.GameOver
    };
}