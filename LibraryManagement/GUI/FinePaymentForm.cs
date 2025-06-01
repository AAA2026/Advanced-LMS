using System;
using System.Windows.Forms;
using LibraryManagement.Models;
using LibraryManagement.Services;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagement.GUI
{
    public partial class FinePaymentForm : Form
    {
        private int _memberId;
        private DataGridView dgvFines;

        public FinePaymentForm(int memberId)
        {
            InitializeComponent();
            _memberId = memberId;
            LoadMemberFines();
        }

        private void InitializeComponent()
        {
            this.Text = "Pay Fines";
            this.Size = new System.Drawing.Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvFines = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = System.Drawing.Color.FromArgb(245, 245, 245) }
            };
            dgvFines.CellContentClick += DgvFines_CellContentClick;

            // Add a Pay button column
            var payButtonColumn = new DataGridViewButtonColumn();
            payButtonColumn.Name = "Pay";
            payButtonColumn.HeaderText = "Pay";
            payButtonColumn.Text = "Pay";
            payButtonColumn.UseColumnTextForButtonValue = true;
            dgvFines.Columns.Add(payButtonColumn);

            this.Controls.Add(dgvFines);
        }

        private void LoadMemberFines()
        {
            List<Fine> memberFines = DatabaseService.GetFinesByMemberId(_memberId);
            // Filter for unpaid fines
            var unpaidFines = memberFines.Where(f => f.Status == "Unpaid").ToList();
            dgvFines.DataSource = unpaidFines;
        }

        private void DgvFines_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked cell is in the Pay button column
            if (dgvFines.Columns[e.ColumnIndex].Name == "Pay" && e.RowIndex >= 0)
            {
                // Get the Fine object for the clicked row
                var fineToPay = dgvFines.Rows[e.RowIndex].DataBoundItem as Fine;

                if (fineToPay != null)
                {
                    // Implement payment logic (e.g., confirm payment)
                    DialogResult result = MessageBox.Show($"Mark fine of {fineToPay.Amount:C} as paid?", "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        fineToPay.PaymentDate = DateTime.Now;
                        fineToPay.Status = "Paid";
                        DatabaseService.UpdateFine(fineToPay);
                        MessageBox.Show("Fine marked as paid.", "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMemberFines(); // Refresh the fines list
                    }
                }
            }
        }
    }
} 