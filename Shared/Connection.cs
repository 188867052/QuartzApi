using Microsoft.Extensions.Configuration;

namespace EFCore.Scaffolding.Extension
{
    public static class Connection
    {
        private static string connectionString;

        public static string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }

                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                connectionString = config.GetSection("ConnectionString").Value;
                return connectionString;
            }
        }
    }
}
