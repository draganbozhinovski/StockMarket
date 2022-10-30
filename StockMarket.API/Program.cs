using Orleans;
using StockMarket.API;
using StockMarket.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add orleans client
builder.Services.AddSingleton<ClusterClientHostedService>();
builder.Services.AddSingleton<IHostedService>(
    sp => sp.GetService<ClusterClientHostedService>());
builder.Services.AddSingleton<IClusterClient>(
    sp => sp.GetService<ClusterClientHostedService>().Client);
builder.Services.AddSingleton<IGrainFactory>(
    sp => sp.GetService<ClusterClientHostedService>().Client);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(x => x
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(origin => true)
           .AllowCredentials());

app.MapHub<NotificationHub>("/notificationHub");

app.Run();



