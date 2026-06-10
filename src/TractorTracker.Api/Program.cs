using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using TractorTracker.Api;
using TractorTracker.Application.UseCases;
using TractorTracker.Infrastructure;
using TractorTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    var logPath = ctx.Configuration["Logging:WebhookRawLogPath"] ?? "logs/webhook-raw-.json";

    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.Logger(lc => lc
           .Filter.ByIncludingOnly(Matching.FromSource("Webhook.Raw"))
           .WriteTo.File(
               logPath,
               rollingInterval: RollingInterval.Day,
               outputTemplate: "{Message:lj}{NewLine}"));
});

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
