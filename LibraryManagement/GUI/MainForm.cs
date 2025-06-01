using System;
using System.Windows.Forms;
using System.Drawing;
using LibraryManagement.Services;
using LibraryManagement.Models; // Needed for Member

namespace LibraryManagement.GUI
{
    public partial class MainForm : Form
    {
        private readonly string _role;
        private readonly int? _memberId; // Store the logged-in member's ID
        private Member _currentMember; // Store the logged-in member's details
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool LoggedOut { get; private set; } // Add LoggedOut property

        private Button btnBooks;
        private Button btnMyReviews; // Changed from btnReviews for Member role
        private Button btnMembers; // Admin only
        private Button btnTransactions; // Admin only
        private Button btnFines; // Both, but context might differ
        private Button btnAllReviews; // Admin only
        private Button btnReports; // Admin only
        private Button btnReservations; // Both
        private Button btnBack;
        private Panel contentPanel;
        private Panel sidebarPanel;
        private Label lblWelcomeUser; // To display member name

        // Constructor updated to accept memberId
        public MainForm(string role, int? memberId = null)
        {
            _role = role;
            _memberId = memberId;

            // Fetch member details immediately if logged in as a member
            if (_role == "Member" && _memberId.HasValue)
            {
                _currentMember = DatabaseService.GetMemberById(_memberId.Value);
                if (_currentMember == null)
                {
                    MessageBox.Show("Error: Could not load member information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
            }

            InitializeComponent();
            // Subscribe to Load event AFTER InitializeComponent
            this.Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Library Management System";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F);

            // --- Sidebar Panel ---
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            // Welcome User Label (Initialize placeholder text)
            lblWelcomeUser = new Label
            {
                Text = "Welcome!", // Placeholder text
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true
            };
            sidebarPanel.Controls.Add(lblWelcomeUser);

            // Back Button
            btnBack = CreateNavButton("← Logout", -1); // Changed text
            btnBack.Location = new Point(10, 50);
            sidebarPanel.Controls.Add(btnBack);

            // --- Navigation Buttons ---
            int buttonIndex = 0;
            // Buttons are added based on role later in this method
            btnBooks = CreateNavButton("📚 Books", buttonIndex++);

            if (_role == "Admin")
            {
                btnMembers = CreateNavButton("👥 Members", buttonIndex++);
                btnTransactions = CreateNavButton("🔄 Transactions", buttonIndex++);
                btnAllReviews = CreateNavButton("⭐ All Reviews", buttonIndex++);
                btnReports = CreateNavButton("📊 Reports", buttonIndex++);
            }
            else if (_role == "Member")
            {
                btnMyReviews = CreateNavButton("⭐ My Reviews", buttonIndex++);
            }

            btnFines = CreateNavButton("💲 Fines", buttonIndex++);
            btnReservations = CreateNavButton("📅 Reservations", buttonIndex++);

            // Add buttons to sidebar in correct order
            sidebarPanel.Controls.Add(btnBooks);
            if (_role == "Admin")
            {
                sidebarPanel.Controls.Add(btnMembers);
                sidebarPanel.Controls.Add(btnTransactions);
                sidebarPanel.Controls.Add(btnAllReviews);
                sidebarPanel.Controls.Add(btnReports);
            }
            else if (_role == "Member")
            {
                sidebarPanel.Controls.Add(btnMyReviews);
            }
            sidebarPanel.Controls.Add(btnFines);
            sidebarPanel.Controls.Add(btnReservations);


            // --- Content Panel ---
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);

            // --- Wire up events ---
            btnBack.Click += BtnBack_Click;
            btnBooks.Click += BtnBooks_Click;
            btnFines.Click += BtnFines_Click;
            btnReservations.Click += BtnReservations_Click;

            if (_role == "Admin")
            {
                btnMembers.Click += BtnMembers_Click;
                btnTransactions.Click += BtnTransactions_Click;
                btnAllReviews.Click += BtnAllReviews_Click;
                btnReports.Click += BtnReports_Click;
            }
            if (_role == "Member")
            {
                btnMyReviews.Click += BtnMyReviews_Click;
            }
        }

        private Button CreateNavButton(string text, int index)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(230, 45),
                // Adjust location based on index, starting below welcome/back
                Location = new Point(10, 100 + (index * 55)),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 48),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right // Allow resizing
            };

            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(0, 122, 204);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(45, 45, 48);

            return button;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set the welcome message text here, ensuring _currentMember is populated
            if (_role == "Member" && _currentMember != null)
            {
                lblWelcomeUser.Text = $"Welcome, {_currentMember.Name}!";
            }
            else if (_role == "Admin")
            {
                lblWelcomeUser.Text = "Welcome, Admin!";
            }
            else
            {
                 lblWelcomeUser.Text = "Welcome!"; // Fallback
            }

            // Show default view based on role
            if (_role == "Admin")
            {
                ShowBooksManagement(); // Admin default
            }
            else if (_role == "Member")
            {
                ShowBooksList(); // Member default
            }
        }

        // Helper to clear content panel and add a form
        private void LoadFormIntoContentPanel(Form form)
        {
            contentPanel.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(form);
            form.Show();
        }

        // --- Methods to Show Different Forms/Views ---

        private void ShowBooksManagement() // Admin view
        {
            // Admin doesn't need memberId passed here
            LoadFormIntoContentPanel(new BookManagementForm(_role));
        }

        private void ShowBooksList() // Member view
        {
            // Pass the memberId to the BookManagementForm for member context
            LoadFormIntoContentPanel(new BookManagementForm(_role, _memberId));
        }

        private void ShowMembersManagement() // Admin only
        {
            LoadFormIntoContentPanel(new MemberManagementForm());
        }

        private void ShowTransactionsManagement() // Admin only
        {
            // Assuming TransactionManagementForm might need role but not memberId for admin
            LoadFormIntoContentPanel(new TransactionManagementForm(_role));
        }

        private void ShowFinesManagement() // Role-aware
        {
            // Pass memberId if the user is a member
            LoadFormIntoContentPanel(new FineManagementForm(_role, _memberId));
        }

        private void ShowAllReviewsManagement() // Admin only
        {
            // Admin sees all reviews, no memberId needed
            LoadFormIntoContentPanel(new ReviewsManagementForm(_role));
        }

        private void ShowMyReviews() // Member only
        {
            if (_memberId.HasValue)
            {
                // Pass memberId to show only their reviews or allow adding new ones
                LoadFormIntoContentPanel(new ReviewsManagementForm(_role, _memberId));
            }
            else
            {
                MessageBox.Show("Member context is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowReports() // Admin only
        {
            LoadFormIntoContentPanel(new ReportGenerationForm(_role));
        }

        private void ShowReservationsManagement() // Role-aware
        {
            // Pass memberId if the user is a member
            LoadFormIntoContentPanel(new ReservationManagementForm(_role, _memberId));
        }

        // --- Button Click Events ---

        private void BtnBooks_Click(object sender, EventArgs e)
        {
            if (_role == "Admin") ShowBooksManagement();
            else ShowBooksList();
        }

        private void BtnMembers_Click(object sender, EventArgs e) // Admin only
        {
            ShowMembersManagement();
        }

        private void BtnTransactions_Click(object sender, EventArgs e) // Admin only
        {
            ShowTransactionsManagement();
        }

        private void BtnFines_Click(object sender, EventArgs e)
        {
            ShowFinesManagement();
        }

        private void BtnAllReviews_Click(object sender, EventArgs e) // Admin only
        {
            ShowAllReviewsManagement();
        }

        private void BtnMyReviews_Click(object sender, EventArgs e) // Member only
        {
            ShowMyReviews();
        }

        private void BtnReports_Click(object sender, EventArgs e) // Admin only
        {
            ShowReports();
        }

        private void BtnReservations_Click(object sender, EventArgs e)
        {
            ShowReservationsManagement();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.LoggedOut = true; // Set LoggedOut to true when user clicks logout
            this.Close(); // Close the form, which will return to the welcome screen
        }
    }
}

