using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LibraryManagement.GUI
{
    public partial class BookListForm : Form
    {
        private DataGridView dgvBooks;
        private Button btnBorrow;
        private Button btnRefresh;
        private Button btnReserve;
        private Button btnReview;
        private TextBox txtSearch;
        private int _memberId;

        public BookListForm(int memberId)
        {
            InitializeComponent();
            _memberId = memberId;
            LoadBooks();
        }

        private void InitializeComponent()
        {
            this.Text = "All Books";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Create top panel to hold search box and buttons
            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 40,
                Padding = new Padding(5, 0, 5, 0),
                BackColor = System.Drawing.Color.White,
                WrapContents = false
            };

            // Create search box
            txtSearch = new TextBox
            {
                Width = 200,
                Height = 35,
                PlaceholderText = "Search books...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                Margin = new Padding(10, 0, 10, 0)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            var searchBoxContainer = new Panel
            {
                Width = 220,
                Height = 35,
                BackColor = Color.FromArgb(240, 240, 240),
                Margin = new Padding(10, 0, 0, 0)
            };
            searchBoxContainer.Controls.Add(txtSearch);

            // Add the search box to the top panel
            topPanel.Controls.Add(searchBoxContainer);

            // Add the Refresh, Borrow, Reserve, and Review buttons for members to the top panel
            btnRefresh = CreateButton("Refresh", "🔄");
            btnBorrow = CreateButton("Borrow Selected Book", "📥");
            btnReserve = CreateButton("Reserve", "📚");
            btnReview = CreateButton("Review Selected Book", "⭐");

            btnBorrow.Visible = true;
            btnReview.Visible = true;

            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnBorrow);
            topPanel.Controls.Add(btnReserve);
            topPanel.Controls.Add(btnReview);

            // Create DataGridView
            dgvBooks = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = System.Drawing.Color.FromArgb(245, 245, 245) }
            };

            // Add context menu
            var dgvContextMenu = new ContextMenuStrip();
            var reserveMenuItem = new ToolStripMenuItem("Reserve");
            var borrowMenuItem = new ToolStripMenuItem("Borrow");
            var reviewMenuItem = new ToolStripMenuItem("Review");

            dgvContextMenu.Items.Add(reserveMenuItem);
            dgvContextMenu.Items.Add(borrowMenuItem);
            dgvContextMenu.Items.Add(reviewMenuItem);

            dgvBooks.ContextMenuStrip = dgvContextMenu;

            // Wire up events
            dgvBooks.CellMouseClick += DgvBooks_CellMouseClick;
            reserveMenuItem.Click += ReserveMenuItem_Click;
            borrowMenuItem.Click += BorrowMenuItem_Click;
            reviewMenuItem.Click += ReviewMenuItem_Click;
            dgvBooks.SelectionChanged += DgvBooks_SelectionChanged;

            // Wire up new button events
            btnRefresh.Click += BtnRefresh_Click;
            btnReserve.Click += ReserveMenuItem_Click; // Reuse the context menu handler for the button
            btnBorrow.Click += BorrowMenuItem_Click; // Reuse the context menu handler for the button
            btnReview.Click += ReviewMenuItem_Click; // Reuse the context menu handler for the button

            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(dgvBooks, 0, 1);
            this.Controls.Add(mainPanel);
        }

        // Helper method to create styled buttons
        private Button CreateButton(string text, string icon)
        {
            var button = new Button
            {
                Text = $"{icon} {text}",
                Size = new Size(150, 35),
                Margin = new Padding(5, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 20, 20));
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 102, 204);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(0, 122, 204);

            return button;
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void LoadBooks()
        {
            List<Book> books = DatabaseService.GetAllBooks();
            var displayBooks = books.Select(b => new
            {
                b.ISBN,
                b.Title,
                Authors = string.Join(", ", b.BookAuthors?.Select(a => DatabaseService.GetAuthorById(a.AuthorID)?.Name) ?? new string[0]),
                Genres = string.Join(", ", b.BookGenres?.Select(g => DatabaseService.GetGenreById(g.GenreID)?.Name) ?? new string[0]),
                b.PublicationYear,
                b.Publisher,
                b.Language,
                b.PageCount,
                b.Availability,
                b.Description
            }).ToList();

            dgvBooks.DataSource = displayBooks;
        }

        private void DgvBooks_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgvBooks.Rows[e.RowIndex].Selected = true;
                // You can access the selected book's data here if needed
            }
        }

        private void DgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                var selectedBook = dgvBooks.SelectedRows[0].DataBoundItem as dynamic;
                string isbn = selectedBook.ISBN;
                int availability = selectedBook.Availability;

                // Enable/disable borrow button based on availability
                btnBorrow.Enabled = availability > 0;

                // Enable/disable reserve button based on availability and existing reservations
                bool hasExistingReservation = DatabaseService.GetActiveTransaction(isbn, _memberId)?.TransactionType == "Reservation";
                btnReserve.Enabled = !hasExistingReservation;
            }
            else
            {
                btnBorrow.Enabled = false;
                btnReserve.Enabled = false;
            }
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                var selectedBook = dgvBooks.SelectedRows[0].DataBoundItem as dynamic;
                string isbn = selectedBook.ISBN;

                // Check book availability
                int availability = DatabaseService.GetBookAvailability(isbn);

                if (availability > 0)
                {
                    // Check member's borrowed book count (assuming a limit of 5)
                    int borrowedCount = DatabaseService.GetBorrowedBookCountByMemberId(_memberId);
                    int borrowingLimit = 5; // Define your borrowing limit here

                    if (borrowedCount < borrowingLimit)
                    {
                        // Implement borrow logic
                        Transaction borrow = new Transaction
                        {
                            ISBN = isbn,
                            MemberID = _memberId,
                            TransactionDate = DateTime.Now,
                            TransactionType = "Borrow",
                            Status = "Borrowed",
                            DueDate = DateTime.Now.AddDays(14) // Example: Due in 14 days
                        };

                        DatabaseService.AddTransaction(borrow);
                        DatabaseService.UpdateBookAvailability(isbn, availability - 1);
                        MessageBox.Show($"Book borrowed successfully! Due Date: {borrow.DueDate:d}", "Borrow Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBooks(); // Refresh the book list
                    }
                    else
                    {
                        MessageBox.Show($"You have reached your borrowing limit of {borrowingLimit} books.", "Borrowing Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("This book is currently not available for borrowing.", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ReserveMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                var selectedBook = dgvBooks.SelectedRows[0].DataBoundItem as dynamic;
                string isbn = selectedBook.ISBN;

                // Check if member already has an active reservation for this book
                var existingReservation = DatabaseService.GetActiveTransaction(isbn, _memberId);
                if (existingReservation != null)
                {
                    MessageBox.Show("You already have an active reservation for this book.", "Reservation Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if member has reached reservation limit (e.g., 3 reservations)
                int reservationCount = DatabaseService.GetReservationCountByMemberId(_memberId);
                int reservationLimit = 3;
                if (reservationCount >= reservationLimit)
                {
                    MessageBox.Show($"You have reached your reservation limit of {reservationLimit} books.", "Reservation Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create reservation
                Transaction reservation = new Transaction
                {
                    ISBN = isbn,
                    MemberID = _memberId,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Reservation",
                    Status = "Pending",
                    DueDate = DateTime.Now.AddDays(7) // Reservation valid for 7 days
                };

                DatabaseService.AddTransaction(reservation);
                MessageBox.Show($"Book reserved successfully! Your reservation is valid until {reservation.DueDate:d}", "Reservation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks(); // Refresh the book list
            }
        }

        private void BorrowMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                var selectedBook = dgvBooks.SelectedRows[0].DataBoundItem as dynamic;
                string isbn = selectedBook.ISBN;

                // Check book availability
                int availability = DatabaseService.GetBookAvailability(isbn);

                if (availability > 0)
                {
                    // Check member's borrowed book count (assuming a limit of 5)
                    int borrowedCount = DatabaseService.GetBorrowedBookCountByMemberId(_memberId);
                    int borrowingLimit = 5; // Define your borrowing limit here

                    if (borrowedCount < borrowingLimit)
                    {
                        // Implement borrow logic
                        Transaction borrow = new Transaction
                        {
                            ISBN = isbn,
                            MemberID = _memberId,
                            TransactionDate = DateTime.Now,
                            TransactionType = "Borrow",
                            Status = "Borrowed",
                            DueDate = DateTime.Now.AddDays(14) // Example: Due in 14 days
                        };

                        DatabaseService.AddTransaction(borrow);
                        DatabaseService.UpdateBookAvailability(isbn, availability - 1);
                        MessageBox.Show($"Book borrowed successfully! Due Date: {borrow.DueDate:d}", "Borrow Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBooks(); // Refresh the book list
                    }
                    else
                    {
                        MessageBox.Show($"You have reached your borrowing limit of {borrowingLimit} books.", "Borrowing Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("This book is currently not available for borrowing.", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ReviewMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                var selectedBook = dgvBooks.SelectedRows[0].DataBoundItem as dynamic;
                string isbn = selectedBook.ISBN;

                // Open ReviewForm
                using (var reviewForm = new ReviewForm(isbn, _memberId))
                {
                    if (reviewForm.ShowDialog() == DialogResult.OK)
                    {
                        // Save the review
                        var review = reviewForm.Review;
                        DatabaseService.AddReview(review);
                        MessageBox.Show("Review submitted successfully!", "Review Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadBooks();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var searchTerm = txtSearch.Text.ToLower();
            var books = DatabaseService.GetAllBooks();
            var filteredBooks = books.Where(b =>
                b.Title.ToLower().Contains(searchTerm) ||
                b.ISBN.ToLower().Contains(searchTerm) ||
                b.Publisher.ToLower().Contains(searchTerm) ||
                (b.BookAuthors != null && b.BookAuthors.Any(ba => DatabaseService.GetAllAuthors().FirstOrDefault(a => a.AuthorID == ba.AuthorID)?.Name.ToLower().Contains(searchTerm) == true)) ||
                (b.BookGenres != null && b.BookGenres.Any(bg => DatabaseService.GetAllGenres().FirstOrDefault(g => g.GenreID == bg.GenreID)?.Name.ToLower().Contains(searchTerm) == true))
            ).Select(b => new
            {
                b.ISBN,
                b.Title,
                Authors = string.Join(", ", b.BookAuthors?.Select(a => DatabaseService.GetAuthorById(a.AuthorID)?.Name) ?? new string[0]),
                Genres = string.Join(", ", b.BookGenres?.Select(g => DatabaseService.GetGenreById(g.GenreID)?.Name) ?? new string[0]),
                b.PublicationYear,
                b.Publisher,
                b.Language,
                b.PageCount,
                b.Availability,
                b.Description
            }).ToList();
            dgvBooks.DataSource = filteredBooks;
        }
    }
} 