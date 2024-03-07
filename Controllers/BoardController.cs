using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.Controllers;

[ApiController]
[Route("api")]
public class BoardController : ControllerBase
{
    [ActionName("HealthCheck")]
    [HttpGet("healthcheck")]
    public IActionResult HealthCheck()
    {
        return Ok(new {status="Ok"});
    }
}