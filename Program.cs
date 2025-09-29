using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AdditionApi.Models;

namespace AdditionApi
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await DockerStarter.StartDockerContainerAsync();
            Console.WriteLine("Waiting for SQL Server to start...");

            string connectionString = Database.GetConnectionString();

            bool connected = false;
            int retryCount = 0;
            int maxRetries = 10;

            while (!connected && retryCount < maxRetries)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        connected = true;
                        Console.WriteLine("Database is ready.");
                    }
                }
                catch (SqlException)
                {
                    retryCount++;
                    Console.WriteLine($"Retry {retryCount}/{maxRetries}: Waiting 2 seconds before retry...");
                    await Task.Delay(2000);
                }
            }

            if (!connected)
            {
                Console.WriteLine("Failed to connect to the database after multiple attempts.");
                return;
            }

            Database.CreateAndSeedDatabase(connectionString);

            Console.WriteLine("Reading data from the database...");
            var calculations = Database.GetAllCalculations(connectionString);

            foreach (var calc in calculations)
            {
                Console.WriteLine(calc);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
