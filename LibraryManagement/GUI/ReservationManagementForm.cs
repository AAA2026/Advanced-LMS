using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using LibraryManagement.Models;
using LibraryManagement.Services;

namespace LibraryManagement.GUI
{
    public partial class ReservationManagementForm : Form
    {
        private DataGridView dgvReservations;
        private Button btnCancel;
        private Button btnRefresh;
        private string _role;
        private int? _memberId;

        public ReservationManagementForm(string role, int? memberId = null)
        {
            _role = role;
            _memberId = memberId;
            InitializeComponent();
            LoadReservations();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Text = "Reservation Management";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Height = 40,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White
            };

            btnCancel = new Button
            {
                Text = "Cancel Reservation",
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += BtnCancel_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadReservations();

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnRefresh);
            mainPanel.Controls.Add(buttonPanel, 0, 0);

            // DataGridView
            dgvReservations = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9),
                GridColor = Color.FromArgb(224, 224, 224),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false
            };
            mainPanel.Controls.Add(dgvReservations, 0, 1);
            this.Controls.Add(mainPanel);
        }

        private void ApplyRolePermissions()
        {
            if (_role == "Member" || _role == "Guest")
            {
                // Members can cancel their own reservations, so btnCancel remains visible.
                // btnRefresh is also visible for members.
            }
            else if (_role == "Admin")
            {
                // Admin can also cancel reservations and refresh.
            }
        }

        private void LoadReservations()
        {
            var reservations = DatabaseService.GetAllReservations()
                .Select(r => new
                {
                    r.ReservationID,
                    BookTitle = r.Book?.Title,
                    MemberName = r.Member?.Name,
                    r.ReservationDate,
                    r.Status
                }).ToList();
            dgvReservations.DataSource = reservations;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvReservations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a reservation to cancel.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var reservationId = (int)dgvReservations.SelectedRows[0].Cells["ReservationID"].Value;
            var result = MessageBox.Show("Are you sure you want to cancel this reservation?", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var reservation = DatabaseService.GetAllReservations().FirstOrDefault(r => r.ReservationID == reservationId);
                if (reservation != null)
                {
                    reservation.Status = "Cancelled";
                    DatabaseService.UpdateReservation(reservation);
                    LoadReservations();
                }
            }
        }
    }
} 