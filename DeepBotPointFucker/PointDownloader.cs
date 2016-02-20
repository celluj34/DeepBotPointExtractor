using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepBotPointFucker.Models;
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

                await SendCommand(command);
            }
            catch
            {
                Log("Command failed.");

                return false;
            }

            try
            {
                var registerResult = await ReceiveMessage<RegisterResult>();

                Log("Response received.");

                if(registerResult?.Message == "success")
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

        public async Task<List<User>> Download()
        {
            Log("Beginning download.");
            var allUsers = new List<User>();
            var currentOffset = 0;
            const int limit = 100;

            do
            {
                string command = $"api|get_users|{currentOffset}|{limit}";

                var users = await GetUsers(command);

                allUsers.AddRange(users);

                currentOffset += users.Count;
            } while(currentOffset % limit == 0);

            Log("Finished download.");
            return allUsers;
        }

        private async Task<List<User>> GetUsers(string command)
        {
            try
            {
                Log($"Sending command `{command}`.");

                await SendCommand(command);
            }
            catch
            {
                Log("Command failed.");

                return new List<User>();
            }

            try
            {
                var result = await ReceiveMessage<UserResult>();

                Log("Response received.");

                return result.Message;
            }
            catch(Exception e)
            {
                Log(e.Message);

                Log("There was an error receiving the response.");

                return new List<User>();
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        private async Task SendCommand(string command)
        {
            var buffer = Encoding.UTF8.GetBytes(command);
            var arraySegment = new ArraySegment<byte>(buffer);

            await _socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<T> ReceiveMessage<T>()
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
                    return default(T);
                }

                using(var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    var message = await reader.ReadToEndAsync();

                    return JsonConvert.DeserializeObject<T>(message);
                }
            }
        }

        public void WriteResultsToFile(List<User> results)
        {
            Log("Beginning writing results to file.");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("User,Points");

            var text = results.Where(x => x.Points > 100)
                              .OrderByDescending(x => x.Points)
                              .Aggregate(stringBuilder, (builder, result) => builder.AppendLine($"{result.Name},{result.Points}"), builder => builder.ToString());

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "results.txt");

            if(!File.Exists(filePath))
            {
                File.Create(filePath);
            }

            File.WriteAllText(filePath, text);

            Log("Completed writing results to file.");
        }
    }
}