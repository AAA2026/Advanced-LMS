-- Recreate Database
DROP DATABASE IF EXISTS Library_db;
CREATE DATABASE Library_db;
USE Library_db;

-- ----------------------------------------
-- BOOK Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS BOOK (
    ISBN VARCHAR(20) PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    PublicationYear INT,
    Publisher VARCHAR(255),
    Language VARCHAR(50),
    PageCount INT,
    Availability INT DEFAULT 0,
    Description TEXT
);

-- ----------------------------------------
-- Genre Table (BCNF)
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Genre (
    GenreID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT
);

-- ----------------------------------------
-- Author Table (BCNF)
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Author (
    AuthorID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Biography TEXT
);

-- ----------------------------------------
-- BookAuthor Table (Many-to-Many)
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS BookAuthor (
    ISBN VARCHAR(20),
    AuthorID INT,
    PRIMARY KEY (ISBN, AuthorID),
    FOREIGN KEY (ISBN) REFERENCES BOOK(ISBN) ON DELETE CASCADE,
    FOREIGN KEY (AuthorID) REFERENCES Author(AuthorID) ON DELETE CASCADE
);

-- ----------------------------------------
-- BookGenre Table (Many-to-Many)
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS BookGenre (
    ISBN VARCHAR(20),
    GenreID INT,
    PRIMARY KEY (ISBN, GenreID),
    FOREIGN KEY (ISBN) REFERENCES BOOK(ISBN) ON DELETE CASCADE,
    FOREIGN KEY (GenreID) REFERENCES Genre(GenreID) ON DELETE CASCADE
);

-- ----------------------------------------
-- Member Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Member (
    MemberID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE,
    Address TEXT
);

-- ----------------------------------------
-- Member_Phone Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Member_Phone (
    MemberID INT,
    Phone VARCHAR(20),
    PRIMARY KEY (MemberID, Phone),
    FOREIGN KEY (MemberID) REFERENCES Member(MemberID) ON DELETE CASCADE
);

-- ----------------------------------------
-- Transaction Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Transaction (
    TransactionID INT AUTO_INCREMENT PRIMARY KEY,
    ISBN VARCHAR(20),
    MemberID INT,
    TransactionDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    TransactionType ENUM('Borrow', 'Return') NOT NULL,
    DueDate DATE,
    FOREIGN KEY (ISBN) REFERENCES BOOK(ISBN) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Member(MemberID) ON DELETE CASCADE
);

-- ----------------------------------------
-- Fine Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Fine (
    FineID INT AUTO_INCREMENT PRIMARY KEY,
    TransactionID INT,
    Amount DECIMAL(10,2) NOT NULL,
    IssuedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PaymentDate TIMESTAMP NULL,
    Status ENUM('Pending', 'Paid', 'Waived') DEFAULT 'Pending',
    FOREIGN KEY (TransactionID) REFERENCES Transaction(TransactionID) ON DELETE CASCADE
);

-- ----------------------------------------
-- Reviews Table
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS Reviews (
    ReviewID INT AUTO_INCREMENT PRIMARY KEY,
    ISBN VARCHAR(20),
    MemberID INT,
    Review TEXT,
    FOREIGN KEY (ISBN) REFERENCES BOOK(ISBN) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Member(MemberID) ON DELETE CASCADE
);

-- ----------------------------------------
-- User Table (for authentication)
-- ----------------------------------------
CREATE TABLE IF NOT EXISTS User (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role ENUM('Admin', 'Member') NOT NULL,
    MemberID INT,
    FOREIGN KEY (MemberID) REFERENCES Member(MemberID) ON DELETE SET NULL
);

-- ----------------------------------------
-- Sample Data
-- ----------------------------------------
INSERT INTO BOOK (ISBN, Title, PublicationYear, Publisher, Language, PageCount, Availability, Description) VALUES
('978-0134685991', 'Clean Code', 2008, 'Prentice Hall', 'English', 431, 5, 'A book about writing clean code'),
('978-0201633610', 'Design Patterns', 1994, 'Addison-Wesley', 'English', 395, 3, 'A book about design patterns in software engineering');

INSERT INTO Member (Name, Email, Address) VALUES
('John Doe', 'john.doe@email.com', '123 Main St'),
('Jane Smith', 'jane.smith@email.com', '456 Oak Ave');

INSERT INTO User (Username, PasswordHash, Role, MemberID) VALUES
('admin', 'admin123', 'Admin', NULL),
('johndoe', 'password1', 'Member', 1),
('janesmith', 'password2', 'Member', 2);

-- GENRES
INSERT INTO Genre (Name, Description) VALUES
('Programming', 'Books about programming'),
('Software Engineering', 'Books about software engineering');

-- AUTHORS
INSERT INTO Author (Name, Biography) VALUES
('Robert C. Martin', 'Known as Uncle Bob, author and software engineer.'),
('Erich Gamma', 'One of the Gang of Four, co-author of Design Patterns.');

-- BookAuthor (Many-to-Many)
INSERT INTO BookAuthor (ISBN, AuthorID) VALUES
('978-0134685991', 1),
('978-0201633610', 2);

-- BookGenre (Many-to-Many)
INSERT INTO BookGenre (ISBN, GenreID) VALUES
('978-0134685991', 1),
('978-0201633610', 2);

-- Member_Phone
INSERT INTO Member_Phone (MemberID, Phone) VALUES
(1, '123-456-7890'),
(2, '098-765-4321');

-- TRANSACTIONS
INSERT INTO Transaction (ISBN, MemberID, TransactionDate, TransactionType, DueDate) VALUES
('978-0134685991', 1, '2025-05-01', 'Borrow', '2025-05-15'),
('978-0201633610', 2, '2025-05-02', 'Borrow', '2025-05-16');

-- FINES
INSERT INTO Fine (TransactionID, Amount, IssuedDate, PaymentDate, Status) VALUES
(1, 5.00, '2025-05-16', NULL, 'Pending'),
(2, 0.00, '2025-05-16', '2025-05-16', 'Paid');

-- REVIEWS
INSERT INTO Reviews (ISBN, MemberID, Review) VALUES
('978-0134685991', 1, 'Excellent book for clean coding practices!'),
('978-0201633610', 2, 'A must-read for every software engineer.');

-- Add Egyptian Members
INSERT INTO Member (Name, Email, Address) VALUES
('Ahmed Hassan', 'ahmed.hassan@example.com', 'Cairo, Egypt'),
('Fatma Ali', 'fatma.ali@example.com', 'Giza, Egypt');

-- Add Famous Books (Examples)
INSERT INTO BOOK (ISBN, Title, PublicationYear, Publisher, Language, PageCount, Availability, Description) VALUES
('978-0743273565', 'The Great Gatsby', 1925, 'Scribner', 'English', 180, 2, 'A novel by F. Scott Fitzgerald.');

-- Add some authors and genres for the new book
-- Assuming AuthorIDs and GenreIDs are auto-generated, we need to get them or add them if they don't exist
-- For simplicity, let's assume some AuthorIDs and GenreIDs exist or we can add new ones.
-- In a real scenario, you'd need to check existing authors/genres or add them first.
-- Let's add F. Scott Fitzgerald as an author if he doesn't exist (assuming AuthorID 3 for example)
INSERT INTO Author (Name, Biography) VALUES ('F. Scott Fitzgerald', 'American novelist');
-- Add the relationship between the new book and author
-- You would need to get the actual AuthorID for F. Scott Fitzgerald after inserting or finding him.
-- For demonstration, let's assume AuthorID 3 is F. Scott Fitzgerald and GenreID 3 is 'Classic'
INSERT INTO BookAuthor (ISBN, AuthorID) VALUES ('978-0743273565', 3);
INSERT INTO Genre (Name, Description) VALUES ('Classic', 'Classic literature');
-- Add the relationship between the new book and genre
-- You would need to get the actual GenreID for 'Classic' after inserting or finding it.
INSERT INTO BookGenre (ISBN, GenreID) VALUES ('978-0743273565', 3);


-- Add Transactions (Example: Ahmed borrows The Great Gatsby)
-- You would need to get the actual MemberID for Ahmed Hassan and the correct ISBN.
-- Assuming MemberID 3 is Ahmed Hassan and ISBN is '978-0743273565'
INSERT INTO Transaction (ISBN, MemberID, TransactionDate, TransactionType, DueDate, Status) VALUES
('978-0743273565', 3, NOW(), 'Borrow', DATE_ADD(NOW(), INTERVAL 14 DAY), 'Borrowed');

-- Add a Fine (Example: Fatma has a fine)
-- Assuming MemberID 4 is Fatma Ali and TransactionID 1 (for a previous transaction)
-- This assumes a transaction already exists for Fatma to link the fine to.
-- Let's add a dummy transaction for Fatma first if needed, or link to an existing one.
-- Assuming TransactionID 2 is a transaction by Fatma
INSERT INTO Fine (TransactionID, Amount, IssuedDate, Status) VALUES
(2, 5.00, NOW(), 'Unpaid');

-- Add a Reservation (Example: Someone reserves a book)
-- Assuming MemberID 3 reserves a book with ISBN '978-0201633610' (Design Patterns)
INSERT INTO Reservation (ISBN, MemberID, ReservationDate, Status) VALUES
('978-0201633610', 3, NOW(), 'Active');
