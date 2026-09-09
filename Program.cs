
using Microsoft.EntityFrameworkCore;
using ozankaya_api.Data;
using ozankaya_api.Hubs;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var rawConnectionString = builder.Configuration["DATABASE_URL"] 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = rawConnectionString;

if (!string.IsNullOrEmpty(rawConnectionString) && rawConnectionString.StartsWith("postgres"))
{
    var databaseUri = new Uri(rawConnectionString);
    var userInfo = databaseUri.UserInfo.Split(':');
    var port = databaseUri.Port > 0 ? databaseUri.Port : 5432;
    var dbName = databaseUri.LocalPath.TrimStart('/');
    
    connectionString = $"Host={databaseUri.Host};Port={port};Database={dbName};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddHostedService<AppointmentCleanupService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<AppointmentHub>("/appointmentHub");

app.Run();