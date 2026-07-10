using ChatApp.Server.Api.Hub;
using ChatApp.Server.Core.Interfaces;
using ChatApp.Server.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton<IMessageRepository>(new MySqlMessageRepository(connectionString));

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<ChatHub>("/chathub");

app.Run();