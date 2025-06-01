using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using LibraryManagement.Services;
using LibraryManagement.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibraryManagement.GUI
{
    public partial class ReportGenerationForm : Form
    {
        private ComboBox cmbReportType;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Button btnGenerate;
        private Button btnExport;
        private DataGridView dgvReport;
        private Label lblReportType;
        private Label lblDateRange;
        private Label lblStartDate;
        private Label lblEndDate;
        private string _role;

        public ReportGenerationForm(string role)
        {
            _role = role;
            InitializeComponent();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Report Generation";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize controls
            lblReportType = new Label
            {
                Text = "Report Type:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            cmbReportType = new ComboBox
            {
                Location = new Point(100, 17),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.AddRange(new string[]
            {
                "Member Activity Report",
                "Transaction History Report",
                "Fine Collection Report",
                "Book Reviews Report",
                "Fine Payment Report"
            });
            cmbReportType.SelectedIndex = 0; // Default selection
            cmbReportType.SelectedIndexChanged += cmbReportType_SelectedIndexChanged;

            lblDateRange = new Label
            {
                Text = "Date Range:",
                Location = new Point(320, 20),
                AutoSize = true
            };

            lblStartDate = new Label
            {
                Text = "Start Date:",
                Location = new Point(400, 20),
                AutoSize = true
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(460, 17),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            lblEndDate = new Label
            {
                Text = "End Date:",
                Location = new Point(600, 20),
                AutoSize = true
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(660, 17),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(800, 15),
                AutoSize = true
            };
            btnGenerate.Click += btnGenerate_Click;

            btnExport = new Button
            {
                Text = "Export to CSV",
                Location = new Point(920, 15),
                AutoSize = true,
                Enabled = false // Disable initially
            };
            btnExport.Click += btnExport_Click;

            dgvReport = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(960, 500),
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Add controls to form
            this.Controls.Add(lblReportType);
            this.Controls.Add(cmbReportType);
            this.Controls.Add(lblDateRange);
            this.Controls.Add(lblStartDate);
            this.Controls.Add(dtpStartDate);
            this.Controls.Add(lblEndDate);
            this.Controls.Add(dtpEndDate);
            this.Controls.Add(btnGenerate);
            this.Controls.Add(btnExport);
            this.Controls.Add(dgvReport);
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member" || _role == "Guest")
            {
                // Members/Guests should not access reports
                 this.Controls.Clear();
                 var permissionLabel = new Label
                 {
                     Text = "You do not have permission to view reports.",
                     TextAlign = ContentAlignment.MiddleCenter,
                     Dock = DockStyle.Fill,
                     Font = new Font("Segoe UI", 12, FontStyle.Bold)
                 };
                 this.Controls.Add(permissionLabel);
            }
            // Admins have full access, no restrictions needed here.
        }

        private void cmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable date pickers based on report type
            bool dateRangeRequired = cmbReportType.SelectedItem.ToString() != "Book Reviews Report";
            lblDateRange.Visible = dateRangeRequired;
            lblStartDate.Visible = dateRangeRequired;
            dtpStartDate.Visible = dateRangeRequired;
            lblEndDate.Visible = dateRangeRequired;
            dtpEndDate.Visible = dateRangeRequired;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void GenerateReport()
        {
            string reportType = cmbReportType.SelectedItem.ToString();
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date;

            switch (reportType)
            {
                case "Member Activity Report":
                    GenerateMemberActivityReport(startDate, endDate);
                    break;
                case "Transaction History Report":
                    GenerateTransactionHistoryReport(startDate, endDate);
                    break;
                case "Fine Collection Report":
                    GenerateFineCollectionReport(startDate, endDate);
                    break;
                case "Book Reviews Report":
                    GenerateBookReviewsReport();
                    break;
                case "Fine Payment Report":
                    GenerateFinePaymentReport(startDate, endDate);
                    break;
            }

            btnExport.Enabled = dgvReport.Rows.Count > 0;
        }

        private void GenerateMemberActivityReport(DateTime startDate, DateTime endDate)
        {
            var transactions = DatabaseService.GetAllTransactions()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .GroupBy(t => t.MemberID)
                .Select(g => new
                {
                    MemberID = g.Key,
                    MemberName = DatabaseService.GetMemberById(g.Key)?.Name ?? "Unknown",
                    TotalBorrowed = g.Count(),
                    OverdueCount = g.Count(t => t.Status == "Overdue"),
                    ReturnedCount = g.Count(t => t.Status == "Returned")
                }).ToList();

            dgvReport.DataSource = transactions;
        }

        private void GenerateTransactionHistoryReport(DateTime startDate, DateTime endDate)
        {
            var transactions = DatabaseService.GetAllTransactions()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Select(t => new
                {
                    t.TransactionID,
                    BookTitle = DatabaseService.GetBookByISBN(t.ISBN)?.Title ?? "Unknown",
                    MemberName = DatabaseService.GetMemberById(t.MemberID)?.Name ?? "Unknown",
                    t.TransactionDate,
                    t.DueDate,
                    t.ReturnDate,
                    t.Status,
                    DaysOverdue = t.Status == "Overdue" && t.DueDate.HasValue ? 
                        (DateTime.Now - t.DueDate.Value).Days : 0
                }).ToList();

            dgvReport.DataSource = transactions;
        }

        private void GenerateFineCollectionReport(DateTime startDate, DateTime endDate)
        {
            var fines = DatabaseService.GetAllFines()
                .Where(f => f.IssuedDate >= startDate && f.IssuedDate <= endDate)
                .Select(f => new
                {
                    f.FineID,
                    BookTitle = DatabaseService.GetTransactionById(f.TransactionID)?.Book?.Title ?? "Unknown",
                    MemberName = DatabaseService.GetTransactionById(f.TransactionID)?.Member?.Name ?? "Unknown",
                    f.Amount,
                    f.IssuedDate,
                    f.PaymentDate,
                    f.Status,
                    f.Reason
                }).ToList();

            dgvReport.DataSource = fines;
        }

        private void GenerateBookReviewsReport()
        {
            var reviews = DatabaseService.GetAllReviews();
            var reportData = reviews.GroupBy(r => r.ISBN)
                                   .Select(g => new
                                   {
                                       ISBN = g.Key,
                                       BookTitle = DatabaseService.GetBookByISBN(g.Key)?.Title ?? "Unknown",
                                       TotalReviews = g.Count(),
                                       AverageRating = g.Any() ? Math.Round(g.Average(r => r.Rating), 2) : 0
                                   }).ToList();

            dgvReport.DataSource = reportData;
        }

        private void GenerateFinePaymentReport(DateTime startDate, DateTime endDate)
        {
            var fines = DatabaseService.GetAllFines()
                .Where(f => f.PaymentDate.HasValue && f.PaymentDate.Value >= startDate && f.PaymentDate.Value <= endDate)
                .Select(f => new
                {
                    f.FineID,
                     BookTitle = DatabaseService.GetTransactionById(f.TransactionID)?.Book?.Title ?? "Unknown",
                    MemberName = DatabaseService.GetTransactionById(f.TransactionID)?.Member?.Name ?? "Unknown",
                    f.Amount,
                    f.IssuedDate,
                    f.PaymentDate,
                    f.Status,
                    f.Reason
                }).ToList();

            dgvReport.DataSource = fines;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.DataSource == null)
            {
                MessageBox.Show("No data to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExportToCsv(dgvReport, cmbReportType.SelectedItem.ToString());
        }

        private void ExportToCsv(DataGridView dgv, string reportType)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                saveDialog.Title = "Export Report";
                saveDialog.FileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        // Write headers
                        var headers = new List<string>();
                        foreach (DataGridViewColumn column in dgv.Columns)
                        {
                            if (column.Visible)
                            {
                                headers.Add(column.HeaderText);
                            }
                        }
                        writer.WriteLine(string.Join(",", headers));

                        // Write data
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            var values = new List<string>();
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                if (cell.OwningColumn.Visible)
                                {
                                    values.Add(cell.Value?.ToString() ?? "");
                                }
                            }
                            writer.WriteLine(string.Join(",", values));
                        }
                    }
                    MessageBox.Show("Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
} 