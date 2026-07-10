using ChatApp.Server.Core.Model;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace WebSocket.Client
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:52492/chathub")
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<ChatMessage>("ReceiveMessage", (message) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string displayMessage = $"[{message.Timestamp:HH:mm:ss}] {message.Username}: {message.Text}";
                        ChatListBox.Items.Add(displayMessage);
                        ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
                    });
                });
                _connection.Closed += async (error) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChatListBox.Items.Add("--- Spojenie so serverom sa prerušilo ---");
                    });
                    await Task.CompletedTask;
                };

                await _connection.StartAsync();

                ChatListBox.Items.Add("--- Úspešne pripojené k serveru ---");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba pripojenia: " + ex.Message);
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_connection != null &&
                _connection.State == HubConnectionState.Connected &&
                !string.IsNullOrWhiteSpace(MessageInputBox.Text))
            {
                try
                {
                    var novaSprava = new ChatMessage
                    {
                        Username = UsernameBox.Text,
                        Text = MessageInputBox.Text
                    };

                    await _connection.InvokeAsync("SendMessage", novaSprava);

                    MessageInputBox.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba pri odosielaní: " + ex.Message);
                }
            }
        }
    }
}