using System;
using System.IO;

namespace YNOV_Password.Services
{
    public static class DatabaseHelper
    {
        private static string? _connectionString;

        public static string GetConnectionString()
        {
            if (_connectionString == null)
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "passwords.db");
                _connectionString = $"Data Source={dbPath}";
            }
            return _connectionString;
        }
    }
}
