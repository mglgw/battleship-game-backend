using BattleshipGame.Hubs;
using BattleshipGame.Services;

string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

#region Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<BoardService>();
builder.Services.AddSingleton<MemoryService>();
builder.Services.AddSignalR(options => { options.EnableDetailedErrors = true; });
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.AllowAnyOrigin();
        });
});
#endregion

var app = builder.Build();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(myAllowSpecificOrigins);
app.MapHub<GameHub>("/game");
app.MapControllers();
app.Run();
