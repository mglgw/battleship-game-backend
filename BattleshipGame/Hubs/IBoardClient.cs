using BattleshipGame.Models;

namespace BattleshipGame.Hubs;

public interface IBoardClient
{
    Task CreateBoard(Board board);
    Task GetBoard(Board board);
    Task JoinGame(Board board);
    Task HitBoard(Board board);
    Task GetRivalBoard(Board board);
    Task StartDuel(Board board);
    Task SendInfo(string message);
}