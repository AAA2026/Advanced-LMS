using System;
using System.IO;
using MySql.Data.MySqlClient;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=localhost;Port=3306;User=root;Password=Balo248:;Allow User Variables=True;";
        
        try
        {
            // Read the SQL script
            string sqlScript = File.ReadAllText("../Library_db.sql");
            
            using (var connection = new MySqlConnection(connectionString))
            {
                Console.WriteLine("Attempting to connect to database...");
                connection.Open();
                Console.WriteLine("Connection successful!");
                
                // Split the script into individual statements
                string[] statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        using (var command = new MySqlCommand(statement, connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                                Console.WriteLine("Executed: " + statement.Trim().Substring(0, Math.Min(50, statement.Trim().Length)) + "...");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing statement: {ex.Message}");
                                Console.WriteLine($"Statement: {statement.Trim()}");
                            }
                        }
                    }
                }
                
                // Verify the data was imported
                connection.ChangeDatabase("library_db");
                using (var command = new MySqlCommand("SELECT COUNT(*) FROM BOOK", connection))
                {
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"\nNumber of books in database: {result}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
