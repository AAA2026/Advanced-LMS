using System;

namespace LibraryManagement
{
    public static class DatabaseConfig
    {
        public static string Server = "localhost";
        public static string Database = "library_db";
        public static string Username = "root";
        public static string Password = "Balo248:";
        public static int Port = 3306;

        public static string GetConnectionString()
        {
            return $"Server={Server};Port={Port};Database={Database};User={Username};Password={Password};Allow User Variables=True;";
        }
    }
} 