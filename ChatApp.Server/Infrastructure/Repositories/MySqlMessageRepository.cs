using MySqlConnector;
using ChatApp.Server.Core.Interfaces;
using ChatApp.Server.Core.Model;

namespace ChatApp.Server.Infrastructure.Repositories
{
    public class MySqlMessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MySqlMessageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("INSERT INTO Messages (Username, Text) VALUES (@u, @t)", conn);
            cmd.Parameters.AddWithValue("@u", message.Username);
            cmd.Parameters.AddWithValue("@t", message.Text);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
