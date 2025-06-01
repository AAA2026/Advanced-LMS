using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.ComponentModel;

namespace LibraryManagement.GUI
{
    public partial class ReviewForm : Form
    {
        private Review _review;
        public Review Review => _review;

        private string _isbn;
        private int _memberId;

        public ReviewForm(string isbn, int memberId)
        {
            InitializeComponent();
            _isbn = isbn;
            _memberId = memberId;
        }

        public ReviewForm(Review review)
        {
            InitializeComponent();
            _review = review;
            _isbn = review.ISBN;
            _memberId = review.MemberID;
            // Optionally, pre-fill UI fields if needed
        }

        private void InitializeComponent()
        {
            this.Text = "Write a Review";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 4,
                ColumnCount = 1
            };
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var ratingLabel = new Label { Text = "Rating (1-5):" };
            var ratingNumericUpDown = new NumericUpDown { Minimum = 1, Maximum = 5, Width = 50 };

            var reviewLabel = new Label { Text = "Your Review:" };
            var reviewTextBox = new TextBox { Multiline = true, Dock = DockStyle.Fill };

            var saveButton = new Button { Text = "Save Review", DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);

            layoutPanel.Controls.Add(ratingLabel);
            layoutPanel.Controls.Add(ratingNumericUpDown);
            layoutPanel.Controls.Add(reviewLabel);
            layoutPanel.Controls.Add(reviewTextBox);
            layoutPanel.Controls.Add(buttonPanel);

            this.Controls.Add(layoutPanel);

            // Handle Save button click
            saveButton.Click += (sender, e) =>
            {
                _review = new Review
                {
                    ISBN = _isbn,
                    MemberID = _memberId,
                    ReviewText = reviewTextBox.Text,
                    Rating = (int)ratingNumericUpDown.Value
                };
            };
        }
    }
} 