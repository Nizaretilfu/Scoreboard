using Scoreboard.Application.DependencyInjection;
using Scoreboard.Infrastructure.DependencyInjection;
using Scoreboard.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<ScoreboardDbContext>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "scoreboard-api", status = "running" }));

app.Run();

public partial class Program;
