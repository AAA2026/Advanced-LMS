using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagement.GUI
{
    public partial class WelcomeForm : Form
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string? SelectedRole { get; private set; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int? SelectedMemberId { get; private set; } // To store the selected member ID

        public WelcomeForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Welcome to Library Management System";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create main panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(40),
                RowStyles = { new RowStyle(SizeType.Absolute, 120), new RowStyle(SizeType.Percent, 100) }
            };

            // Welcome label
            var lblWelcome = new Label
            {
                Text = "Welcome to Library Management System",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Create buttons panel (centered)
            var buttonsPanel = new TableLayoutPanel
            {
                // Use Anchor instead of Dock to allow centering
                Anchor = AnchorStyles.None, // Center horizontally and vertically
                Size = new Size(640, 240), // Explicit size for the panel containing buttons
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 50),
                    new ColumnStyle(SizeType.Percent, 50)
                },
                Padding = new Padding(10) // Padding inside the button panel
            };

            // Create Admin button
            var btnAdmin = CreateRoleButton("Admin", "👨‍💼", Color.FromArgb(0, 122, 204));
            btnAdmin.Click += BtnAdmin_Click; // Use separate handler

            // Create Member button
            var btnMember = CreateRoleButton("Member", "👤", Color.FromArgb(0, 122, 204));
            btnMember.Click += BtnMember_Click; // Use separate handler

            // Add buttons to panel
            buttonsPanel.Controls.Add(btnAdmin, 0, 0);
            buttonsPanel.Controls.Add(btnMember, 1, 0);

            // Add controls to main panel
            mainPanel.Controls.Add(lblWelcome, 0, 0);
            mainPanel.Controls.Add(buttonsPanel, 0, 1);
            mainPanel.SetRow(buttonsPanel, 1);
            mainPanel.SetColumn(buttonsPanel, 0);

            // Add main panel to form
            this.Controls.Add(mainPanel);
        }

        private Button CreateRoleButton(string role, string icon, Color baseColor)
        {
            var button = new Button
            {
                Text = $"{icon}\n{role}",
                // Size = new Size(300, 200), // Let TableLayoutPanel handle sizing
                FlatStyle = FlatStyle.Flat,
                BackColor = baseColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                Margin = new Padding(10) // Margin around each button
            };

            button.FlatAppearance.BorderSize = 0;
            // Apply rounded corners in Paint event for better resizing handling
            button.Paint += (sender, e) => {
                Button currentButton = sender as Button;
                if (currentButton != null)
                {
                    // Use a path to clip the button to a rounded rectangle
                    System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                    int radius = 20;
                    path.AddArc(0, 0, radius, radius, 180, 90);
                    path.AddArc(currentButton.Width - radius, 0, radius, radius, 270, 90);
                    path.AddArc(currentButton.Width - radius, currentButton.Height - radius, radius, radius, 0, 90);
                    path.AddArc(0, currentButton.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    currentButton.Region = new Region(path);
                }
            };
            // button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 20, 20)); // Less reliable
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 102, 204);
            button.MouseLeave += (s, e) => button.BackColor = baseColor;

            return button;
        }

        private void BtnAdmin_Click(object sender, EventArgs e)
        {
            SelectedRole = "Admin";
            SelectedMemberId = null; // Ensure member ID is null for Admin
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnMember_Click(object sender, EventArgs e)
        {
            // Show the Member Selection Form Modally
            using (var memberSelectionForm = new MemberSelectionForm())
            {
                var dialogResult = memberSelectionForm.ShowDialog(this); // Show as modal dialog

                if (dialogResult == DialogResult.OK)
                {
                    // If a member was selected
                    SelectedRole = "Member";
                    SelectedMemberId = memberSelectionForm.SelectedMemberId;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                // If the user cancelled the selection form (DialogResult.Cancel or closed),
                // stay on the WelcomeForm.
            }
        }

        // Keep DllImport if CreateRoundRectRgn is needed elsewhere, otherwise remove
        // [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        // private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}

