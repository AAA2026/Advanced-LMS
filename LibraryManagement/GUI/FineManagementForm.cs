using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public class FineManagementForm : Form
    {
        private DataGridView dgvFines;
        private Button btnAdd;
        private Button btnPay;
        private Button btnRefresh;
        private TextBox txtSearch;
        private string _role;
        private int? _memberId;

        public FineManagementForm(string role, int? memberId = null)
        {
            _role = role;
            _memberId = memberId;
            InitializeComponent();
            LoadFines();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            // Manual UI Initialization
            this.Text = "Fine Management";
            this.Size = new Size(800, 600);
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
                PlaceholderText = "Search fines...",
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
            // Note: CreateRoundRectRgn requires System.Drawing.Drawing2D, which might not be included or set up.
            // For simplicity, we might omit the rounded corners initially if it causes issues.
            // searchBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, searchBox.Width, searchBox.Height, 15, 15));

            searchBox.Controls.Add(txtSearch);

            searchPanel.Controls.Add(searchBox);

            // Create buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                BackColor = Color.White,
                WrapContents = false
            };

            btnAdd = CreateButton("Add", "➕");
            btnPay = CreateButton("Pay Fine", "💳");
            btnRefresh = CreateButton("Refresh", "🔄");

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnPay);
            buttonsPanel.Controls.Add(btnRefresh);

            // Create DataGridView
            dgvFines = new DataGridView
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
            mainPanel.Controls.Add(dgvFines, 0, 2);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Wire up events
            btnAdd.Click += BtnAdd_Click;
            btnPay.Click += BtnPay_Click;
            btnRefresh.Click += BtnRefresh_Click;

            // Initially disable Pay button
            btnPay.Enabled = false;
            dgvFines.SelectionChanged += DgvFines_SelectionChanged;
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
            // button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 20, 20)); // Omitted due to potential complexity
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 102, 204);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(0, 122, 204);

            return button;
        }

        // Omitted CreateRoundRectRgn DllImport for simplicity
        // [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        // private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void LoadFines()
        {
            var fines = DatabaseService.GetAllFines();
            dgvFines.DataSource = fines;
            // Column configuration might need adjustment based on actual data properties
             if (dgvFines.Columns.Contains("FineID")) dgvFines.Columns["FineID"].Visible = false;
             if (dgvFines.Columns.Contains("TransactionID")) dgvFines.Columns["TransactionID"].Visible = false;
             if (dgvFines.Columns.Contains("Transaction")) dgvFines.Columns["Transaction"].Visible = false;
             if (dgvFines.Columns.Contains("Amount")) dgvFines.Columns["Amount"].DefaultCellStyle.Format = "C2";
             if (dgvFines.Columns.Contains("IssuedDate")) dgvFines.Columns["IssuedDate"].DefaultCellStyle.Format = "g";
             if (dgvFines.Columns.Contains("PaymentDate")) dgvFines.Columns["PaymentDate"].DefaultCellStyle.Format = "g";
             if (dgvFines.Columns.Contains("Status")) dgvFines.Columns["Status"].HeaderText = "Status";
             if (dgvFines.Columns.Contains("Reason")) dgvFines.Columns["Reason"].HeaderText = "Reason";
             if (dgvFines.Columns.Contains("Amount")) dgvFines.Columns["Amount"].HeaderText = "Amount";
             if (dgvFines.Columns.Contains("IssuedDate")) dgvFines.Columns["IssuedDate"].HeaderText = "Issued Date";
             if (dgvFines.Columns.Contains("PaymentDate")) dgvFines.Columns["PaymentDate"].HeaderText = "Payment Date";
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvFines.SelectedRows.Count > 0;
            btnPay.Enabled = hasSelection;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new FineForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadFines();
            }
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (dgvFines.SelectedRows.Count > 0)
            {
                var fine = (Fine)dgvFines.SelectedRows[0].DataBoundItem;
                if (fine.Status != "Paid")
                {
                    fine.Status = "Paid";
                    fine.PaymentDate = DateTime.Now;
                    DatabaseService.UpdateFine(fine);
                    LoadFines();
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadFines();
        }

        private void DgvFines_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var fines = DatabaseService.GetAllFines();
            var filtered = fines.Where(f => 
                f.Transaction?.Book?.Title?.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) == true ||
                f.Transaction?.Member?.Name?.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) == true ||
                f.Reason?.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
            dgvFines.DataSource = filtered;
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member" || _role == "Guest")
            {
                btnAdd.Visible = false;
                // btnPay button is for members, so it remains visible
            }
            else if (_role == "Admin")
            {
                btnAdd.Visible = true;
                // btnPay button is visible for admin as well to manage payments
            }
        }
    }
} 