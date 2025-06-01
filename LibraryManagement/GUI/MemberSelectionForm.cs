using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    // This form allows selecting a member from a list.
    public partial class MemberSelectionForm : Form
    {
        private DataGridView dgvMembers;
        private Button btnSelect;
        private Button btnCancel;
        private TextBox txtSearch;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int? SelectedMemberId { get; private set; }

        public MemberSelectionForm()
        {
            InitializeComponent();
            LoadMembers();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Member";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent; // Center relative to parent (WelcomeForm)
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = btnSelect; // Allow Enter key to select
            this.CancelButton = btnCancel; // Allow Esc key to cancel

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20),
                RowStyles = { new RowStyle(SizeType.Absolute, 60), new RowStyle(SizeType.Percent, 100), new RowStyle(SizeType.Absolute, 60) }
            };

            // --- Search Panel ---
            var searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300, // Initial width, anchor handles resizing
                Height = 35,
                Location = new Point(0, 10),
                PlaceholderText = "Search members by name or email...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle // Simple border for clarity
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(txtSearch);

            // --- DataGridView Panel ---
            dgvMembers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false, // Define columns manually for better control
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
            dgvMembers.CellDoubleClick += dgvMembers_CellDoubleClick; // Double-click to select

            // Define Columns
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "MemberID", HeaderText = "ID", DataPropertyName = "MemberID", Width = 50 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name", FillWeight = 100 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", FillWeight = 100 });

            // --- Buttons Panel ---
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft, // Align buttons to the right
                Padding = new Padding(0, 10, 0, 0)
            };

            btnSelect = CreateButton("Select Member", Color.FromArgb(0, 122, 204));
            btnCancel = CreateButton("Cancel", Color.Gray);

            buttonsPanel.Controls.Add(btnCancel); // Add in reverse order for RightToLeft flow
            buttonsPanel.Controls.Add(btnSelect);

            // Add controls to main panel
            mainPanel.Controls.Add(searchPanel, 0, 0);
            mainPanel.Controls.Add(dgvMembers, 0, 1);
            mainPanel.Controls.Add(buttonsPanel, 0, 2);

            this.Controls.Add(mainPanel);

            // Wire up events
            btnSelect.Click += BtnSelect_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private Button CreateButton(string text, Color baseColor)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(140, 40),
                Margin = new Padding(10, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            // Simple hover effect
            button.MouseEnter += (s, e) => button.BackColor = ControlPaint.Dark(baseColor, 0.1f);
            button.MouseLeave += (s, e) => button.BackColor = baseColor;

            return button;
        }

        private void LoadMembers(string searchTerm = null)
        {
            var members = DatabaseService.GetAllMembers(); // Assuming this exists and works

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                members = members.Where(m =>
                    m.Name.ToLower().Contains(searchTerm) ||
                    m.Email.ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Select only necessary fields for display
            var displayMembers = members.Select(m => new
            {
                m.MemberID,
                m.Name,
                m.Email
            }).ToList();

            dgvMembers.DataSource = displayMembers;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMembers(txtSearch.Text);
        }

        private void SelectMemberAndClose()
        {
            if (dgvMembers.SelectedRows.Count > 0)
            {
                SelectedMemberId = (int)dgvMembers.SelectedRows[0].Cells["MemberID"].Value;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a member from the list.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            SelectMemberAndClose();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure double-click is on a valid row
            if (e.RowIndex >= 0)
            {
                SelectMemberAndClose();
            }
        }
    }
}

