using System;
using System.Threading.Tasks;
using DeepBotPointExtractor.Services;

namespace DeepBotPointExtractor
{
    public class Program
    {
        private static void Main()
        {
            Task.Run(GetValue).Wait();
        }

        private static async Task GetValue()
        {
            IPointDownloadService pointDownloadService = new PointDownloadService();

            pointDownloadService.Initialize();

            var result = await pointDownloadService.Connect();

            if(result)
            {
                var results = await pointDownloadService.Download();

                pointDownloadService.WriteResultsToFile(results);
            }

            Console.WriteLine("Press any key to quit.");
            Console.ReadLine();
        }
    }
}