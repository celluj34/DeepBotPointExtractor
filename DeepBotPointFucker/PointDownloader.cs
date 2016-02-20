using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DeepBotPointFucker
{
    public class PointDownloader
    {
        private readonly ClientWebSocket _socket;

        public PointDownloader()
        {
            _socket = new ClientWebSocket();
        }

        public async Task<bool> Connect(string apiKey)
        {
            try
            {
                Log("Establishing connection.");

                const string url = "ws://localhost:3337";

                Log($"Connecting to `{url}`.");

                await _socket.ConnectAsync(new Uri(url), CancellationToken.None);
            }
            catch
            {
                Log("Connection failed.");

                return false;
            }

            try
            {
                var command = $"api|register|{apiKey}";

                Log($"Sending command `{command}`.");

                await SendMessage(command);
            }
            catch
            {
                Log("Command failed.");

                return false;
            }

            try
            {
                var registerResult = await ReceiveMessage();

                Log("Response received.");

                if (registerResult?.Message == "success")
                {
                    Log("Registering with DeepBot's API was successful.");

                    return true;
                }
            }
            catch
            {
                // ignored
            }

            Log("Registering with DeepBot's API was not successful.");

            return false;
        }

        public async Task<List<UserResult>> Download()
        {
            var allUsers = new List<UserResult>();
            var currentOffset = 0;
            const int limit = 100;

            var command = $"api|get_users|{currentOffset}|{limit}";

            List<UserResult> users;
            do
            {
                users = await GetUsers(command);

                allUsers.AddRange(users);

                currentOffset += users.Count;
            } while(users.Any());

            return allUsers;
        }

        private async Task<List<UserResult>> GetUsers(string command)
        {
            try
            {
                Log($"Sending command `{command}`.");

                await SendMessage(command);
            }
            catch
            {
                Log("Command failed.");

                return new List<UserResult>();
            }

            try
            {
                var result = await ReceiveMessage();

                Log("Response received.");

                return JsonConvert.DeserializeObject<List<UserResult>>(result.Message);
            }
            catch
            {
                Log("There was an error receiving the response.");

                return new List<UserResult>();
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        private async Task SendMessage(string message)
        {
            using(var memoryStream = new MemoryStream())
            {
                using(var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    await writer.WriteLineAsync(message);
                }

                var arraySegment = new ArraySegment<byte>(memoryStream.ToArray());

                await _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task<CommandResult> ReceiveMessage()
        {
            using(var memoryStream = new MemoryStream())
            {
                var buffer = new ArraySegment<byte>(new byte[8192]);
                WebSocketReceiveResult result;
                do
                {
                    result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                    memoryStream.Write(buffer.Array, buffer.Offset, result.Count);
                } while(!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);

                if(result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                using(var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    var message = await reader.ReadToEndAsync();

                    return JsonConvert.DeserializeObject<CommandResult>(message);
                }
            }
        }

        public void WriteResultsToFile(List<UserResult> results)
        {
            var ofd = new OpenFileDialog();
            if(ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("User,Points");

            var text = results.Aggregate(stringBuilder, (builder, result) => builder.AppendLine($"{result.User},{result.Points}"), builder => builder.ToString());

            File.WriteAllText(ofd.FileName, text);
        }
    }
}