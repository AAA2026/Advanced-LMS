using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public partial class MemberManagementForm : Form
    {
        private DataGridView dgvMembers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private TextBox txtSearch;

        public MemberManagementForm()
        {
            InitializeComponent();
            LoadMembers();
        }

        private void InitializeComponent()
        {
            this.Text = "Member Management";
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
                PlaceholderText = "Search members...",
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
            btnEdit = CreateButton("Edit", "✏️");
            btnDelete = CreateButton("Delete", "🗑️");
            btnRefresh = CreateButton("Refresh", "🔄");

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnEdit);
            buttonsPanel.Controls.Add(btnDelete);
            buttonsPanel.Controls.Add(btnRefresh);

            // Create DataGridView
            dgvMembers = new DataGridView
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
            dgvMembers.CellDoubleClick += dgvMembers_CellDoubleClick;

            // Add controls to main panel
            mainPanel.Controls.Add(searchPanel, 0, 0);
            mainPanel.Controls.Add(buttonsPanel, 0, 1);
            mainPanel.Controls.Add(dgvMembers, 0, 2);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Wire up events
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
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

        private void LoadMembers()
        {
            var members = DatabaseService.GetAllMembers();
            MessageBox.Show($"Members loaded: {members.Count}");
            var displayMembers = members.Select(m => new
            {
                m.MemberID,
                m.Name,
                m.Email,
                Phones = string.Join(", ", m.MemberPhones?.Select(p => p.Phone) ?? new string[0]),
                m.Address
            }).ToList();
            dgvMembers.DataSource = displayMembers;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new MemberForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (DatabaseService.AddMember(form.Member))
                    {
                        LoadMembers();
                    }
                    else
                    {
                        MessageBox.Show("A member with this email already exists.", "Duplicate Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a member to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var memberId = (int)dgvMembers.SelectedRows[0].Cells["MemberID"].Value;
            var member = DatabaseService.GetMemberById(memberId);
            using (var form = new MemberForm(member))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService.UpdateMember(form.Member);
                    LoadMembers();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a member to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var memberId = (int)dgvMembers.SelectedRows[0].Cells["MemberID"].Value;
            var result = MessageBox.Show($"Are you sure you want to delete member with ID {memberId}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DatabaseService.DeleteMember(memberId);
                LoadMembers();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadMembers();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var searchTerm = txtSearch.Text.ToLower();
            var members = DatabaseService.GetAllMembers();
            var filteredMembers = members.Where(m =>
                m.Name.ToLower().Contains(searchTerm) ||
                m.Email.ToLower().Contains(searchTerm) ||
                m.Address.ToLower().Contains(searchTerm) ||
                (m.MemberPhones != null && m.MemberPhones.Any(p => p.Phone.ToLower().Contains(searchTerm)))
            ).Select(m => new
            {
                m.MemberID,
                m.Name,
                m.Email,
                Phones = string.Join(", ", m.MemberPhones?.Select(p => p.Phone) ?? new string[0]),
                m.Address
            }).ToList();
            dgvMembers.DataSource = filteredMembers;
        }

        private void MemberManagementForm_Load(object sender, EventArgs e)
        {
            // Additional initialization code if needed
        }

        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var memberId = (int)dgvMembers.Rows[e.RowIndex].Cells["MemberID"].Value;
                var member = DatabaseService.GetMemberById(memberId);
                using (var form = new MemberViewForm(memberId))
                {
                    form.ShowDialog();
                }
            }
        }
    }
} 