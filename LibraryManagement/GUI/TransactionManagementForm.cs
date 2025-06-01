using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public partial class TransactionManagementForm : Form
    {
        private DataGridView dgvTransactions;
        private Button btnAdd;
        private Button btnReturn;
        private Button btnRefresh;
        private TextBox txtSearch;
        private string _role;

        public TransactionManagementForm(string role)
        {
            _role = role;
            InitializeComponent();
            LoadTransactions();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Transaction Management";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Create search panel
            var searchPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                Width = 200,
                Height = 35,
                Location = new Point(10, 10),
                PlaceholderText = "Search transactions...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Create rounded search box
            var searchBox = new Panel
            {
                Width = 220,
                Height = 35,
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            searchBox.Controls.Add(txtSearch);
            searchBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, searchBox.Width, searchBox.Height, 15, 15));

            searchPanel.Controls.Add(searchBox);

            // Create buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                BackColor = Color.White,
                WrapContents = false
            };

            btnAdd = CreateButton("Add", "➕");
            btnReturn = CreateButton("Return", "↩️");
            btnRefresh = CreateButton("Refresh", "🔄");

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnReturn);
            buttonsPanel.Controls.Add(btnRefresh);

            // Create DataGridView
            dgvTransactions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 245, 245) }
            };

            // Add controls to main panel
            mainPanel.Controls.Add(searchPanel, 0, 0);
            mainPanel.Controls.Add(buttonsPanel, 0, 1);
            mainPanel.Controls.Add(dgvTransactions, 0, 2);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Wire up events
            btnAdd.Click += BtnAdd_Click;
            btnReturn.Click += BtnReturn_Click;
            btnRefresh.Click += BtnRefresh_Click;

            // Initially disable Return button
            btnReturn.Enabled = false;
            dgvTransactions.SelectionChanged += dgvTransactions_SelectionChanged;
        }

        private Button CreateButton(string text, string icon)
        {
            var button = new Button
            {
                Text = $"{icon} {text}",
                Size = new Size(120, 35),
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

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void LoadTransactions()
        {
            var transactions = DatabaseService.GetAllTransactions();
            var displayTransactions = transactions.Select(t => new
            {
                t.TransactionID,
                BookTitle = DatabaseService.GetBookByISBN(t.ISBN)?.Title ?? "Unknown",
                MemberName = DatabaseService.GetMemberById(t.MemberID)?.Name ?? "Unknown",
                t.TransactionDate,
                t.DueDate,
                t.ReturnDate,
                t.Status
            }).ToList();
            dgvTransactions.DataSource = displayTransactions;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new TransactionForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService.AddTransaction(form.Transaction);
                    LoadTransactions();
                }
            }
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a transaction to return.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var transactionId = (int)dgvTransactions.SelectedRows[0].Cells["TransactionID"].Value;
            var transaction = DatabaseService.GetTransactionById(transactionId);

            if (transaction != null && transaction.Status != "Returned")
            {
                transaction.ReturnDate = DateTime.Now;
                transaction.Status = "Returned";
                DatabaseService.UpdateTransaction(transaction);
                DatabaseService.UpdateOverdueFines(); // Update fines after return
                MessageBox.Show("Book returned successfully!", "Return Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTransactions();
            }
            else
            {
                MessageBox.Show("Selected transaction is already returned or invalid.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var searchTerm = txtSearch.Text.ToLower();
            var transactions = DatabaseService.GetAllTransactions();
            var filteredTransactions = transactions.Where(t =>
                (DatabaseService.GetBookByISBN(t.ISBN)?.Title?.ToLower().Contains(searchTerm) ?? false) ||
                (DatabaseService.GetMemberById(t.MemberID)?.Name?.ToLower().Contains(searchTerm) ?? false) ||
                t.Status.ToLower().Contains(searchTerm)
            ).Select(t => new
            {
                t.TransactionID,
                BookTitle = DatabaseService.GetBookByISBN(t.ISBN)?.Title ?? "Unknown",
                MemberName = DatabaseService.GetMemberById(t.MemberID)?.Name ?? "Unknown",
                t.TransactionDate,
                t.DueDate,
                t.ReturnDate,
                t.Status
            }).ToList();

            dgvTransactions.DataSource = filteredTransactions;
        }

        private void dgvTransactions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count > 0)
            {
                var status = dgvTransactions.SelectedRows[0].Cells["Status"].Value.ToString();
                btnReturn.Enabled = status != "Returned";
            }
            else
            {
                btnReturn.Enabled = false;
            }
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member" || _role == "Guest")
            {
                btnAdd.Visible = false;
                // btnReturn and btnRefresh should be visible for members to view their transactions
            }
            else if (_role == "Admin")
            {
                btnAdd.Visible = true;
                // btnReturn and btnRefresh visible for admin
            }
        }
    }
} 