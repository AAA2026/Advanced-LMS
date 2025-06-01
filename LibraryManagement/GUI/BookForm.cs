using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.ComponentModel;
using System.Collections.Generic;

namespace LibraryManagement.GUI
{
    public partial class BookForm : Form
    {
        private readonly bool _isEdit;
        private TextBox txtTitle;
        private TextBox txtISBN;
        private TextBox txtPublisher;
        private NumericUpDown numYear;
        private NumericUpDown numPageCount;
        private TextBox txtLanguage;
        private NumericUpDown numAvailability;
        private RichTextBox rtbDescription;
        private CheckedListBox clbAuthors;
        private CheckedListBox clbGenres;
        private Button btnSave;
        private Button btnCancel;
        private TextBox txtNewAuthor;
        private Label lblNewAuthor;
        private TextBox txtNewGenre;
        private Label lblNewGenre;
        private Book? _book;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Book Book => _book ?? new Book();

        public BookForm(Book? book = null)
        {
            InitializeComponent();
            _book = book;
            _isEdit = book != null;
            if (_book != null)
            {
                Text = "Edit Book";
                txtTitle.Text = _book.Title;
                txtISBN.Text = _book.ISBN;
                txtISBN.ReadOnly = true; // Prevent changing ISBN on edit
                txtPublisher.Text = _book.Publisher;
                numYear.Value = _book.PublicationYear;
                numPageCount.Value = _book.PageCount;
                txtLanguage.Text = _book.Language;
                numAvailability.Value = _book.Availability;
                rtbDescription.Text = _book.Description;
            }
            else
            {
                Text = "Add Book";
            }

            LoadAuthorsAndGenres();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Book Details";

            // Initialize controls
            txtTitle = new TextBox();
            txtISBN = new TextBox();
            txtPublisher = new TextBox();
            numYear = new NumericUpDown();
            numPageCount = new NumericUpDown();
            txtLanguage = new TextBox();
            numAvailability = new NumericUpDown();
            rtbDescription = new RichTextBox();
            clbAuthors = new CheckedListBox();
            clbGenres = new CheckedListBox();
            btnSave = new Button();
            btnCancel = new Button();
            txtNewAuthor = new TextBox();
            lblNewAuthor = new Label();
            txtNewGenre = new TextBox();
            lblNewGenre = new Label();

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 16,
                Padding = new Padding(10),
                AutoScroll = true
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Labels and controls
            AddRow(mainPanel, "Title:", txtTitle, 0);
            AddRow(mainPanel, "ISBN:", txtISBN, 1);
            AddRow(mainPanel, "Publisher:", txtPublisher, 2);
            AddRow(mainPanel, "Publication Year:", numYear, 3);
            numYear.Minimum = 0;
            numYear.Maximum = DateTime.Now.Year;
            AddRow(mainPanel, "Page Count:", numPageCount, 4);
            numPageCount.Minimum = 0;
            AddRow(mainPanel, "Language:", txtLanguage, 5);
            AddRow(mainPanel, "Availability:", numAvailability, 6);
            numAvailability.Minimum = 0;

            // Description
            var lblDescription = new Label { Text = "Description:", Font = new Font("Segoe UI", 10), AutoSize = true };
            mainPanel.Controls.Add(lblDescription, 0, 7);
            mainPanel.Controls.Add(rtbDescription, 1, 7);
            rtbDescription.Dock = DockStyle.Fill;
            mainPanel.SetRowSpan(rtbDescription, 2);

            // Authors
            var lblAuthors = new Label { Text = "Authors:", Font = new Font("Segoe UI", 10), AutoSize = true };
            mainPanel.Controls.Add(lblAuthors, 0, 9);
            mainPanel.Controls.Add(clbAuthors, 1, 9);
            clbAuthors.Dock = DockStyle.Fill;
            mainPanel.SetRowSpan(clbAuthors, 2);

            // Genres
            var lblGenres = new Label { Text = "Genres:", Font = new Font("Segoe UI", 10), AutoSize = true };
            mainPanel.Controls.Add(lblGenres, 0, 10);
            mainPanel.Controls.Add(clbGenres, 1, 10);
            clbGenres.Dock = DockStyle.Fill;
            mainPanel.SetRowSpan(clbGenres, 2);

            // New Author
            AddRow(mainPanel, "Add New Author:", txtNewAuthor, 11);

            // New Genre
            AddRow(mainPanel, "Add New Genre:", txtNewGenre, 12);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                AutoSize = true
            };

            btnSave = new Button { Text = "Save" };
            btnCancel = new Button { Text = "Cancel" };

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            // Add panels to form
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Wire up events
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void AddRow(TableLayoutPanel panel, string labelText, Control control, int row)
        {
            var label = new Label { Text = labelText, Font = new Font("Segoe UI", 10), AutoSize = true, Anchor = AnchorStyles.Left };
            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(control, 1, row);
            control.Dock = DockStyle.Fill;
        }

        private void LoadAuthorsAndGenres()
        {
            var allAuthors = DatabaseService.GetAllAuthors();
            clbAuthors.DataSource = allAuthors;
            clbAuthors.DisplayMember = "Name";
            clbAuthors.ValueMember = "AuthorID";

            var allGenres = DatabaseService.GetAllGenres();
            clbGenres.DataSource = allGenres;
            clbGenres.DisplayMember = "Name";
            clbGenres.ValueMember = "GenreID";

            if (_isEdit)
            {
                // Check authors
                foreach (var author in _book.BookAuthors)
                {
                    for (int i = 0; i < clbAuthors.Items.Count; i++)
                    {
                        var item = (Author)clbAuthors.Items[i];
                        if (item.AuthorID == author.AuthorID)
                        {
                            clbAuthors.SetItemChecked(i, true);
                            break;
                        }
                    }
                }

                // Check genres
                foreach (var genre in _book.BookGenres)
                {
                    for (int i = 0; i < clbGenres.Items.Count; i++)
                    {
                        var item = (Genre)clbGenres.Items[i];
                        if (item.GenreID == genre.GenreID)
                        {
                            clbGenres.SetItemChecked(i, true);
                            break;
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            // If adding a new book, create a new Book object
            if (_book == null)
            {
                _book = new Book();
            }

            _book.Title = txtTitle.Text;
            _book.ISBN = txtISBN.Text;
            _book.Publisher = txtPublisher.Text;
            _book.PublicationYear = (int)numYear.Value;
            _book.PageCount = (int)numPageCount.Value;
            _book.Language = txtLanguage.Text;
            _book.Availability = (int)numAvailability.Value;
            _book.Description = rtbDescription.Text;

            // Handle new author
            if (!string.IsNullOrWhiteSpace(txtNewAuthor.Text))
            {
                var newAuthor = new Author { Name = txtNewAuthor.Text.Trim() };
                int authorId = DatabaseService.AddAuthor(newAuthor);
                // Reload authors to include the new one and get its ID
                LoadAuthorsAndGenres();
                // Find and select the newly added author
                for (int i = 0; i < clbAuthors.Items.Count; i++)
                {
                    var author = (Author)clbAuthors.Items[i];
                    if (author.AuthorID == authorId)
                    {
                        clbAuthors.SetItemChecked(i, true);
                        break;
                    }
                }
                txtNewAuthor.Clear();
            }

            // Handle new genre
            if (!string.IsNullOrWhiteSpace(txtNewGenre.Text))
            {
                var newGenre = new Genre { Name = txtNewGenre.Text.Trim() };
                int genreId = DatabaseService.AddGenre(newGenre);
                // Reload genres to include the new one and get its ID
                LoadAuthorsAndGenres();
                // Find and select the newly added genre
                for (int i = 0; i < clbGenres.Items.Count; i++)
                {
                    var genre = (Genre)clbGenres.Items[i];
                    if (genre.GenreID == genreId)
                    {
                        clbGenres.SetItemChecked(i, true);
                        break;
                    }
                }
                txtNewGenre.Clear();
            }

            // Handle selected authors and genres
            var selectedAuthorIds = clbAuthors.CheckedItems.Cast<Author>().Select(a => a.AuthorID).ToList();
            var selectedGenreIds = clbGenres.CheckedItems.Cast<Genre>().Select(g => g.GenreID).ToList();

            // Update book's authors and genres lists (assuming Book model has these)
            _book.BookAuthors = selectedAuthorIds.Select(authorId => new BookAuthor { ISBN = _book.ISBN, AuthorID = (int)authorId }).ToList();
            _book.BookGenres = selectedGenreIds.Select(genreId => new BookGenre { ISBN = _book.ISBN, GenreID = (int)genreId }).ToList();

            bool success;
            if (_isEdit)
            {
                success = DatabaseService.UpdateBook(_book);
            }
            else
            {
                // Need to handle adding authors and genres relationships after adding the book
                DatabaseService.AddBook(_book);
                success = true; // Assuming AddBook is successful for now
            }

            if (success)
            {
                MessageBox.Show("Book saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to save book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter the book title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                MessageBox.Show("Please enter the book ISBN.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
             if (!_isEdit && DatabaseService.GetBookByISBN(txtISBN.Text) != null)
            {
                 MessageBox.Show("Book with this ISBN already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return false;
            }
            if (string.IsNullOrWhiteSpace(txtPublisher.Text))
            {
                MessageBox.Show("Please enter the publisher.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
             if (string.IsNullOrWhiteSpace(txtLanguage.Text))
            {
                MessageBox.Show("Please enter the language.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
} 