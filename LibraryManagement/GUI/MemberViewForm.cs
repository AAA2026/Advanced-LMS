using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public partial class MemberViewForm : Form
    {
        private int _memberId;

        public MemberViewForm(int memberId)
        {
            InitializeComponent();
            _memberId = memberId;
            LoadMemberData();
        }

        private void InitializeComponent()
        {
            this.Text = "Member Details";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create a panel to hold the buttons
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                AutoSize = true
            };

            // Create buttons
            var btnViewBooks = new Button { Text = "View All Books", Size = new System.Drawing.Size(120, 30) };
            var btnReserveBook = new Button { Text = "Reserve/Borrow Book", Size = new System.Drawing.Size(150, 30) };
            var btnReviewBook = new Button { Text = "Review Book", Size = new System.Drawing.Size(120, 30) };
            var btnPayFine = new Button { Text = "Pay Fine", Size = new System.Drawing.Size(120, 30) };

            // Add buttons to the panel
            buttonsPanel.Controls.Add(btnViewBooks);
            buttonsPanel.Controls.Add(btnReserveBook);
            buttonsPanel.Controls.Add(btnReviewBook);
            buttonsPanel.Controls.Add(btnPayFine);

            // Add the panel to the form
            this.Controls.Add(buttonsPanel);

            // Wire up button click events
            btnViewBooks.Click += BtnViewBooks_Click;
            btnReserveBook.Click += BtnReserveBook_Click;
            btnReviewBook.Click += BtnReviewBook_Click;
            btnPayFine.Click += BtnPayFine_Click;
        }

        private void LoadMemberData()
        {
            Member member = DatabaseService.GetMemberById(_memberId);
            if (member != null)
            {
                this.Text = $"Member Details: {member.Name}";
                // TODO: Populate form with member data and add member-specific controls
            }
            else
            {
                MessageBox.Show("Member not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void BtnViewBooks_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to show all books
            // MessageBox.Show("View All Books clicked!");
            using (var bookListForm = new BookListForm(_memberId))
            {
                bookListForm.ShowDialog();
            }
        }

        private void BtnReserveBook_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to reserve or borrow a book
            MessageBox.Show("Reserve/Borrow Book clicked!");
        }

        private void BtnReviewBook_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to review a book
            MessageBox.Show("Review Book clicked!");
        }

        private void BtnPayFine_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to pay a fine
            // MessageBox.Show("Pay Fine clicked!");
            using (var finePaymentForm = new FinePaymentForm(_memberId))
            {
                finePaymentForm.ShowDialog();
            }
        }
    }
} 