using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.GUI
{
    public class BookManagementForm : Form
    {
        private DataGridView dgvBooks;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnBorrow;
        private Button btnReserve; // Added Reserve button
        private Button btnReturn;
        private TextBox txtSearch;
        private string _role;
        private int? _memberId; // Store the logged-in member ID

        // Constructor for Admin (no memberId needed)
        public BookManagementForm(string role)
            : this(role, null) // Call the main constructor
        {
        }

        // Main constructor accepting optional memberId
        public BookManagementForm(string role, int? memberId)
        {
            _role = role;
            _memberId = memberId;
            InitializeComponent();
            LoadBooks();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Book Management";
            this.Size = new Size(950, 600); // Slightly wider for new button
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10),
                RowStyles = { new RowStyle(SizeType.Absolute, 50), new RowStyle(SizeType.Absolute, 50), new RowStyle(SizeType.Percent, 100) }
            };

            // Create search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                Height = 35,
                Location = new Point(10, 10),
                PlaceholderText = "Search books by title, ISBN, author...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(txtSearch);

            // Create buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 5, 0, 5),
                BackColor = Color.White,
                WrapContents = false,
                AutoSize = true
            };

            btnAdd = CreateButton("Add Book", "➕");
            btnEdit = CreateButton("Edit Book", "✏️");
            btnDelete = CreateButton("Delete Book", "🗑️");
            btnRefresh = CreateButton("Refresh", "🔄");
            btnBorrow = CreateButton("Borrow", "📥");
            btnReserve = CreateButton("Reserve", "📚"); // Added Reserve button
            btnReturn = CreateButton("Return", "📤");

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnEdit);
            buttonsPanel.Controls.Add(btnDelete);
            buttonsPanel.Controls.Add(btnRefresh);
            buttonsPanel.Controls.Add(btnBorrow);
            buttonsPanel.Controls.Add(btnReserve);
            buttonsPanel.Controls.Add(btnReturn);

            // Create DataGridView
            dgvBooks = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false, // Manual columns
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 245, 245) },
                DefaultCellStyle = new DataGridViewCellStyle { Padding = new Padding(5), Font = new Font("Segoe UI", 9) },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Padding = new Padding(5) }
            };

            // Define Columns
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "ISBN", HeaderText = "ISBN", DataPropertyName = "ISBN", Width = 120 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", HeaderText = "Title", DataPropertyName = "Title", FillWeight = 150 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Authors", HeaderText = "Authors", DataPropertyName = "Authors", FillWeight = 100 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Availability", HeaderText = "Available Copies", DataPropertyName = "Availability", Width = 80 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 100 }); // Added Status column

            // Add controls to main panel
            mainPanel.Controls.Add(searchPanel, 0, 0);
            mainPanel.Controls.Add(buttonsPanel, 0, 1);
            mainPanel.Controls.Add(dgvBooks, 0, 2);

            this.Controls.Add(mainPanel);

            // Wire up events
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnBorrow.Click += BtnBorrow_Click;
            btnReserve.Click += BtnReserve_Click; // Added Reserve click handler
            btnReturn.Click += BtnReturn_Click;
            dgvBooks.SelectionChanged += dgvBooks_SelectionChanged;
            dgvBooks.CellDoubleClick += dgvBooks_CellDoubleClick;
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnBorrow.Visible = true;
                btnReserve.Visible = true; // Show reserve button for members
                btnReturn.Visible = true;
            }
            else if (_role == "Admin")
            {
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                btnBorrow.Visible = false;
                btnReserve.Visible = false; // Hide reserve button for admins
                btnReturn.Visible = false;
            }
            else // Guest or other roles
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnBorrow.Visible = false;
                btnReserve.Visible = false; // Hide reserve button for guests
                btnReturn.Visible = false;
            }
            // Initial state for member buttons
            btnBorrow.Enabled = false;
            btnReserve.Enabled = false; // Initialize reserve button state
            btnReturn.Enabled = false;
        }

        private Button CreateButton(string text, string icon)
        {
            var button = new Button
            {
                Text = $"{icon} {text}",
                Size = new Size(130, 35), // Adjusted size
                Margin = new Padding(5, 0, 5, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            // Simple hover effect
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 102, 204);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(0, 122, 204);

            return button;
        }

        // Keep DllImport if needed, otherwise remove
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void LoadBooks()
        {
            var books = DatabaseService.GetAllBooks();
            var displayBooks = books.Select(b =>
            {
                string status = "Available";
                if (_memberId.HasValue)
                {
                    // Check if the current member has borrowed this book
                    var transaction = DatabaseService.GetActiveTransaction(b.ISBN, _memberId.Value);
                    if (transaction != null)
                    {
                        status = $"Borrowed (Due: {transaction.DueDate:d})";
                    }
                    else if (b.Availability <= 0)
                    {
                        // Check if reserved by this member
                        var memberReservation = DatabaseService.GetAllReservations()
                            .FirstOrDefault(r => r.ISBN == b.ISBN && r.MemberID == _memberId.Value && r.Status == "Active");
                        if (memberReservation != null)
                        {
                            status = "Reserved by you";
                        }
                        else
                        {
                            // Check if reserved by someone else
                            var otherReservation = DatabaseService.GetAllReservations()
                                .FirstOrDefault(r => r.ISBN == b.ISBN && r.Status == "Active");
                            if (otherReservation != null)
                            {
                                status = "Reserved";
                            }
                            else
                            {
                                status = "Unavailable";
                            }
                        }
                    }
                }
                else if (b.Availability <= 0)
                {
                    // Check if reserved by anyone
                    var reservation = DatabaseService.GetAllReservations()
                        .FirstOrDefault(r => r.ISBN == b.ISBN && r.Status == "Active");
                    status = reservation != null ? "Reserved" : "Unavailable";
                }

                // Get authors for this book
                var authors = new List<string>();
                if (b.BookAuthors != null)
                {
                    foreach (var ba in b.BookAuthors)
                    {
                        var author = DatabaseService.GetAuthorById(ba.AuthorID);
                        if (author != null)
                        {
                            authors.Add(author.Name);
                        }
                    }
                }

                return new
                {
                    b.ISBN,
                    b.Title,
                    Authors = string.Join(", ", authors),
                    b.Availability,
                    Status = status
                };
            }).ToList();
            dgvBooks.DataSource = displayBooks;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new BookForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DatabaseService.AddBook(form.Book);
                        MessageBox.Show("Book added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add book. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    LoadBooks();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0) return;
            var isbn = dgvBooks.SelectedRows[0].Cells["ISBN"].Value.ToString();
            var book = DatabaseService.GetBookByISBN(isbn);
            if (book == null) return;

            using (var form = new BookForm(book)) // Assuming BookForm can be initialized with a book for editing
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (DatabaseService.UpdateBook(form.Book))
                    {
                        MessageBox.Show("Book updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    LoadBooks();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0) return;
            var isbn = dgvBooks.SelectedRows[0].Cells["ISBN"].Value.ToString();
            var result = MessageBox.Show($"Are you sure you want to delete book with ISBN {isbn}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                // Need DatabaseService.DeleteBook method
                // DatabaseService.DeleteBook(isbn);
                if (DatabaseService.DeleteBook(isbn))
                {
                    MessageBox.Show("Book deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LoadBooks();
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Basic search - refine as needed
            LoadBooks(); // Reload all and then filter the DataSource
            var searchTerm = txtSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchTerm) && dgvBooks.DataSource is List<dynamic> currentData)
            {
                 var filteredData = currentData.Where(b =>
                    ((string)b.ISBN)?.ToLower().Contains(searchTerm) == true ||
                    ((string)b.Title)?.ToLower().Contains(searchTerm) == true ||
                    ((string)b.Authors)?.ToLower().Contains(searchTerm) == true
                 ).ToList();
                 dgvBooks.DataSource = filteredData;
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadBooks();
        }

        private void dgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (_role != "Member" || !_memberId.HasValue || dgvBooks.SelectedRows.Count == 0)
            {
                btnBorrow.Enabled = false;
                btnReserve.Enabled = false;
                btnReturn.Enabled = false;
                return;
            }

            // Get selected book details from the dynamic object
            var selectedRow = dgvBooks.SelectedRows[0];
            string isbn = selectedRow.Cells["ISBN"].Value.ToString();
            int availability = (int)selectedRow.Cells["Availability"].Value;
            string status = selectedRow.Cells["Status"].Value.ToString();

            // Enable/Disable Borrow/Reserve/Return based on status and availability
            bool isBorrowedByCurrentUser = status.StartsWith("Borrowed");
            bool hasExistingReservation = DatabaseService.GetActiveTransaction(isbn, _memberId.Value)?.TransactionType == "Reservation";

            btnBorrow.Enabled = !isBorrowedByCurrentUser && availability > 0;
            btnReserve.Enabled = !isBorrowedByCurrentUser && !hasExistingReservation && availability == 0;
            btnReturn.Enabled = isBorrowedByCurrentUser;
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0) return;

            var isbn = dgvBooks.SelectedRows[0].Cells["ISBN"].Value?.ToString();
            var availability = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["Availability"].Value);

            if (string.IsNullOrEmpty(isbn))
            {
                MessageBox.Show("Please select a valid book.");
                return;
            }

            if (!_memberId.HasValue)
            {
                MessageBox.Show("You must be logged in to borrow a book.");
                return;
            }

            if (availability <= 0)
            {
                MessageBox.Show("No available copies to borrow.");
                return;
            }

            // Check if already borrowed
            var activeTransaction = DatabaseService.GetActiveTransaction(isbn, _memberId.Value);
            if (activeTransaction != null)
            {
                MessageBox.Show("You have already borrowed this book.");
                return;
            }

            // Create transaction
            var transaction = new Transaction
            {
                ISBN = isbn,
                MemberID = _memberId.Value,
                TransactionDate = DateTime.Now,
                TransactionType = "Borrow",
                DueDate = DateTime.Now.AddDays(14),
                Status = "Active",
                ReturnDate = null
            };

            try
            {
                DatabaseService.AddTransaction(transaction);
                DatabaseService.UpdateBookAvailability(isbn, availability - 1);
                MessageBox.Show("Book borrowed successfully.");
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to borrow book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReserve_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0) {
                MessageBox.Show("Please select a book to reserve.");
                return;
            }

            var isbn = dgvBooks.SelectedRows[0].Cells["ISBN"].Value?.ToString();
            var availability = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["Availability"].Value);

            if (string.IsNullOrEmpty(isbn))
            {
                MessageBox.Show("Please select a valid book.");
                return;
            }

            // Check if already reserved by this member
            var existingReservation = DatabaseService.GetAllReservations()
                .FirstOrDefault(r => r.ISBN == isbn && r.MemberID == _memberId.Value && r.Status == "Active");
            if (existingReservation != null)
            {
                MessageBox.Show("You have already reserved this book.");
                return;
            }

            // Create reservation
            var reservation = new Reservation
            {
                ISBN = isbn,
                MemberID = _memberId.Value,
                ReservationDate = DateTime.Now,
                Status = "Active"
            };

            try
            {
                DatabaseService.AddReservation(reservation);
                MessageBox.Show($"Book reserved successfully!", "Reservation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reserve book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0) return;

            var isbn = dgvBooks.SelectedRows[0].Cells["ISBN"].Value?.ToString();
            var availability = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["Availability"].Value);

            if (string.IsNullOrEmpty(isbn))
            {
                MessageBox.Show("Please select a valid book.");
                return;
            }

            if (!_memberId.HasValue)
            {
                MessageBox.Show("You must be logged in to return a book.");
                return;
            }

            var activeTransaction = DatabaseService.GetActiveTransaction(isbn, _memberId.Value);
            if (activeTransaction == null)
            {
                MessageBox.Show("You have not borrowed this book.");
                return;
            }

            // Update transaction
            activeTransaction.Status = "Returned";
            activeTransaction.ReturnDate = DateTime.Now;
            DatabaseService.UpdateTransaction(activeTransaction);
            DatabaseService.UpdateBookAvailability(isbn, availability + 1);
            MessageBox.Show("Book returned successfully.");
            LoadBooks();
        }

        private void dgvBooks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var isbn = dgvBooks.Rows[e.RowIndex].Cells["ISBN"].Value.ToString();
                var book = DatabaseService.GetBookByISBN(isbn);
                if (book != null)
                {
                    if (_role == "Admin")
                    {
                        using (var form = new BookForm(book))
                        {
                            form.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Only administrators can edit book details.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }
}

