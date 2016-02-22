using System;
using System.Threading.Tasks;

namespace DeepBotPointFucker
{
    public class Program
    {
        private static void Main()
        {
            Task.Run(GetValue).Wait();
        }

        private static async Task GetValue()
        {
            IPointDownloader pointDownloader = new PointDownloader();

            pointDownloader.Initialize();

            var result = await pointDownloader.Connect();

            if(result)
            {
                var results = await pointDownloader.Download();

                pointDownloader.WriteResultsToFile(results);
            }

            Console.WriteLine("Press any key to quit.");
            Console.ReadLine();
        }
    }
}