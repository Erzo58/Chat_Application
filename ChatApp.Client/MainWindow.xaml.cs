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

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ConnectToServerAsync();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            await ConnectToServerAsync();
        }

        private async Task ConnectToServerAsync()
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                return;
            }

            ConnectButton.IsEnabled = false;

            try
            {
                if (_connection == null)
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl("http://localhost:52492/chathub")
                        .WithAutomaticReconnect()
                        .Build();

                    _connection.On<ChatMessage>("ReceiveMessage", (message) =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            string displayMessage = $"[{DateTime.Now.ToLongTimeString()}] {message.Username}: {message.Text}";
                            ChatListBox.Items.Add(displayMessage);
                            ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
                        });
                    });

                    _connection.Closed += async (error) =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ChatListBox.Items.Add("--- Spojenie prerušené ---");
                            ConnectButton.IsEnabled = true;
                        });
                        await Task.CompletedTask;
                    };
                }

                await _connection.StartAsync();
                ChatListBox.Items.Add("--- Úspešne pripojené k serveru ---");

                ConnectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba pripojenia: " + ex.Message);
                ConnectButton.IsEnabled = true;
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

        private async void Some_Typeing() { 
            // some one typing implementation
        
        }
    }
}