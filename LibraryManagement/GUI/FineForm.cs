using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public partial class FineForm : Form
    {
        private readonly Fine _fine;
        private readonly bool _isEdit;
        private TextBox txtAmount;
        private TextBox txtReason;
        private ComboBox cmbTransaction;
        private Button btnSave;
        private Button btnCancel;

        public Fine Fine => _fine;

        public FineForm(Fine? fine = null)
        {
            InitializeComponent();
            _isEdit = fine != null;
            _fine = fine ?? new Fine
            {
                IssuedDate = DateTime.Now,
                Status = "Pending"
            };

            if (_isEdit)
            {
                Text = "Edit Fine";
                txtAmount.Text = _fine.Amount.ToString("F2");
                txtReason.Text = _fine.Reason;
                cmbTransaction.SelectedValue = _fine.TransactionID;
            }
            else
            {
                Text = "Add Fine";
            }

            LoadTransactions();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Create input panel
            var inputPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            var lblTransaction = new Label
            {
                Text = "Transaction:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            cmbTransaction = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            inputPanel.Controls.Add(lblTransaction, 0, 0);
            inputPanel.Controls.Add(cmbTransaction, 1, 0);

            var lblAmount = new Label
            {
                Text = "Amount:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            txtAmount = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };
            inputPanel.Controls.Add(lblAmount, 0, 1);
            inputPanel.Controls.Add(txtAmount, 1, 1);

            var lblReason = new Label
            {
                Text = "Reason:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            txtReason = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };
            inputPanel.Controls.Add(lblReason, 0, 2);
            inputPanel.Controls.Add(txtReason, 1, 2);

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

            mainPanel.Controls.Add(inputPanel, 0, 0);
            mainPanel.SetRowSpan(inputPanel, 2);
            mainPanel.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(mainPanel);

            // Wire up events
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadTransactions()
        {
            var transactions = DatabaseService.GetAllTransactions()
                .Where(t => t.Status != "Returned")
                .ToList();
            cmbTransaction.DataSource = transactions;
            cmbTransaction.DisplayMember = "TransactionID";
            cmbTransaction.ValueMember = "TransactionID";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            _fine.Amount = decimal.Parse(txtAmount.Text);
            _fine.Reason = txtReason.Text;
            _fine.TransactionID = (int)cmbTransaction.SelectedValue;

            if (!_isEdit)
            {
                DatabaseService.AddFine(_fine);
            }
            else
            {
                DatabaseService.UpdateFine(_fine);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out _))
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Please enter a reason for the fine.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cmbTransaction.SelectedValue == null)
            {
                MessageBox.Show("Please select a transaction.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
} 