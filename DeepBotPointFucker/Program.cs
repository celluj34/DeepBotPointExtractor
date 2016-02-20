using System;
using System.Threading.Tasks;

namespace DeepBotPointFucker
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var apiKey = "8E7DCJWLUcXSbUYLcIJQdSfOfDeDQccECITOa";

            Task.Run(async () =>
            {
                var pointDownloader = new PointDownloader();

                var result = await pointDownloader.Connect(apiKey);

                if(result)
                {
                    var results = await pointDownloader.Download();

                    pointDownloader.WriteResultsToFile(results);
                }

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }).Wait();
        }
    }
}