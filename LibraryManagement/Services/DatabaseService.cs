using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LibraryManagement.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace LibraryManagement.Services
{
    public static class DatabaseService
    {
        private static readonly string _connectionString;

        static DatabaseService()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
            InitializeDatabase();
        }

        public static void InitializeDatabase()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Create Book Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Book (
                        ISBN VARCHAR(13) PRIMARY KEY,
                        Title VARCHAR(100) NOT NULL,
                        PublicationYear INT,
                        Publisher VARCHAR(100),
                        Language VARCHAR(50),
                        PageCount INT,
                        Availability INT DEFAULT 0,
                        Description TEXT
                    );";
                command.ExecuteNonQuery();

                // Create Author Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Author (
                        AuthorID INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL,
                        Biography TEXT
                    );";
                command.ExecuteNonQuery();

                // Create Genre Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Genre (
                        GenreID INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(50) NOT NULL,
                        Description TEXT
                    );";
                command.ExecuteNonQuery();

                // Create BookAuthor Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BookAuthor (
                        ISBN VARCHAR(13),
                        AuthorID INT,
                        PRIMARY KEY (ISBN, AuthorID),
                        FOREIGN KEY (ISBN) REFERENCES Book(ISBN),
                        FOREIGN KEY (AuthorID) REFERENCES Author(AuthorID)
                    );";
                command.ExecuteNonQuery();

                // Create BookGenre Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BookGenre (
                        ISBN VARCHAR(13),
                        GenreID INT,
                        PRIMARY KEY (ISBN, GenreID),
                        FOREIGN KEY (ISBN) REFERENCES Book(ISBN),
                        FOREIGN KEY (GenreID) REFERENCES Genre(GenreID)
                    );";
                command.ExecuteNonQuery();

                // Create Member Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Member (
                        MemberID INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(100) NOT NULL,
                        Email VARCHAR(100) NOT NULL UNIQUE,
                        Address VARCHAR(200)
                    );";
                command.ExecuteNonQuery();

                // Create Member_Phone Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Member_Phone (
                        MemberID INT,
                        Phone VARCHAR(20),
                        PRIMARY KEY (MemberID, Phone),
                        FOREIGN KEY (MemberID) REFERENCES Member(MemberID)
                    );";
                command.ExecuteNonQuery();

                // Create Transaction Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Transaction (
                        TransactionID INT AUTO_INCREMENT PRIMARY KEY,
                        ISBN VARCHAR(13),
                        MemberID INT,
                        TransactionDate DATETIME NOT NULL,
                        TransactionType VARCHAR(20) NOT NULL,
                        DueDate DATETIME NULL,
                        Status VARCHAR(20),
                        ReturnDate DATETIME NULL,
                        FOREIGN KEY (ISBN) REFERENCES Book(ISBN),
                        FOREIGN KEY (MemberID) REFERENCES Member(MemberID)
                    );";
                command.ExecuteNonQuery();

                // Create Fine Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Fine (
                        FineID INT AUTO_INCREMENT PRIMARY KEY,
                        TransactionID INT,
                        Amount DECIMAL(10,2) NOT NULL,
                        IssuedDate DATETIME NOT NULL,
                        PaymentDate DATETIME,
                        Status VARCHAR(20) NOT NULL,
                        FOREIGN KEY (TransactionID) REFERENCES Transaction(TransactionID)
                    );";
                command.ExecuteNonQuery();

                // Create Reviews Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Reviews (
                        ReviewID INT AUTO_INCREMENT PRIMARY KEY,
                        ISBN VARCHAR(13),
                        MemberID INT,
                        Review TEXT NOT NULL,
                        Rating INT,
                        FOREIGN KEY (ISBN) REFERENCES Book(ISBN),
                        FOREIGN KEY (MemberID) REFERENCES Member(MemberID)
                    );";
                command.ExecuteNonQuery();

                // Create Reservation Table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Reservation (
                        ReservationID INT AUTO_INCREMENT PRIMARY KEY,
                        ISBN VARCHAR(13),
                        MemberID INT,
                        ReservationDate DATETIME NOT NULL,
                        Status VARCHAR(20) NOT NULL,
                        FOREIGN KEY (ISBN) REFERENCES Book(ISBN),
                        FOREIGN KEY (MemberID) REFERENCES Member(MemberID)
                    );";
                command.ExecuteNonQuery();
            }
        }

        // Book Methods
        public static void AddBook(Book book)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO BOOK (ISBN, Title, PublicationYear, Publisher, Language, PageCount, Availability, Description)
                    VALUES (@ISBN, @Title, @PublicationYear, @Publisher, @Language, @PageCount, @Availability, @Description);
                ";
                command.Parameters.AddWithValue("@ISBN", book.ISBN);
                command.Parameters.AddWithValue("@Title", book.Title);
                command.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
                command.Parameters.AddWithValue("@Publisher", book.Publisher);
                command.Parameters.AddWithValue("@Language", book.Language);
                command.Parameters.AddWithValue("@PageCount", book.PageCount);
                command.Parameters.AddWithValue("@Availability", book.Availability);
                command.Parameters.AddWithValue("@Description", book.Description);
                command.ExecuteNonQuery();

                // Insert BookAuthors
                if (book.BookAuthors != null)
                {
                    foreach (var ba in book.BookAuthors)
                    {
                        var baCmd = connection.CreateCommand();
                        baCmd.CommandText = "INSERT INTO BookAuthor (ISBN, AuthorID) VALUES (@ISBN, @AuthorID);";
                        baCmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                        baCmd.Parameters.AddWithValue("@AuthorID", ba.AuthorID);
                        baCmd.ExecuteNonQuery();
                    }
                }
                // Insert BookGenres
                if (book.BookGenres != null)
                {
                    foreach (var bg in book.BookGenres)
                    {
                        var bgCmd = connection.CreateCommand();
                        bgCmd.CommandText = "INSERT INTO BookGenre (ISBN, GenreID) VALUES (@ISBN, @GenreID);";
                        bgCmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                        bgCmd.Parameters.AddWithValue("@GenreID", bg.GenreID);
                        bgCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static Book? GetBookByISBN(string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM BOOK WHERE ISBN = @ISBN;";
                command.Parameters.AddWithValue("@ISBN", isbn);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var book = new Book
                        {
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            Title = reader["Title"].ToString() ?? string.Empty,
                            PublicationYear = reader["PublicationYear"] != DBNull.Value ? Convert.ToInt32(reader["PublicationYear"]) : 0,
                            Publisher = reader["Publisher"].ToString() ?? string.Empty,
                            Language = reader["Language"].ToString() ?? string.Empty,
                            PageCount = reader["PageCount"] != DBNull.Value ? Convert.ToInt32(reader["PageCount"]) : 0,
                            Availability = reader["Availability"] != DBNull.Value ? Convert.ToInt32(reader["Availability"]) : 0,
                            Description = reader["Description"].ToString() ?? string.Empty
                        };
                        // Fetch authors
                        book.BookAuthors = GetBookAuthors(book.ISBN);
                        // Fetch genres
                        book.BookGenres = GetBookGenres(book.ISBN);
                        return book;
                    }
                }
            }
            return null;
        }

        public static List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM BOOK;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var book = new Book
                        {
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            Title = reader["Title"].ToString() ?? string.Empty,
                            PublicationYear = reader["PublicationYear"] != DBNull.Value ? Convert.ToInt32(reader["PublicationYear"]) : 0,
                            Publisher = reader["Publisher"].ToString() ?? string.Empty,
                            Language = reader["Language"].ToString() ?? string.Empty,
                            PageCount = reader["PageCount"] != DBNull.Value ? Convert.ToInt32(reader["PageCount"]) : 0,
                            Availability = reader["Availability"] != DBNull.Value ? Convert.ToInt32(reader["Availability"]) : 0,
                            Description = reader["Description"].ToString() ?? string.Empty
                        };
                        books.Add(book);
                    }
                }
                // Fetch authors and genres for each book
                foreach (var book in books)
                {
                    book.BookAuthors = GetBookAuthors(book.ISBN);
                    book.BookGenres = GetBookGenres(book.ISBN);
                }
            }
            return books;
        }

        public static List<BookAuthor> GetBookAuthors(string isbn)
        {
            var authors = new List<BookAuthor>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM BookAuthor WHERE ISBN = @ISBN;";
                command.Parameters.AddWithValue("@ISBN", isbn);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        authors.Add(new BookAuthor
                        {
                            ISBN = reader["ISBN"].ToString(),
                            AuthorID = Convert.ToInt32(reader["AuthorID"])
                        });
                    }
                }
            }
            return authors;
        }

        public static List<BookGenre> GetBookGenres(string isbn)
        {
            var genres = new List<BookGenre>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM BookGenre WHERE ISBN = @ISBN;";
                command.Parameters.AddWithValue("@ISBN", isbn);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        genres.Add(new BookGenre
                        {
                            ISBN = reader["ISBN"].ToString(),
                            GenreID = Convert.ToInt32(reader["GenreID"])
                        });
                    }
                }
            }
            return genres;
        }

        public static int GetBookAvailability(string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Availability FROM Book WHERE ISBN = @ISBN;";
                command.Parameters.AddWithValue("@ISBN", isbn);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public static void UpdateBookAvailability(string isbn, int availability)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Book SET Availability = @Availability WHERE ISBN = @ISBN;";
                command.Parameters.AddWithValue("@ISBN", isbn);
                command.Parameters.AddWithValue("@Availability", availability);
                command.ExecuteNonQuery();
            }
        }

        // Author CRUD
        public static int AddAuthor(Author author)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Author (Name, Biography) 
                    VALUES (@Name, @Biography);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@Name", author.Name);
                command.Parameters.AddWithValue("@Biography", author.Biography);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static Author? GetAuthorById(int authorId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Author WHERE AuthorID = @AuthorID;";
                command.Parameters.AddWithValue("@AuthorID", authorId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Author
                        {
                            AuthorID = Convert.ToInt32(reader["AuthorID"]),
                            Name = reader["Name"].ToString(),
                            Biography = reader["Biography"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public static List<Author> GetAllAuthors()
        {
            var authors = new List<Author>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Author;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        authors.Add(new Author
                        {
                            AuthorID = Convert.ToInt32(reader["AuthorID"]),
                            Name = reader["Name"].ToString(),
                            Biography = reader["Biography"].ToString()
                        });
                    }
                }
            }
            return authors;
        }

        // Genre CRUD
        public static int AddGenre(Genre genre)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Genre (Name, Description) 
                    VALUES (@Name, @Description);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@Name", genre.Name);
                command.Parameters.AddWithValue("@Description", genre.Description);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static Genre? GetGenreById(int genreId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Genre WHERE GenreID = @GenreID;";
                command.Parameters.AddWithValue("@GenreID", genreId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Genre
                        {
                            GenreID = Convert.ToInt32(reader["GenreID"]),
                            ISBN = reader["ISBN"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public static List<Genre> GetAllGenres()
        {
            var genres = new List<Genre>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Genre;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        genres.Add(new Genre
                        {
                            GenreID = Convert.ToInt32(reader["GenreID"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString()
                        });
                    }
                }
            }
            return genres;
        }

        // Member Methods
        public static bool MemberExistsWithEmail(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Member WHERE LOWER(Email) = LOWER(@Email);";
                command.Parameters.AddWithValue("@Email", email.Trim());
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public static bool AddMember(Member member)
        {
            if (MemberExistsWithEmail(member.Email))
            {
                return false; // Email already exists
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Member (Name, Email, Address)
                    VALUES (@Name, @Email, @Address);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@Name", member.Name);
                command.Parameters.AddWithValue("@Email", member.Email);
                command.Parameters.AddWithValue("@Address", member.Address);
                member.MemberID = Convert.ToInt32(command.ExecuteScalar());

                // Insert MemberPhones
                if (member.MemberPhones != null)
                {
                    foreach (var phone in member.MemberPhones)
                    {
                        var phoneCmd = connection.CreateCommand();
                        phoneCmd.CommandText = "INSERT INTO Member_Phone (MemberID, Phone) VALUES (@MemberID, @Phone);";
                        phoneCmd.Parameters.AddWithValue("@MemberID", member.MemberID);
                        phoneCmd.Parameters.AddWithValue("@Phone", phone.Phone);
                        phoneCmd.ExecuteNonQuery();
                    }
                }
            }
            return true; // Member added successfully
        }

        public static Member? GetMemberById(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Member WHERE MemberID = @MemberID;";
                command.Parameters.AddWithValue("@MemberID", memberId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var member = new Member
                        {
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            Name = reader["Name"].ToString() ?? string.Empty,
                            Email = reader["Email"].ToString() ?? string.Empty,
                            Address = reader["Address"].ToString() ?? string.Empty
                        };
                        member.MemberPhones = GetMemberPhones(member.MemberID);
                        return member;
                    }
                }
            }
            return null;
        }

        public static List<Member> GetAllMembers()
        {
            var members = new List<Member>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Member;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var member = new Member
                        {
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            Name = reader["Name"].ToString() ?? string.Empty,
                            Email = reader["Email"].ToString() ?? string.Empty,
                            Address = reader["Address"].ToString() ?? string.Empty
                        };
                        members.Add(member);
                    }
                }
                // Fetch phones for each member
                foreach (var member in members)
                {
                    member.MemberPhones = GetMemberPhones(member.MemberID);
                }
            }
            return members;
        }

        public static List<MemberPhone> GetMemberPhones(int memberId)
        {
            var phones = new List<MemberPhone>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Member_Phone WHERE MemberID = @MemberID;";
                command.Parameters.AddWithValue("@MemberID", memberId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        phones.Add(new MemberPhone
                        {
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            Phone = reader["Phone"].ToString()
                        });
                    }
                }
            }
            return phones;
        }

        public static void UpdateMember(Member member)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Member SET Name = @Name, Email = @Email, Address = @Address WHERE MemberID = @MemberID;";
                command.Parameters.AddWithValue("@MemberID", member.MemberID);
                command.Parameters.AddWithValue("@Name", member.Name);
                command.Parameters.AddWithValue("@Email", member.Email);
                command.Parameters.AddWithValue("@Address", member.Address);
                command.ExecuteNonQuery();

                // Delete old phones
                var delCmd = connection.CreateCommand();
                delCmd.CommandText = "DELETE FROM Member_Phone WHERE MemberID = @MemberID;";
                delCmd.Parameters.AddWithValue("@MemberID", member.MemberID);
                delCmd.ExecuteNonQuery();
                // Insert new phones
                if (member.MemberPhones != null)
                {
                    foreach (var phone in member.MemberPhones)
                    {
                        var phoneCmd = connection.CreateCommand();
                        phoneCmd.CommandText = "INSERT INTO Member_Phone (MemberID, Phone) VALUES (@MemberID, @Phone);";
                        phoneCmd.Parameters.AddWithValue("@MemberID", member.MemberID);
                        phoneCmd.Parameters.AddWithValue("@Phone", phone.Phone);
                        phoneCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeleteMember(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;

                        // First delete related fines
                        command.CommandText = @"
                            DELETE f FROM Fine f 
                            INNER JOIN Transaction t ON f.TransactionID = t.TransactionID 
                            WHERE t.MemberID = @MemberID;";
                        command.Parameters.AddWithValue("@MemberID", memberId);
                        command.ExecuteNonQuery();

                        // Then delete related transactions
                        command.CommandText = "DELETE FROM Transaction WHERE MemberID = @MemberID;";
                        command.ExecuteNonQuery();

                        // Delete related reviews
                        command.CommandText = "DELETE FROM Reviews WHERE MemberID = @MemberID;";
                        command.ExecuteNonQuery();

                        // Delete related reservations
                        command.CommandText = "DELETE FROM Reservation WHERE MemberID = @MemberID;";
                        command.ExecuteNonQuery();

                        // Delete related phone numbers
                        command.CommandText = "DELETE FROM member_phone WHERE MemberID = @MemberID;";
                        command.ExecuteNonQuery();

                        // Finally delete the member
                        command.CommandText = "DELETE FROM Member WHERE MemberID = @MemberID;";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Transaction Methods
        public static void AddTransaction(Transaction transaction)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Transaction (ISBN, MemberID, TransactionDate, TransactionType, DueDate, Status, ReturnDate)
                    VALUES (@ISBN, @MemberID, @TransactionDate, @TransactionType, @DueDate, @Status, @ReturnDate);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@ISBN", transaction.ISBN);
                command.Parameters.AddWithValue("@MemberID", transaction.MemberID);
                command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                command.Parameters.AddWithValue("@DueDate", (object?)transaction.DueDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)transaction.Status ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReturnDate", (object?)transaction.ReturnDate ?? DBNull.Value);
                transaction.TransactionID = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static int GetBorrowedBookCountByMemberId(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Count transactions that are 'Borrow' type and 'Borrowed' status and not returned
                command.CommandText = "SELECT COUNT(*) FROM Transaction WHERE MemberID = @MemberID AND TransactionType = 'Borrow' AND Status = 'Borrowed' AND ReturnDate IS NULL;";
                command.Parameters.AddWithValue("@MemberID", memberId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static Transaction? GetTransactionById(int transactionId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Transaction WHERE TransactionID = @TransactionID;";
                command.Parameters.AddWithValue("@TransactionID", transactionId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                            TransactionType = reader["TransactionType"].ToString() ?? string.Empty,
                            DueDate = reader["DueDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DueDate"]) : null,
                            Status = reader["Status"]?.ToString() ?? string.Empty,
                            ReturnDate = reader["ReturnDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ReturnDate"]) : null
                        };
                        // Navigation
                        transaction.Book = GetBookByISBN(transaction.ISBN);
                        transaction.Member = GetMemberById(transaction.MemberID);
                        return transaction;
                    }
                }
            }
            return null;
        }

        public static List<Transaction> GetAllTransactions()
        {
            var transactions = new List<Transaction>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Transaction;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var transaction = new Transaction
                        {
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                            TransactionType = reader["TransactionType"].ToString() ?? string.Empty,
                            DueDate = reader["DueDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["DueDate"]) : null,
                            Status = reader["Status"]?.ToString() ?? string.Empty,
                            ReturnDate = reader["ReturnDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ReturnDate"]) : null
                        };
                        transactions.Add(transaction);
                    }
                }
                // Navigation
                foreach (var transaction in transactions)
                {
                    transaction.Book = GetBookByISBN(transaction.ISBN);
                    transaction.Member = GetMemberById(transaction.MemberID);
                }
            }
            return transactions;
        }

        public static void UpdateTransaction(Transaction transaction)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Transaction SET ISBN = @ISBN, MemberID = @MemberID, TransactionDate = @TransactionDate, TransactionType = @TransactionType, DueDate = @DueDate, Status = @Status, ReturnDate = @ReturnDate
                    WHERE TransactionID = @TransactionID;";
                command.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                command.Parameters.AddWithValue("@ISBN", transaction.ISBN);
                command.Parameters.AddWithValue("@MemberID", transaction.MemberID);
                command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                command.Parameters.AddWithValue("@DueDate", (object?)transaction.DueDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)transaction.Status ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReturnDate", (object?)transaction.ReturnDate ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteTransaction(int transactionId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Transaction WHERE TransactionID = @TransactionID;";
                command.Parameters.AddWithValue("@TransactionID", transactionId);
                command.ExecuteNonQuery();
            }
        }

        // Fine Methods
        public static void AddFine(Fine fine)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Fine (TransactionID, Amount, IssuedDate, PaymentDate, Status)
                    VALUES (@TransactionID, @Amount, @IssuedDate, @PaymentDate, @Status);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@TransactionID", fine.TransactionID);
                command.Parameters.AddWithValue("@Amount", fine.Amount);
                command.Parameters.AddWithValue("@IssuedDate", fine.IssuedDate);
                command.Parameters.AddWithValue("@PaymentDate", (object)fine.PaymentDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", fine.Status);
                fine.FineID = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static List<Fine> GetFinesByMemberId(int memberId)
        {
            var fines = new List<Fine>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Select fines based on TransactionID linked to MemberID
                command.CommandText = @"SELECT f.* FROM Fine f JOIN Transaction t ON f.TransactionID = t.TransactionID WHERE t.MemberID = @MemberID;";
                command.Parameters.AddWithValue("@MemberID", memberId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fines.Add(new Fine
                        {
                            FineID = Convert.ToInt32(reader["FineID"]),
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
                            PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : null,
                            Status = reader["Status"].ToString() ?? string.Empty
                             // Reason is not selected here, add if needed
                        });
                    }
                }
            }
            return fines;
        }

        public static Fine? GetFineById(int fineId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM Fine WHERE FineID = @FineID;";
                command.Parameters.AddWithValue("@FineID", fineId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var fine = new Fine
                        {
                            FineID = Convert.ToInt32(reader["FineID"]),
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
                            PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : null,
                            Status = reader["Status"].ToString() ?? string.Empty
                        };
                        // Navigation
                        fine.Transaction = GetTransactionById(fine.TransactionID);
                        return fine;
                    }
                }
            }
            return null;
        }

        public static List<Fine> GetAllFines()
        {
            var fines = new List<Fine>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Fine;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fine = new Fine
                        {
                            FineID = Convert.ToInt32(reader["FineID"]),
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            IssuedDate = Convert.ToDateTime(reader["IssuedDate"]),
                            PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : null,
                            Status = reader["Status"].ToString() ?? string.Empty
                        };
                        fines.Add(fine);
                    }
                }
                // Navigation
                foreach (var fine in fines)
                {
                    fine.Transaction = GetTransactionById(fine.TransactionID);
                }
            }
            return fines;
        }

        public static void UpdateFine(Fine fine)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Fine SET TransactionID = @TransactionID, Amount = @Amount, IssuedDate = @IssuedDate, PaymentDate = @PaymentDate, Status = @Status, Reason = @Reason
                    WHERE FineID = @FineID;";
                command.Parameters.AddWithValue("@FineID", fine.FineID);
                command.Parameters.AddWithValue("@TransactionID", fine.TransactionID);
                command.Parameters.AddWithValue("@Amount", fine.Amount);
                command.Parameters.AddWithValue("@IssuedDate", fine.IssuedDate);
                command.Parameters.AddWithValue("@PaymentDate", (object?)fine.PaymentDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", fine.Status);
                command.Parameters.AddWithValue("@Reason", fine.Reason ?? (object)DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteFine(int fineId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Fine WHERE FineID = @FineID;";
                command.Parameters.AddWithValue("@FineID", fineId);
                command.ExecuteNonQuery();
            }
        }

        // Review Methods
        public static void AddReview(Review review)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Reviews (ISBN, MemberID, Review, Rating)
                    VALUES (@ISBN, @MemberID, @Review, @Rating);";
                command.Parameters.AddWithValue("@ISBN", review.ISBN);
                command.Parameters.AddWithValue("@MemberID", review.MemberID);
                command.Parameters.AddWithValue("@Review", review.ReviewText);
                command.Parameters.AddWithValue("@Rating", review.Rating);
                command.ExecuteNonQuery();
            }
        }

        public static void UpdateReview(Review review)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Reviews SET ISBN = @ISBN, MemberID = @MemberID, Review = @Review, Rating = @Rating
                    WHERE ReviewID = @ReviewID;";
                command.Parameters.AddWithValue("@ReviewID", review.ReviewID);
                command.Parameters.AddWithValue("@ISBN", review.ISBN);
                command.Parameters.AddWithValue("@MemberID", review.MemberID);
                command.Parameters.AddWithValue("@Review", review.ReviewText);
                command.Parameters.AddWithValue("@Rating", review.Rating);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteReview(int reviewId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Reviews WHERE ReviewID = @ReviewID;";
                command.Parameters.AddWithValue("@ReviewID", reviewId);
                command.ExecuteNonQuery();
            }
        }

        public static Review? GetReview(int reviewId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Reviews WHERE ReviewID = @ReviewID;";
                command.Parameters.AddWithValue("@ReviewID", reviewId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var review = new Review
                        {
                            ReviewID = Convert.ToInt32(reader["ReviewID"]),
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            ReviewText = reader["Review"].ToString() ?? string.Empty,
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : 0
                        };
                        // Navigation
                        review.Book = GetBookByISBN(review.ISBN);
                        review.Member = GetMemberById(review.MemberID);
                        return review;
                    }
                }
            }
            return null;
        }

        public static List<Review> GetAllReviews()
        {
            var reviews = new List<Review>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM Reviews;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var review = new Review
                        {
                            ReviewID = Convert.ToInt32(reader["ReviewID"]),
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            MemberID = Convert.ToInt32(reader["MemberID"]),
                            ReviewText = reader["Review"].ToString() ?? string.Empty,
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : 0
                        };
                        reviews.Add(review);
                    }
                }
                // Navigation
                foreach (var review in reviews)
                {
                    review.Book = GetBookByISBN(review.ISBN);
                    review.Member = GetMemberById(review.MemberID);
                }
            }
            return reviews;
        }

        // Calculates and updates overdue fines for all transactions
        public static void UpdateOverdueFines()
        {
            var transactions = GetAllTransactions();
            foreach (var transaction in transactions)
            {
                if (transaction.DueDate.HasValue && transaction.DueDate.Value < DateTime.Now)
                {
                    var existingFine = GetAllFines().FirstOrDefault(f => f.TransactionID == transaction.TransactionID);
                    if (existingFine == null)
                    {
                        var daysOverdue = (DateTime.Now - transaction.DueDate.Value).Days;
                        var fineAmount = daysOverdue * 1.00m; // $1 per day
                        var fine = new Fine
                        {
                            TransactionID = transaction.TransactionID,
                            Amount = fineAmount,
                            IssuedDate = DateTime.Now,
                            Status = "Unpaid"
                        };
                        AddFine(fine);
                    }
                }
            }
        }

        // Reservation Methods
        public static void AddReservation(Reservation reservation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Reservation (ISBN, MemberID, ReservationDate, Status)
                    VALUES (@ISBN, @MemberID, @ReservationDate, @Status);
                    SELECT LAST_INSERT_ID();";
                command.Parameters.AddWithValue("@ISBN", reservation.ISBN);
                command.Parameters.AddWithValue("@MemberID", reservation.MemberID == 0 ? (object)DBNull.Value : reservation.MemberID);
                command.Parameters.AddWithValue("@ReservationDate", reservation.ReservationDate);
                command.Parameters.AddWithValue("@Status", reservation.Status);
                reservation.ReservationID = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static List<Reservation> GetAllReservations()
        {
            var reservations = new List<Reservation>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Reservation;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reservation = new Reservation
                        {
                            ReservationID = Convert.ToInt32(reader["ReservationID"]),
                            ISBN = reader["ISBN"].ToString() ?? string.Empty,
                            MemberID = reader["MemberID"] != DBNull.Value ? Convert.ToInt32(reader["MemberID"]) : 0,
                            ReservationDate = Convert.ToDateTime(reader["ReservationDate"]),
                            Status = reader["Status"].ToString() ?? string.Empty
                        };
                        reservations.Add(reservation);
                    }
                }
                // Navigation
                foreach (var reservation in reservations)
                {
                    reservation.Book = GetBookByISBN(reservation.ISBN);
                    reservation.Member = GetMemberById((int)reservation.MemberID);
                }
            }
            return reservations;
        }

        public static void UpdateReservation(Reservation reservation)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Reservation
                    SET ISBN = @ISBN, MemberID = @MemberID, ReservationDate = @ReservationDate, Status = @Status
                    WHERE ReservationID = @ReservationID;";
                command.Parameters.AddWithValue("@ReservationID", reservation.ReservationID);
                command.Parameters.AddWithValue("@ISBN", reservation.ISBN);
                command.Parameters.AddWithValue("@MemberID", reservation.MemberID);
                command.Parameters.AddWithValue("@ReservationDate", reservation.ReservationDate);
                command.Parameters.AddWithValue("@Status", reservation.Status);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteReservation(int reservationId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Reservation WHERE ReservationID = @ReservationID;";
                command.Parameters.AddWithValue("@ReservationID", reservationId);
                command.ExecuteNonQuery();
            }
        }

        public static List<Review> GetReviewsByMemberId(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Reviews 
                    WHERE MemberID = @MemberID;
                ";
                command.Parameters.AddWithValue("@MemberID", memberId);

                var reviews = new List<Review>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new Review
                        {
                            ReviewID = reader.GetInt32("ReviewID"),
                            ISBN = reader.GetString("ISBN"),
                            MemberID = reader.GetInt32("MemberID"),
                            ReviewText = reader.GetString("Review"),
                            Rating = reader.GetInt32("Rating")
                        });
                    }
                }
                return reviews;
            }
        }

        public static Transaction? GetActiveTransaction(string isbn, int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Transaction 
                    WHERE ISBN = @ISBN 
                    AND MemberID = @MemberID 
                    AND TransactionType = 'Borrow' 
                    AND Status = 'Active'
                    ORDER BY TransactionDate DESC 
                    LIMIT 1;
                ";
                command.Parameters.AddWithValue("@ISBN", isbn);
                command.Parameters.AddWithValue("@MemberID", memberId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Transaction
                        {
                            TransactionID = reader.GetInt32("TransactionID"),
                            ISBN = reader.GetString("ISBN"),
                            MemberID = reader.GetInt32("MemberID"),
                            TransactionDate = reader.GetDateTime("TransactionDate"),
                            TransactionType = reader.GetString("TransactionType"),
                            DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? null : reader.GetDateTime("DueDate"),
                            Status = reader.GetString("Status"),
                            ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) ? null : reader.GetDateTime("ReturnDate")
                        };
                    }
                }
                return null;
            }
        }

        public static Review? GetReviewByMemberAndBook(int memberId, string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Reviews 
                    WHERE MemberID = @MemberID 
                    AND ISBN = @ISBN;
                ";
                command.Parameters.AddWithValue("@MemberID", memberId);
                command.Parameters.AddWithValue("@ISBN", isbn);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Review
                        {
                            ReviewID = reader.GetInt32("ReviewID"),
                            ISBN = reader.GetString("ISBN"),
                            MemberID = reader.GetInt32("MemberID"),
                            ReviewText = reader.GetString("Review"),
                            Rating = reader.GetInt32("Rating")
                        };
                    }
                }
                return null;
            }
        }

        public static bool UpdateBook(Book book)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Book 
                    SET Title = @Title,
                        PublicationYear = @PublicationYear,
                        Publisher = @Publisher,
                        Language = @Language,
                        PageCount = @PageCount,
                        Availability = @Availability,
                        Description = @Description
                    WHERE ISBN = @ISBN;";

                command.Parameters.AddWithValue("@ISBN", book.ISBN);
                command.Parameters.AddWithValue("@Title", book.Title);
                command.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
                command.Parameters.AddWithValue("@Publisher", book.Publisher);
                command.Parameters.AddWithValue("@Language", book.Language);
                command.Parameters.AddWithValue("@PageCount", book.PageCount);
                command.Parameters.AddWithValue("@Availability", book.Availability);
                command.Parameters.AddWithValue("@Description", book.Description);

                return command.ExecuteNonQuery() > 0;
            }
        }

        public static bool DeleteBook(string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;

                        // Delete related fines (dependent on transactions)
                        command.CommandText = @"
                            DELETE f FROM Fine f
                            INNER JOIN Transaction t ON f.TransactionID = t.TransactionID
                            WHERE t.ISBN = @ISBN;";
                        command.Parameters.AddWithValue("@ISBN", isbn);
                        command.ExecuteNonQuery();

                        // Delete related transactions
                        command.CommandText = "DELETE FROM Transaction WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        // Delete related reviews
                        command.CommandText = "DELETE FROM Reviews WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        // Delete related reservations
                        command.CommandText = "DELETE FROM Reservation WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        // Delete related book authors
                        command.CommandText = "DELETE FROM BookAuthor WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        // Delete related book genres
                        command.CommandText = "DELETE FROM BookGenre WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        // Finally delete the book
                        command.CommandText = "DELETE FROM Book WHERE ISBN = @ISBN;";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool HasActiveTransactions(string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Transaction 
                    WHERE ISBN = @ISBN AND Status = 'Active';";
                command.Parameters.AddWithValue("@ISBN", isbn);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public static bool HasActiveReservations(string isbn)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Reservation 
                    WHERE ISBN = @ISBN AND Status = 'Active';";
                command.Parameters.AddWithValue("@ISBN", isbn);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public static List<Reservation> GetReservationsByMemberId(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Reservation 
                    WHERE MemberID = @MemberID;
                ";
                command.Parameters.AddWithValue("@MemberID", memberId);

                var reservations = new List<Reservation>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reservations.Add(new Reservation
                        {
                            ReservationID = reader.GetInt32("ReservationID"),
                            ISBN = reader.GetString("ISBN"),
                            MemberID = reader.GetInt32("MemberID"),
                            ReservationDate = reader.GetDateTime("ReservationDate"),
                            Status = reader.GetString("Status")
                        });
                    }
                }
                return reservations;
            }
        }

        public static int GetReservationCountByMemberId(int memberId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) FROM Transaction 
                    WHERE MemberID = @MemberID 
                    AND TransactionType = 'Reservation' 
                    AND Status = 'Pending';
                ";
                command.Parameters.AddWithValue("@MemberID", memberId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
} 