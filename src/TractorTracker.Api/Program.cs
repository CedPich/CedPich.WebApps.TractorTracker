using Microsoft.EntityFrameworkCore;
using TractorTracker.Api;
using TractorTracker.Application.UseCases;
using TractorTracker.Infrastructure;
using TractorTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<GetCurrentPosition>();
builder.Services.AddScoped<GetPositionHistory>();
builder.Services.AddScoped<GetDailyWorkHours>();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Auto-migration au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
