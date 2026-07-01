using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebSocket.Client
{
    public partial class MainWindow : Window
    {
        private ClientWebSocket _webSocket = new ClientWebSocket();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri serverUri = new Uri("ws://localhost:5000/ws");
                await _webSocket.ConnectAsync(serverUri, CancellationToken.None);

                ChatListBox.Items.Add("-- Úspešne pripojené k serveru --");
                UsernameBox.IsEnabled = false;

                _ = Task.Run(() => ReceiveMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba pripojenia: " + ex.Message);
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ChatListBox.Items.Add(message);
                            ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
                        });
                    }
                }
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() => ChatListBox.Items.Add("--- Spojenie so serverom sa prerušilo ---"));
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_webSocket.State == WebSocketState.Open && !string.IsNullOrWhiteSpace(MessageInputBox.Text))
            {
                string fullMessage = $"{UsernameBox.Text}: {MessageInputBox.Text}";
                var buffer = Encoding.UTF8.GetBytes(fullMessage);

                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                MessageInputBox.Text = "";
            }
        }

        private void MessageInputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}