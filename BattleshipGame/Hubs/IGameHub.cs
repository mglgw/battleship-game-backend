using BattleshipGame.Models;
using BattleshipGame.Models.ViewModels;

namespace BattleshipGame.Hubs;

public interface IGameHub
{
    Task SendScore(int score);
    Task UpdatePlayerBoardAfterMove(Board board);
    Task UpdatePlayerBoard(Board board);
    Task SendGameInfo(GameViewModel game);
    Task SendError(string message);
    Task SendPlayerId(string playerId);
    Task SendHitInfo(HitInfoViewModel hitInfoViewModel);
}