## Advanced LMS
This project is a comprehensive desktop application designed for managing the operations of a library. Built using C# with Windows Forms on the .NET 9.0 framework, it provides a graphical user interface for librarians and members to interact with the library's catalog and services. The system utilizes a MySQL database to store and manage information about books, members, transactions, and other relevant library data.

## Features

The Library Management System offers a range of features to streamline library tasks. It supports detailed book cataloging, including information such as ISBN, title, author(s), genre(s), publication year, publisher, language, page count, availability, and descriptions. The system allows for the management of multiple authors and genres, linking them appropriately to the books in the catalog. Member management capabilities include registering new members, storing contact details (including multiple phone numbers), and maintaining member records. 

The application facilitates core library transactions, specifically borrowing and returning books. It tracks transaction dates, due dates, and the status of each transaction. To handle overdue books, the system includes functionality for managing fines, recording the amount, issue date, payment status, and payment date. Members can also view books and potentially leave reviews or ratings, enhancing the interactive aspect of the library service. Furthermore, the system supports book reservations, allowing members to reserve items that are currently unavailable. User authentication is implemented with distinct roles, typically 'Admin' for librarians managing the system and 'Member' for library users accessing services, ensuring appropriate access control to different functionalities.

## Prerequisites

### Before running this application, ensure you have the following software installed on your system:

*   **.NET Desktop Runtime 9.0 or later:** The application is built on the .NET 9.0 framework and requires the corresponding runtime for execution. You can download it from the official Microsoft .NET website.
*   **MySQL Server:** A running instance of MySQL server is required to host the library database. The application connects to this database to store and retrieve all its data. Ensure the MySQL server is accessible from the machine where the application will run.

## Setup and Installation

To set up the Library Management System, follow these steps:

1.  **Get the Code:** Clone the repository or download the source code files to your local machine.
2.  **Database Setup:** The application requires a MySQL database named `Library_db`. You have two options for setting up the database schema:
    *   **Automatic Initialization:** The application includes logic within its `DatabaseService` to automatically create the necessary tables (`Book`, `Author`, `Genre`, `Member`, `Transaction`, `Fine`, `Reviews`, `Reservation`, `User`, etc.) if they do not exist when the application starts. This is handled by the `InitializeDatabase` method.
    *   **Manual Setup:** Alternatively, you can manually create the database and tables using the provided `Library_db.sql` script. This script contains the `CREATE TABLE` statements for the entire schema and also includes sample data for testing purposes. Execute this script against your MySQL server using a tool like MySQL Workbench or the command-line client.
3.  **Configure Database Connection:** The application needs to know how to connect to your MySQL database. This is typically configured within a dedicated configuration file or class (likely `DatabaseConfig.cs` based on the code structure, though this file wasn't explicitly provided in the zip). You will need to update the connection string details (server address, database name, username, password) to match your MySQL server setup.
4.  **Build the Project:** Open the solution file (`.sln`, likely located within the main project directory) in Visual Studio or use the .NET CLI. Build the solution to compile the code and resolve dependencies. Using the CLI, navigate to the directory containing the `.csproj` file (`/home/ubuntu/library_project/library/LibraryManagement/`) and run the command `dotnet build`.

## Usage

Once the setup is complete, you can run the application by executing the generated `.exe` file located in the build output directory (e.g., `bin/Debug/net9.0-windows/`).

Upon starting the application, you will typically be presented with a welcome screen or login form (`WelcomeForm`). Here, you will select your role ('Admin' or 'Member') and potentially log in. Based on the sample data in `Library_db.sql`, default credentials might include 'admin' (password: 'admin123') for the administrator role and 'johndoe' (password: 'password1') or 'janesmith' (password: 'password2') for member roles.

After successful login, the main application window (`MainForm`) will appear, providing access to features based on the selected role. Administrators will generally have access to all management functions, including adding/editing books, authors, genres, members, and managing transactions and fines. Members will typically have functionalities focused on searching the catalog, viewing book details, checking their borrowed items, managing reservations, and potentially viewing fines or leaving reviews.

## Database Schema

The application relies on a relational database schema implemented in MySQL. Key tables include:

*   `Book`: Stores details about each book.
*   `Author`, `Genre`: Store information about authors and genres, respectively.
*   `BookAuthor`, `BookGenre`: Junction tables managing the many-to-many relationships between books and authors/genres.
*   `Member`: Contains information about library members.
*   `Member_Phone`: Stores multiple phone numbers per member.
*   `Transaction`: Records borrowing and returning activities.
*   `Fine`: Tracks fines associated with overdue transactions.
*   `Reviews`: Stores member reviews for books.
*   `Reservation`: Manages book reservations made by members.
*   `User`: Handles user authentication details, linking to members where applicable and storing roles.

Refer to the `Library_db.sql` script or the `InitializeDatabase` method in `DatabaseService.cs` for the exact table structures, fields, data types, and relationships (including primary and foreign keys).

*(Optional: Add sections for Contributing Guidelines and License information here if applicable.)*

