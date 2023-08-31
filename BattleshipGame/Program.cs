using BattleshipGame.Hubs;
using BattleshipGame.Services;
var  myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<BoardService>();
builder.Services.AddSingleton<MemoryService>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
} );
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy  =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.AllowAnyOrigin();
        });
});
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
app.MapHub<BoardHub>("/board");
app.MapControllers();
app.Run();