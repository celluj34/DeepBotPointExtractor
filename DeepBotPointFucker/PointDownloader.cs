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
    public interface IPointDownloader
    {
        void Initialize();
        Task<bool> Connect();
        Task<List<User>> Download();
        void WriteResultsToFile(List<User> results);
    }

    public class PointDownloader : IPointDownloader
    {
        private const int ApiLimit = 100;
        private readonly ClientWebSocket _socket;
        private string _apiKey;
        private int _minimumPointValue;

        public PointDownloader()
        {
            _socket = new ClientWebSocket();
        }

        #region IPointDownloader Members
        public void Initialize()
        {
            _apiKey = GetValueFromConsole("Please enter your DeepBot Api Key. This is located in your master settings.",
                                          value => new Box<string> {HasValue = !string.IsNullOrWhiteSpace(value), Value = value});

            _minimumPointValue = GetValueFromConsole("Please enter the minimum number of points required for export.",
                                                     value =>
                                                     {
                                                         var box = new Box<int>();

                                                         int outVal;
                                                         if(int.TryParse(value, out outVal))
                                                         {
                                                             box.HasValue = true;
                                                             box.Value = outVal;
                                                         }

                                                         return box;
                                                     });
        }

        public async Task<bool> Connect()
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
                var command = $"api|register|{_apiKey}";

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

                if(registerResult?.Message == "success")
                {
                    Log("Registering with DeepBot's API was successful.");

                    return true;
                }
            }
            catch
            {
                Log("Registering with DeepBot's API was not successful.");

                return false;
            }

            return false;
        }

        public async Task<List<User>> Download()
        {
            Log("Beginning download.");
            var allUsers = new List<User>();
            var currentOffset = 0;

            do
            {
                var command = $"api|get_users|{currentOffset}|{ApiLimit}";

                var users = await GetUsers(command);

                if(users == null)
                {
                    break;
                }

                allUsers.AddRange(users);

                currentOffset += users.Count;
            } while(currentOffset % ApiLimit == 0);

            Log("Finished download.");

            return allUsers;
        }

        public void WriteResultsToFile(List<User> results)
        {
            Log("Beginning writing results to file.");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("User,Points");

            var text = results.Where(x => x.Points >= _minimumPointValue)
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
        #endregion

        private T GetValueFromConsole<T>(string initMessage, Func<string, Box<T>> handler)
        {
            Box<T> box;

            Log(initMessage);

            do
            {
                var value = Console.ReadLine();

                box = handler(value);
            } while(!box.HasValue);

            return box.Value;
        }

        private async Task<List<User>> GetUsers(string command)
        {
            try
            {
                await SendCommand(command);
            }
            catch
            {
                Log("Command failed.");

                return null;
            }

            try
            {
                var result = await ReceiveMessage<UserResult>();

                return result.Message;
            }
            catch(Exception e)
            {
                Log("There was an error receiving the response.");

                Log(e.Message);

                return null;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        private async Task SendCommand(string command)
        {
            Log($"Sending command `{command}`.");

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

                Log("Response received.");

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
    }
}