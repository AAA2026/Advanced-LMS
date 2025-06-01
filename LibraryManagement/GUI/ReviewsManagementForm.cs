using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Services;
using LibraryManagement.Models;
using System.Collections.Generic;

namespace LibraryManagement.GUI
{
    public partial class ReviewsManagementForm : Form
    {
        private DataGridView dgvReviews;
        private Button btnAdd;
        private Button btnEdit; // Keep for Admin, maybe hide for Member?
        private Button btnDelete; // Keep for Admin, maybe hide for Member?
        private Button btnRefresh;
        private TextBox txtSearch;
        private string _role;
        private int? _memberId; // Store the logged-in member ID

        // Constructor for Admin (no memberId needed)
        public ReviewsManagementForm(string role)
            : this(role, null) // Call the main constructor
        {
        }

        // Main constructor accepting optional memberId
        public ReviewsManagementForm(string role, int? memberId)
        {
            _role = role;
            _memberId = memberId;
            InitializeComponent();
            LoadReviews();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Text = _role == "Member" ? "My Reviews" : "Reviews Management";
            this.Size = new Size(900, 600);
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
                Dock = DockStyle.Fill, // Fill the row
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300, // Initial width
                Height = 35,
                Location = new Point(10, 10),
                PlaceholderText = "Search reviews...",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(txtSearch);

            // Create buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, // Fill the row
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 5, 0, 5),
                BackColor = Color.White,
                WrapContents = false,
                AutoSize = true // Let it size based on buttons
            };

            btnAdd = CreateButton("Add Review", "➕");
            btnEdit = CreateButton("Edit Review", "✏️"); // Maybe only allow editing own review for member?
            btnDelete = CreateButton("Delete Review", "🗑️"); // Maybe only allow deleting own review for member?
            btnRefresh = CreateButton("Refresh", "🔄");

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnEdit);
            buttonsPanel.Controls.Add(btnDelete);
            buttonsPanel.Controls.Add(btnRefresh);

            // Create DataGridView
            dgvReviews = new DataGridView
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
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReviewID", HeaderText = "ID", DataPropertyName = "ReviewID", Width = 50 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { Name = "BookTitle", HeaderText = "Book Title", DataPropertyName = "BookTitle", FillWeight = 100 });
            if (_role == "Admin") // Only show Member Name for Admin
            {
                dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { Name = "MemberName", HeaderText = "Member", DataPropertyName = "MemberName", FillWeight = 80 });
            }
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rating", HeaderText = "Rating", DataPropertyName = "Rating", Width = 60 });
            dgvReviews.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReviewText", HeaderText = "Review", DataPropertyName = "ReviewText", FillWeight = 150 });

            // Add controls to main panel
            mainPanel.Controls.Add(searchPanel, 0, 0);
            mainPanel.Controls.Add(buttonsPanel, 0, 1);
            mainPanel.Controls.Add(dgvReviews, 0, 2);

            this.Controls.Add(mainPanel);

            // Wire up events
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            dgvReviews.SelectionChanged += DgvReviews_SelectionChanged;
        }

        private Button CreateButton(string text, string icon)
        {
            var button = new Button
            {
                Text = $"{icon} {text}",
                Size = new Size(140, 35), // Slightly wider buttons
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

        private void LoadReviews()
        {
            IEnumerable<Review> reviews;
            if (_role == "Member" && _memberId.HasValue)
            {
                // Load only reviews for the current member
                reviews = DatabaseService.GetReviewsByMemberId(_memberId.Value);
            }
            else
            {
                // Admin loads all reviews
                reviews = DatabaseService.GetAllReviews();
            }

            var displayReviews = reviews.Select(r => new
            {
                r.ReviewID,
                BookTitle = DatabaseService.GetBookByISBN(r.ISBN)?.Title ?? "Unknown",
                MemberName = _role == "Admin" ? (DatabaseService.GetMemberById(r.MemberID)?.Name ?? "Unknown") : null, // Only needed for Admin view
                r.Rating,
                r.ReviewText
            }).ToList();
            dgvReviews.DataSource = displayReviews;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_role == "Member" && !_memberId.HasValue)
            {
                MessageBox.Show("Cannot add review without member context.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // For Member: Directly use _memberId. Prompt for ISBN.
            // For Admin: Prompt for both ISBN and MemberID.

            string isbn = PromptForISBN();
            if (string.IsNullOrWhiteSpace(isbn))
                return; // User cancelled or entered nothing

            int memberIdToAddReviewFor;
            if (_role == "Member")
            {
                memberIdToAddReviewFor = _memberId.Value;
            }
            else // Admin needs to specify the member
            {
                memberIdToAddReviewFor = PromptForMemberID();
                if (memberIdToAddReviewFor <= 0)
                    return; // User cancelled or entered invalid ID
            }

            // Check if book and member exist
            if (DatabaseService.GetBookByISBN(isbn) == null)
            {
                MessageBox.Show("Book with this ISBN not found.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (DatabaseService.GetMemberById(memberIdToAddReviewFor) == null)
            {
                MessageBox.Show("Member with this ID not found.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if this member already reviewed this book
            if (DatabaseService.GetReviewByMemberAndBook(memberIdToAddReviewFor, isbn) != null)
            {
                 MessageBox.Show("You have already reviewed this book. You can edit your existing review.", "Review Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                 return;
            }

            using (var form = new ReviewForm(isbn, _memberId ?? -1))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService.AddReview(form.Review);
                    LoadReviews();
                }
            }
        }

        // Helper method to prompt for ISBN
        private string PromptForISBN()
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "Enter Book ISBN";
                inputForm.Size = new Size(300, 130);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var layoutPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 2, ColumnCount = 2 };
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                var isbnLabel = new Label { Text = "ISBN:", Anchor = AnchorStyles.Left, AutoSize = true };
                var isbnTextBox = new TextBox { Dock = DockStyle.Fill };

                var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Right };
                var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Right };

                var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(okButton);

                layoutPanel.Controls.Add(isbnLabel, 0, 0);
                layoutPanel.Controls.Add(isbnTextBox, 1, 0);
                layoutPanel.Controls.Add(buttonPanel, 0, 1);
                layoutPanel.SetColumnSpan(buttonPanel, 2);

                inputForm.Controls.Add(layoutPanel);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                return inputForm.ShowDialog() == DialogResult.OK ? isbnTextBox.Text : null;
            }
        }

        // Helper method for Admin to prompt for Member ID
        private int PromptForMemberID()
        {
             using (var inputForm = new Form())
            {
                inputForm.Text = "Enter Member ID";
                inputForm.Size = new Size(300, 130);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var layoutPanel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 2, ColumnCount = 2 };
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                var memberLabel = new Label { Text = "Member ID:", Anchor = AnchorStyles.Left, AutoSize = true };
                var memberNumericUpDown = new NumericUpDown { Minimum = 1, Maximum = 99999, Width = 100, Dock = DockStyle.Fill };

                var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Right };
                var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Right };

                var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(okButton);

                layoutPanel.Controls.Add(memberLabel, 0, 0);
                layoutPanel.Controls.Add(memberNumericUpDown, 1, 0);
                layoutPanel.Controls.Add(buttonPanel, 0, 1);
                layoutPanel.SetColumnSpan(buttonPanel, 2);

                inputForm.Controls.Add(layoutPanel);
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                return inputForm.ShowDialog() == DialogResult.OK ? (int)memberNumericUpDown.Value : -1;
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvReviews.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a review to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var reviewId = (int)dgvReviews.SelectedRows[0].Cells["ReviewID"].Value;
            var review = DatabaseService.GetReview(reviewId);

            // Member can only edit their own reviews
            if (_role == "Member" && review.MemberID != _memberId)
            {
                MessageBox.Show("You can only edit your own reviews.", "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Use the existing ReviewForm, passing the review details
            // Assuming ReviewForm constructor can handle loading existing review data
            using (var form = new ReviewForm(review)) // Assuming ReviewForm has a constructor accepting a Review object
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Update the existing review from the form data
                    DatabaseService.UpdateReview(form.Review); // Assuming form.Review holds the updated data
                    LoadReviews();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvReviews.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a review to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var reviewId = (int)dgvReviews.SelectedRows[0].Cells["ReviewID"].Value;
            var review = DatabaseService.GetReview(reviewId);

            // Member can only delete their own reviews
            if (_role == "Member" && review.MemberID != _memberId)
            {
                MessageBox.Show("You can only delete your own reviews.", "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this review?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DatabaseService.DeleteReview(reviewId);
                LoadReviews();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReviews();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Reload reviews with the search term
            // The LoadReviews method needs to be adapted to handle search term filtering
            // For simplicity, this basic search might need refinement based on LoadReviews implementation
            LoadReviews(); // Re-call LoadReviews, which should ideally handle the filter
            // Or implement filtering logic directly here if LoadReviews doesn't handle it
            var searchTerm = txtSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchTerm) && dgvReviews.DataSource is List<dynamic> currentData)
            {
                 var filteredData = currentData.Where(r =>
                    ((string)r.BookTitle)?.ToLower().Contains(searchTerm) == true ||
                    (_role == "Admin" && ((string)r.MemberName)?.ToLower().Contains(searchTerm) == true) ||
                    ((string)r.ReviewText)?.ToLower().Contains(searchTerm) == true
                 ).ToList();
                 dgvReviews.DataSource = filteredData;
            }
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member")
            {
                btnAdd.Visible = true; // Members can add reviews
                // Edit and Delete buttons will be shown/hidden dynamically based on selection
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
            else if (_role == "Admin")
            {
                btnAdd.Visible = true;
                btnEdit.Visible = true;
                btnDelete.Visible = true;
            }
            else // Guest or other roles
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
        }

        private void UpdateEditDeleteButtonsVisibility()
        {
            if (_role == "Member" && dgvReviews.SelectedRows.Count > 0)
            {
                var reviewId = (int)dgvReviews.SelectedRows[0].Cells["ReviewID"].Value;
                var review = DatabaseService.GetReview(reviewId);
                
                // Show edit/delete buttons only if the member is the author of the review
                bool isAuthor = review?.MemberID == _memberId;
                btnEdit.Visible = isAuthor;
                btnDelete.Visible = isAuthor;
            }
        }

        private void DgvReviews_SelectionChanged(object sender, EventArgs e)
        {
            UpdateEditDeleteButtonsVisibility();
        }
    }
}

