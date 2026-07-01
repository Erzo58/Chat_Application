using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var clients = new ConcurrentDictionary<Guid, WebSocket>();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection");

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var clientId = Guid.NewGuid();
        clients.TryAdd(clientId, webSocket); 

        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    using (var conn = new MySqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        var parts = message.Split(new[] { ':' }, 2);
                        string user = parts[0];
                        string text = parts.Length > 1 ? parts[1].Trim() : message;

                        using var cmd = new MySqlCommand("INSERT INTO Messages (Username, Text) VALUES (@u, @t)", conn);
                        cmd.Parameters.AddWithValue("@u", user);
                        cmd.Parameters.AddWithValue("@t", text);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var msgBytes = Encoding.UTF8.GetBytes(message);
                    foreach (var client in clients.Values)
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    clients.TryRemove(clientId, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Zatvorené", CancellationToken.None);
                }
            }
        }
        catch (Exception)
        {
            clients.TryRemove(clientId, out _);
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run("http://localhost:5000");