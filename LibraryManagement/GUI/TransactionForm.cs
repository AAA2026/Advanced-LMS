using System;
using System.Windows.Forms;
using System.Linq;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.ComponentModel;

namespace LibraryManagement.GUI
{
    public partial class TransactionForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Transaction Transaction { get; private set; }
        private ComboBox cmbBook;
        private ComboBox cmbMember;
        private DateTimePicker dtpDueDate;
        private Button btnSave;
        private Button btnCancel;

        public TransactionForm()
        {
            Transaction = new Transaction
            {
                TransactionDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Active"
            };
            InitializeComponent();
            LoadData();
            LoadTransactionData();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Transaction";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            // Add controls to main panel
            mainPanel.Controls.Add(CreateLabel("Book:"), 0, 0);
            cmbBook = CreateComboBox();
            mainPanel.Controls.Add(cmbBook, 1, 0);

            mainPanel.Controls.Add(CreateLabel("Member:"), 0, 1);
            cmbMember = CreateComboBox();
            mainPanel.Controls.Add(cmbMember, 1, 1);

            mainPanel.Controls.Add(CreateLabel("Due Date:"), 0, 2);
            dtpDueDate = CreateDateTimePicker();
            mainPanel.Controls.Add(dtpDueDate, 1, 2);

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

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Wire up events
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private DateTimePicker CreateDateTimePicker()
        {
            return new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void LoadData()
        {
            // Load books
            var books = DatabaseService.GetAllBooks().ToList();
            cmbBook.DisplayMember = "Title";
            cmbBook.ValueMember = "ISBN";
            cmbBook.DataSource = books;

            // Load members
            var members = DatabaseService.GetAllMembers().ToList();
            cmbMember.DisplayMember = "Name";
            cmbMember.ValueMember = "MemberID";
            cmbMember.DataSource = members;
        }

        private void LoadTransactionData()
        {
            // This form is for adding new transactions only.
            // Editing existing transactions is handled in TransactionManagementForm.
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                Transaction.ISBN = cmbBook.SelectedValue.ToString();
                Transaction.MemberID = (int)cmbMember.SelectedValue;
                Transaction.DueDate = dtpDueDate.Value;

                DatabaseService.AddTransaction(Transaction);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInput()
        {
            if (cmbBook.SelectedValue == null)
            {
                MessageBox.Show("Please select a book.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbBook.Focus();
                return false;
            }
            if (cmbMember.SelectedValue == null)
            {
                MessageBox.Show("Please select a member.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbMember.Focus();
                return false;
            }
            return true;
        }
    }
} 